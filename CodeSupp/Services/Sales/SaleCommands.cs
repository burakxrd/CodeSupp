using CodeSupp.Data;
using CodeSupp.Dtos;
using CodeSupp.Enums;
using CodeSupp.Exceptions;
using CodeSupp.Models;
using CodeSupp.Services.Infrastructure;
using CodeSupp.ViewModels;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CodeSupp.Services.Sales
{
    public class SaleCommands : ISaleCommands
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<CreateSaleViewModel> _validator;
        private readonly ILogger<SaleCommands> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;

        public SaleCommands(
            ApplicationDbContext context,
            IValidator<CreateSaleViewModel> validator,
            ILogger<SaleCommands> logger,
            IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _validator = validator;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
        }

        private async Task<string> GenerateOrderCode()
        {
            var today = _dateTimeProvider.UtcNow.Date;
            var countToday = await _context.Sales
                .Where(s => s.SaleDate.Date == today)
                .CountAsync();
            return $"{today:yyMMdd}-{countToday + 1}";
        }

        public async Task<Sale> CreateSaleAsync(CreateSaleViewModel model)
        {
            var validation = await _validator.ValidateAsync(model);
            if (!validation.IsValid) throw new ValidationException(validation.Errors);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Ürün Kontrolü
                var productIds = model.Items.Select(i => i.ProductId).Distinct().ToList();
                // [FIX] PostgreSQL hatasını önlemek için ürünleri normal EF Core ile çekiyoruz
                var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();
                var productMap = products.ToDictionary(p => p.Id);

                foreach (var id in productIds)
                    if (!productMap.ContainsKey(id)) throw new NotFoundException("Product", id);

                // 2. Satış Nesnesi
                var sale = new Sale
                {
                    CustomerId = model.CustomerId,
                    SaleDate = model.SaleDate,
                    CreatedAt = _dateTimeProvider.UtcNow,
                    OrderCode = await GenerateOrderCode(),
                    ExternalReference = model.ExternalReference,
                    ShippingCost = model.ShippingCost,
                    PlatformCommission = model.PlatformCommission,
                    ManualDiscount = model.ManualDiscount,
                    ShippingStatus = ShippingStatus.SiparisAlindi
                };

                decimal totalProductAmount = 0;

                // 3. Stok Düşme ve Kalemleri Ekleme (negatif stok özellik olarak kabul ediliyor; frontend uyarı veriyor)
                foreach (var item in model.Items)
                {
                    if (productMap.TryGetValue(item.ProductId, out var product))
                    {
                        product.Stock -= item.Quantity;
                    }

                    decimal lineTotal = item.Quantity * item.UnitPrice;
                    totalProductAmount += lineTotal;

                    sale.SaleItems.Add(new SaleItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = lineTotal
                    });
                }

                // Toplam Tutar
                sale.TotalAmount = Math.Max(0, totalProductAmount + model.ShippingCost - model.ManualDiscount);

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                // [TEMİZLİK] Buradaki "Otomatik Payment" kodunu sildik. 
                // Artık sipariş "Ödendi" olarak gelmeyecek.

                await transaction.CommitAsync();
                return sale;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Satış oluşturulamadı.");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<SalesProcessResult> CreateBulkSalesAsync(BulkSaleRequestDto request)
        {
            var result = new SalesProcessResult();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var models = request.Items;
                var customers = await _context.Customers.Where(c => models.Select(m => m.CustomerName).Contains(c.Name)).ToListAsync();
                var allProductIds = models.SelectMany(m => m.Products ?? new List<BulkSaleProductDetail>()).Select(p => p.ProductId).Distinct().ToList();

                // [FIX] AsNoTracking kaldırdık çünkü stok düşmemiz lazım
                var allProducts = await _context.Products.Where(p => allProductIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

                foreach (var order in models)
                {
                    if (order.Products == null || !order.Products.Any()) { result.FailCount++; result.Errors.Add($"{order.CustomerName}: Ürün yok."); continue; }

                    var customer = customers.FirstOrDefault(c => c.Name == (order.CustomerName ?? "Misafir"));
                    if (customer == null)
                    {
                        customer = new Customer { Name = order.CustomerName ?? "Misafir", Phone = order.Phone, Address = order.Address, CreatedAt = _dateTimeProvider.UtcNow };
                        _context.Customers.Add(customer);
                        await _context.SaveChangesAsync();
                        customers.Add(customer);
                    }

                    var sale = new Sale
                    {
                        CustomerId = customer.Id,
                        SaleDate = _dateTimeProvider.UtcNow,
                        CreatedAt = _dateTimeProvider.UtcNow,
                        OrderCode = await GenerateOrderCode(),
                        ShippingStatus = ShippingStatus.SiparisAlindi
                    };

                    decimal totalAmount = 0;
                    foreach (var item in order.Products)
                    {
                        if (!allProducts.TryGetValue(item.ProductId, out var product)) continue;

                        product.Stock -= item.Quantity;
                        decimal lineTotal = item.Quantity * item.UnitPrice;
                        totalAmount += lineTotal;
                        sale.SaleItems.Add(new SaleItem { ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = item.UnitPrice, TotalPrice = lineTotal });
                    }
                    sale.TotalAmount = totalAmount;
                    _context.Sales.Add(sale);
                    result.SuccessCount++;
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Toplu işlem hatası");
                throw;
            }
            return result;
        }

        public async Task<Sale> UpdateSaleAsync(int id, EditSaleViewModel model)
        {
            var sale = await _context.Sales.Include(s => s.SaleItems).ThenInclude(si => si.Product).FirstOrDefaultAsync(s => s.Id == id);
            if (sale == null) throw new NotFoundException("Sale", id);

            if (model.RowVersion != null && sale.RowVersion != null)
            {
                _context.Entry(sale).OriginalValues["RowVersion"] = model.RowVersion;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Eski stokları geri ver
                foreach (var item in sale.SaleItems)
                    if (item.Product != null) item.Product.Stock += item.Quantity;

                _context.SaleItems.RemoveRange(sale.SaleItems);

                var newItems = new List<SaleItem>();
                decimal totalAmount = 0;

                foreach (var newItem in model.Items)
                {
                    var product = await _context.Products.FindAsync(newItem.ProductId);
                    if (product == null) throw new NotFoundException("Product", newItem.ProductId);

                    product.Stock -= newItem.Quantity;
                    decimal lineTotal = newItem.Quantity * newItem.UnitPrice;
                    totalAmount += lineTotal;

                    newItems.Add(new SaleItem { ProductId = newItem.ProductId, Quantity = newItem.Quantity, UnitPrice = newItem.UnitPrice, TotalPrice = lineTotal });
                }

                sale.CustomerId = model.CustomerId;
                sale.SaleDate = model.SaleDate;
                sale.TotalAmount = Math.Max(0, totalAmount - model.ManualDiscount);
                sale.SaleItems = newItems;
                sale.ShippingCost = model.ShippingCost;
                sale.PlatformCommission = model.PlatformCommission;
                sale.ManualDiscount = model.ManualDiscount;
                sale.ExternalReference = model.ExternalReference;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return sale;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                throw new ConflictException("Bu kayıt başka bir kullanıcı tarafından güncellendi. Lütfen sayfayı yenileyip tekrar deneyin.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteSaleAsync(int id)
        {
            var sale = await _context.Sales.Include(s => s.SaleItems).ThenInclude(si => si.Product).FirstOrDefaultAsync(s => s.Id == id);
            if (sale == null) throw new NotFoundException("Sale", id);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Stokları iade et
                foreach (var item in sale.SaleItems)
                    if (item.Product != null) item.Product.Stock += item.Quantity;

                _context.Sales.Remove(sale);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch { await transaction.RollbackAsync(); throw; }
        }

        public async Task<Sale> UpdateShippingStatusAsync(int saleId, UpdateShippingStatusDto dto)
        {
            var sale = await _context.Sales.FirstOrDefaultAsync(s => s.Id == saleId);
            if (sale == null) throw new NotFoundException("Sale", saleId);

            sale.ShippingStatus = dto.Status;
            await _context.SaveChangesAsync();
            return sale;
        }
    }
}