using AutoMapper;
using CodeSupp.Data;
using CodeSupp.Dtos;
using CodeSupp.Enums;
using CodeSupp.Models;
using CodeSupp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CodeSupp.Services.Sales
{
    public class SaleQueries : ISaleQueries
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SaleQueries(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<SaleListViewModel>> GetSalesAsync(SaleFilterDto filter)
        {
            var query = _context.Sales
                .AsNoTracking()
                .Include(s => s.Customer)
                .Include(s => s.SaleItems).ThenInclude(si => si.Product)
                .Include(s => s.Payments)
                .AsQueryable();

            if (filter.CustomerId.HasValue)
            {
                query = query.Where(s => s.CustomerId == filter.CustomerId.Value);
            }

            // Arama Filtresi (ToUpper ile case-insensitive - PostgreSQL collation bağımsız)
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string searchUpper = filter.Search.Trim().ToUpperInvariant();

                // Türkçe karakter normalizasyonu (SearchText için)
                string normalizedSearch = filter.Search.Trim()
                    .Replace("İ", "i").Replace("ı", "i")
                    .Replace("Ö", "o").Replace("ö", "o")
                    .Replace("Ü", "u").Replace("ü", "u")
                    .Replace("Ş", "s").Replace("ş", "s")
                    .Replace("Ğ", "g").Replace("ğ", "g")
                    .Replace("Ç", "c").Replace("ç", "c")
                    .ToLowerInvariant();

                bool isNumeric = int.TryParse(filter.Search.Trim(), out int searchId);

                query = query.Where(s =>
                    // Müşteri adı
                    s.Customer.Name.ToUpper().Contains(searchUpper) ||

                    // Ürün arama
                    s.SaleItems.Any(si =>
                        (si.Product.SearchText != null && si.Product.SearchText.Contains(normalizedSearch)) ||
                        si.Product.Name.ToUpper().Contains(searchUpper)
                    ) ||

                    // ID
                    (isNumeric && s.Id == searchId) ||

                    // OrderCode
                    (s.OrderCode != null && s.OrderCode.ToUpper().Contains(searchUpper)) ||

                    // ExternalReference
                    (s.ExternalReference != null && s.ExternalReference.ToUpper().Contains(searchUpper))
                );
            }

            if (filter.StartDate.HasValue)
                query = query.Where(s => s.SaleDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
            {
                var end = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(s => s.SaleDate <= end);
            }

            if (!string.IsNullOrEmpty(filter.ShippingStatus) && filter.ShippingStatus != "Tümü")
            {
                ShippingStatus? statusEnum = filter.ShippingStatus switch
                {
                    "Sipariş Alındı" => ShippingStatus.SiparisAlindi,
                    "Hazırlanıyor" => ShippingStatus.Hazirlaniyor,
                    "Kargoya Verildi" => ShippingStatus.KargoyaVerildi,
                    "Teslim Edildi" => ShippingStatus.TeslimEdildi,
                    "İptal" => ShippingStatus.IptalEdildi,
                    "İade" => ShippingStatus.IadeEdildi,
                    _ => null
                };

                if (statusEnum.HasValue)
                    query = query.Where(s => s.ShippingStatus == statusEnum.Value);
            }

            var projectedQuery = query.Select(s => new
            {
                Sale = s,
                PaidAmount = s.Payments.Sum(p => p.Amount),
                Remaining = s.TotalAmount - s.Payments.Sum(p => p.Amount)
            });

            if (!string.IsNullOrEmpty(filter.PaymentStatus) && filter.PaymentStatus != "Tümü")
            {
                if (filter.PaymentStatus == "Tamamlandı")
                    projectedQuery = projectedQuery.Where(x => x.Remaining <= 0.01m);
                else if (filter.PaymentStatus == "Bekliyor")
                    projectedQuery = projectedQuery.Where(x => x.Remaining > 0.01m);
            }

            bool isAsc = filter.SortDir == "asc";
            projectedQuery = filter.SortBy switch
            {
                "total" => isAsc ? projectedQuery.OrderBy(x => x.Sale.TotalAmount) : projectedQuery.OrderByDescending(x => x.Sale.TotalAmount),
                "collected" => isAsc ? projectedQuery.OrderBy(x => x.PaidAmount) : projectedQuery.OrderByDescending(x => x.PaidAmount),
                "remaining" => isAsc ? projectedQuery.OrderBy(x => x.Remaining) : projectedQuery.OrderByDescending(x => x.Remaining),
                _ => projectedQuery.OrderByDescending(x => x.Sale.SaleDate)
            };

            var totalCount = await projectedQuery.CountAsync();

            var pagedData = await projectedQuery
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var saleEntities = pagedData.Select(x => x.Sale).ToList();
            var viewModels = _mapper.Map<List<SaleListViewModel>>(saleEntities);

            return new PaginatedResult<SaleListViewModel>(viewModels, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<Sale?> GetSaleByIdAsync(int id)
        {
            return await _context.Sales
                .AsNoTracking()
                .Include(s => s.Customer)
                .Include(s => s.SaleItems).ThenInclude(si => si.Product)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<CreateSaleDataViewModel> GetCreateSaleDataAsync()
        {
            var products = await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new ProductInfoForSaleViewModel
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Stock = p.Stock,
                    Price = p.Price
                })
                .ToListAsync();

            var customers = await _context.Customers
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            return new CreateSaleDataViewModel
            {
                Customers = customers,
                Products = products
            };
        }
    }
}