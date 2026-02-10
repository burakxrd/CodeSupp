using CodeSupp.Data;
using CodeSupp.Models;
using CodeSupp.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CodeSupp.Services.Products
{
    public class PurchaseService : IPurchaseService
    {
        private readonly ApplicationDbContext _context;

        public PurchaseService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ----------------------------------------------------------------------------------
        // READ OPERATIONS (OKUMA)
        // ----------------------------------------------------------------------------------

        /// <summary>
        /// Satın alma geçmişini sayfalı olarak listeler.
        /// </summary>
        public async Task<PaginatedResult<ProductPurchaseViewModel>> GetPurchaseHistoryAsync(int page, int pageSize)
        {
            // Global Tenant Filter devrede
            var query = _context.ProductPurchaseHistories
                .AsNoTracking()
                .Include(h => h.Product)
                .OrderByDescending(h => h.PurchaseDate)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var history = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new ProductPurchaseViewModel
                {
                    Id = h.Id,
                    ProductId = h.ProductId,
                    QuantityInUnits = h.Quantity,
                    ProductPricePerUnit = h.ProductPricePerUnit,
                    TotalKg = h.TotalKg,
                    ShippingCostPerKg = h.ShippingCostPerKg,
                    PurchaseDate = h.PurchaseDate,
                    TotalCost = h.TotalCost,
                    Description = h.Description,
                    Product = new ProductIndexViewModel
                    {
                        Id = h.Product.Id,
                        Name = h.Product.Name,
                        Code = h.Product.Code,
                        ImagePath = h.Product.ImagePath
                    }
                })
                .ToListAsync();

            return new PaginatedResult<ProductPurchaseViewModel>(history, totalCount, page, pageSize);
        }

        public async Task<ProductPurchaseViewModel?> GetPurchaseByIdAsync(int id)
        {
            // FirstOrDefaultAsync ile Global Filter garantisi
            var h = await _context.ProductPurchaseHistories
                .AsNoTracking()
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (h == null) return null;

            // Dropdown listesini hazırlıyoruz (Sadece Tenant'ın ürünleri gelir)
            var products = await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();

            return new ProductPurchaseViewModel
            {
                Id = h.Id,
                ProductId = h.ProductId,
                QuantityInUnits = h.Quantity,
                ProductPricePerUnit = h.ProductPricePerUnit,
                TotalKg = h.TotalKg,
                ShippingCostPerKg = h.ShippingCostPerKg,
                PurchaseDate = h.PurchaseDate,
                TotalCost = h.TotalCost,
                Description = h.Description,
                Products = new SelectList(products, "Id", "Name", h.ProductId)
            };
        }

        public async Task<ProductPurchaseViewModel> GetCreatePurchaseDataAsync()
        {
            // Global Tenant Filter devrede
            var products = await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();

            return new ProductPurchaseViewModel
            {
                Products = new SelectList(products, "Id", "Name")
            };
        }

        // ----------------------------------------------------------------------------------
        // WRITE OPERATIONS (YAZMA - TRANSACTIONAL)
        // ----------------------------------------------------------------------------------

        public async Task<ProductPurchaseHistory> CreatePurchaseAsync(ProductPurchaseViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Ürünü Bul
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == model.ProductId);
                if (product == null) throw new KeyNotFoundException("Seçilen ürün bulunamadı.");

                // 2. Satın Alma Geçmişi Nesnesi
                var history = new ProductPurchaseHistory
                {
                    ProductId = model.ProductId,
                    PurchaseDate = model.PurchaseDate.HasValue
                        ? model.PurchaseDate.Value.Date.Add(DateTime.Now.TimeOfDay)
                        : DateTime.Now,
                    Quantity = model.QuantityInUnits,
                    ProductPricePerUnit = model.ProductPricePerUnit,
                    TotalKg = model.TotalKg,
                    ShippingCostPerKg = model.ShippingCostPerKg,
                    Description = model.Description
                };
                history.CalculateCosts();

                // 3. Gider (Expense) Nesnesi
                var expense = new Expense
                {
                    CreatedAt = DateTime.UtcNow,
                    Date = history.PurchaseDate,
                    Amount = history.TotalCost,
                    Description = !string.IsNullOrEmpty(model.Description)
                        ? $"{product.Name} Alımı - {model.Description}"
                        : $"Stok Alımı: {product.Name} ({model.QuantityInUnits} Adet)",
                    ProductPurchaseHistory = history
                };

                // 4. Stok Güncelleme
                product.Stock += model.QuantityInUnits;

                // 5. Context'e Ekleme
                _context.Expenses.Add(expense);
                _context.Products.Update(product);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return history;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdatePurchaseAsync(int id, ProductPurchaseViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var history = await _context.ProductPurchaseHistories
                    .Include(p => p.Product)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (history == null) throw new KeyNotFoundException("Kayıt bulunamadı.");

                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.ProductPurchaseHistoryId == id);

                // Ürün değişikliği kontrolü
                var newProduct = (history.ProductId == model.ProductId)
                    ? history.Product
                    : await _context.Products.FirstOrDefaultAsync(p => p.Id == model.ProductId);

                if (newProduct == null) throw new KeyNotFoundException("Yeni seçilen ürün bulunamadı.");

                // --- STOK MANTIĞI ---
                if (history.ProductId == newProduct.Id)
                {
                    int difference = model.QuantityInUnits - history.Quantity;
                    newProduct.Stock += difference;
                }
                else
                {
                    // Eski ürünün stoğunu iade et
                    history.Product.Stock -= history.Quantity;
                    // Yeni ürünün stoğunu arttır
                    newProduct.Stock += model.QuantityInUnits;

                    _context.Products.Update(history.Product);
                }

                // --- HISTORY GÜNCELLEME ---
                history.ProductId = model.ProductId;
                history.Quantity = model.QuantityInUnits;
                history.ProductPricePerUnit = model.ProductPricePerUnit;
                history.TotalKg = model.TotalKg;
                history.ShippingCostPerKg = model.ShippingCostPerKg;
                history.Description = model.Description;
                if (model.PurchaseDate.HasValue)
                    history.PurchaseDate = model.PurchaseDate.Value.Date.Add(DateTime.Now.TimeOfDay);

                history.CalculateCosts();

                // --- EXPENSE GÜNCELLEME ---
                if (expense != null)
                {
                    expense.Amount = history.TotalCost;
                    expense.Date = history.PurchaseDate;
                    expense.Description = !string.IsNullOrEmpty(model.Description)
                        ? $"{newProduct.Name} Alımı - {model.Description}"
                        : $"Stok Alımı: {newProduct.Name} ({model.QuantityInUnits} Adet)";
                    _context.Expenses.Update(expense);
                }

                _context.ProductPurchaseHistories.Update(history);
                _context.Products.Update(newProduct);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeletePurchaseAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var history = await _context.ProductPurchaseHistories
                    .Include(p => p.Product)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (history == null) throw new KeyNotFoundException("Silinecek kayıt bulunamadı.");

                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.ProductPurchaseHistoryId == id);

                // 1. Gideri Sil
                if (expense != null)
                {
                    _context.Expenses.Remove(expense);
                }

                // 2. Stoğu Geri Al
                if (history.Product != null)
                {
                    history.Product.Stock -= history.Quantity;
                    _context.Products.Update(history.Product);
                }

                // 3. Geçmiş Kaydını Sil
                _context.ProductPurchaseHistories.Remove(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ----------------------------------------------------------------------------------
        // BULK OPERATION (TOPLU EKLEME)
        // ----------------------------------------------------------------------------------

        public async Task<(bool IsSuccess, string Message, int SuccessCount)> CreateBulkPurchaseAsync(List<ProductPurchaseViewModel> models)
        {
            if (models == null || !models.Any()) return (false, "Liste boş.", 0);

            // Lookup Dictionary (Tenant filtresi global olarak devrede)
            var productIds = models.Select(m => m.ProductId).Distinct().ToList();
            var productsDict = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int successCount = 0;

                foreach (var model in models)
                {
                    if (model.QuantityInUnits <= 0) continue;
                    if (!productsDict.TryGetValue(model.ProductId, out var product)) continue;

                    var history = new ProductPurchaseHistory
                    {
                        ProductId = model.ProductId,
                        PurchaseDate = model.PurchaseDate.HasValue
                            ? model.PurchaseDate.Value.Date.Add(DateTime.Now.TimeOfDay)
                            : DateTime.Now,
                        Quantity = model.QuantityInUnits,
                        ProductPricePerUnit = model.ProductPricePerUnit,
                        TotalKg = model.TotalKg,
                        ShippingCostPerKg = model.ShippingCostPerKg,
                        Description = model.Description ?? "Toplu Alım"
                    };
                    history.CalculateCosts();

                    var expense = new Expense
                    {
                        CreatedAt = DateTime.UtcNow,
                        Date = history.PurchaseDate,
                        Amount = history.TotalCost,
                        Description = $"Toplu Alım: {product.Name} ({model.QuantityInUnits} Adet)",
                        ProductPurchaseHistory = history
                    };

                    product.Stock += model.QuantityInUnits;

                    _context.Expenses.Add(expense);
                }

                successCount = models.Count;

                if (successCount > 0)
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return (true, $"{successCount} kalem ürün başarıyla işlendi.", successCount);
                }
                else
                {
                    return (false, "İşlenecek geçerli kayıt bulunamadı.", 0);
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Toplu işlem hatası: {ex.Message}", 0);
            }
        }
    }
}