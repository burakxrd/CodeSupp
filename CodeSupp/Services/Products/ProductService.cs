using CodeSupp.Data;
using CodeSupp.Enums;
using CodeSupp.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Globalization;

namespace CodeSupp.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ürün isminden akıllı ve benzersiz bir stok kodu (SKU) üretir.
        /// Örn: "Kablosuz Mouse" -> "KM101", "KM102"...
        /// </summary>
        public async Task<string> GenerateSmartCodeAsync(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName)) return "X101";

            var culture = new CultureInfo("tr-TR");
            // Sadece harf ve rakamları bırak
            var cleanName = Regex.Replace(productName, "[^a-zA-Z0-9 ]", "");
            var words = cleanName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string prefix = "";
            if (words.Length > 0)
            {
                foreach (var word in words)
                {
                    prefix += word[0].ToString().ToUpper(culture);
                }
            }

            if (string.IsNullOrEmpty(prefix)) prefix = "UR";

            // Global Query Filter devrede, sadece kendi tenantımızın kodlarına bakarız.
            var lastCode = await _context.Products
                .Where(p => p.Code != null && p.Code.StartsWith(prefix))
                .OrderByDescending(p => p.Code!.Length)
                .ThenByDescending(p => p.Code)
                .Select(p => p.Code)
                .FirstOrDefaultAsync();

            int nextNumber = 101;

            if (lastCode != null)
            {
                string numberPart = lastCode.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int lastVal))
                {
                    nextNumber = lastVal + 1;
                }
            }
            return prefix + nextNumber;
        }

        /// <summary>
        /// Stok miktarını manuel olarak düzeltir ve geçmişe log atar.
        /// </summary>
        public async Task<int> AdjustStockAsync(int productId, int adjustmentAmount, string reason, decimal? specificCost = null)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) throw new KeyNotFoundException("Ürün bulunamadı.");

            var oldStock = product.Stock;
            var newStock = oldStock + adjustmentAmount;

            if (newStock < 0)
                throw new InvalidOperationException($"Stok düzeltme işlemi stoğu 0'ın altına düşüremez. Mevcut: {oldStock}, Düşülen: {Math.Abs(adjustmentAmount)}");

            decimal unitCost = specificCost ?? product.CostPrice;

            product.Stock = newStock;

            var history = new ProductPurchaseHistory
            {
                ProductId = product.Id,
                PurchaseDate = DateTime.UtcNow,
                Quantity = adjustmentAmount,
                ProductPricePerUnit = unitCost,
                TotalCost = adjustmentAmount * unitCost,
                Description = $"[DÜZELTME] {reason}"
            };

            _context.ProductPurchaseHistories.Add(history);
            await _context.SaveChangesAsync();

            return newStock;
        }

        /// <summary>
        /// Verilen ürünlerin ortalama maliyetlerini hesaplar.
        /// </summary>
        public async Task<Dictionary<int, decimal>> CalculateAverageCostsAsync(List<int> productIds)
        {
            if (productIds == null || !productIds.Any()) return new Dictionary<int, decimal>();

            var costs = await _context.ProductPurchaseHistories
                .Where(h => productIds.Contains(h.ProductId))
                .GroupBy(h => h.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalCost = g.Sum(x => x.TotalCost),
                    TotalQty = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            var result = new Dictionary<int, decimal>();

            foreach (var item in costs)
            {
                decimal avgCost = item.TotalQty > 0
                    ? item.TotalCost / item.TotalQty
                    : (await _context.Products
                        .Where(p => p.Id == item.ProductId)
                        .Select(p => p.CostPrice)
                        .FirstOrDefaultAsync());

                result.Add(item.ProductId, avgCost);
            }

            foreach (var id in productIds)
            {
                if (!result.ContainsKey(id)) result.Add(id, 0);
            }

            return result;
        }

        public async Task AddOpeningStockLogAsync(int productId, int quantity, decimal unitCost)
        {
            if (quantity <= 0) return;

            var history = new ProductPurchaseHistory
            {
                ProductId = productId,
                PurchaseDate = DateTime.UtcNow,
                Quantity = quantity,
                ProductPricePerUnit = unitCost,
                TotalCost = quantity * unitCost,
                TotalKg = 0,
                ShippingCostPerKg = 0,
                Description = "Açılış Stoğu / Devir"
            };

            _context.ProductPurchaseHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            ValidateProductPrices(product);

            // [NORMALIZATION] Arama metnini oluştur (Türkçe -> İngilizce)
            product.SearchText = PrepareSearchText(product.Name, product.Code);

            if (product.CategoryId.HasValue)
            {
                var category = await _context.Categories.FindAsync(product.CategoryId.Value);
                if (category != null && category.Type == CategoryType.Clothing)
                {
                    product.Type = ProductType.Physical;
                }
            }

            if (string.IsNullOrEmpty(product.Code))
            {
                product.Code = await GenerateSmartCodeAsync(product.Name);
                // Kod sonradan üretildiği için SearchText'i güncelle
                product.SearchText = PrepareSearchText(product.Name, product.Code);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            if (product.Stock > 0)
            {
                await AddOpeningStockLogAsync(product.Id, product.Stock, product.CostPrice);
            }

            return product;
        }

        public async Task UpdateProductAsync(Product product)
        {
            ValidateProductPrices(product);

            // [NORMALIZATION] Arama metnini güncelle
            product.SearchText = PrepareSearchText(product.Name, product.Code);

            if (product.CategoryId.HasValue)
            {
                var category = await _context.Categories.FindAsync(product.CategoryId.Value);
                if (category != null && category.Type == CategoryType.Clothing)
                {
                    product.Type = ProductType.Physical;
                }
            }

            if (string.IsNullOrWhiteSpace(product.Code))
            {
                product.Code = await GenerateSmartCodeAsync(product.Name);
                product.SearchText = PrepareSearchText(product.Name, product.Code);
            }

            try
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new Exception("Bu ürün siz işlem yaparken başka biri tarafından güncellendi. Lütfen sayfayı yenileyip tekrar deneyin.");
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                try
                {
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    throw new InvalidOperationException($"'{product.Name}' adlı ürün geçmişte işlem gördüğü için (Satış veya Satın Alma) fiziksel olarak silinemez.");
                }
            }
        }

        private void ValidateProductPrices(Product product)
        {
            if (product.DiscountedPrice.HasValue && product.DiscountedPrice.Value >= product.Price)
            {
                throw new InvalidOperationException($"İndirimli fiyat ({product.DiscountedPrice.Value}), satış fiyatından ({product.Price}) büyük veya ona eşit olamaz.");
            }
        }

        /// <summary>
        /// Ürün adını ve kodunu alıp, Türkçe karakterlerden arındırılmış ve küçültülmüş bir arama metni oluşturur.
        /// Örnek: "Örnek Ürün" -> "ornek urun"
        /// </summary>
        private string PrepareSearchText(string name, string? code)
        {
            var text = (name + " " + (code ?? "")).Trim();
            if (string.IsNullOrEmpty(text)) return "";

            // Manuel Replace ile en garantisi (Veritabanı Dilinden Bağımsız)
            text = text.Replace("İ", "i").Replace("ı", "i")
                       .Replace("Ö", "o").Replace("ö", "o")
                       .Replace("Ü", "u").Replace("ü", "u")
                       .Replace("Ş", "s").Replace("ş", "s")
                       .Replace("Ğ", "g").Replace("ğ", "g")
                       .Replace("Ç", "c").Replace("ç", "c");

            return text.ToLowerInvariant();
        }
    }
}