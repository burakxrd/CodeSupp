using CodeSupp.Data;
using CodeSupp.Models;
using CodeSupp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CodeSupp.Services.Customers
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Müşterileri sayfalı ve filtreli şekilde listeler.
        /// [GÜNCELLEME] ToUpper() ile case-insensitive arama (PostgreSQL collation bağımsız)
        /// </summary>
        public async Task<PaginatedResult<CustomerListViewModel>> GetCustomersAsync(int pageNumber = 1, int pageSize = 10, string? statusFilter = null, string? searchTerm = null)
        {
            var query = _context.Customers
                .AsNoTracking()
                .AsQueryable();

            // 1. Arama Filtresi (ToUpper ile case-insensitive)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchUpper = searchTerm.Trim().ToUpperInvariant();

                query = query.Where(c =>
                    c.Name.ToUpper().Contains(searchUpper) ||
                    (c.Phone != null && c.Phone.Contains(searchTerm.Trim()))
                );
            }

            // 2. Statü Filtresi
            if (!string.IsNullOrEmpty(statusFilter))
            {
                statusFilter = statusFilter.ToLower();
                var today = DateTime.UtcNow.Date;

                if (statusFilter == "vip")
                {
                    query = query.Where(c => c.Sales.Sum(s => s.TotalAmount) >= 20000 || c.Sales.Count() >= 10);
                }
                else if (statusFilter == "sadik")
                {
                    query = query.Where(c =>
                        !(c.Sales.Sum(s => s.TotalAmount) >= 20000 || c.Sales.Count() >= 10) &&
                        c.Sales.Count() >= 3);
                }
                else if (statusFilter == "yeni")
                {
                    var thirtyDaysAgo = today.AddDays(-30);
                    query = query.Where(c => c.Sales.Any() && c.Sales.Min(s => s.SaleDate) >= thirtyDaysAgo);
                }
                else if (statusFilter == "riskli")
                {
                    var start = today.AddDays(-75);
                    var end = today.AddDays(-45);
                    query = query.Where(c => c.Sales.Max(s => s.SaleDate) >= start && c.Sales.Max(s => s.SaleDate) <= end);
                }
                else if (statusFilter == "plus75")
                {
                    var limit = today.AddDays(-75);
                    query = query.Where(c => c.Sales.Max(s => s.SaleDate) < limit);
                }
            }

            // 3. Sıralama
            query = query.OrderByDescending(c => c.Sales.Max(s => s.SaleDate));

            // 4. Sayfalama
            var totalCount = await query.CountAsync();

            var rawData = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Phone,
                    c.Address,
                    c.Email,
                    c.InstagramHandle,
                    OrderCount = c.Sales.Count(),
                    TotalSpent = c.Sales.Sum(s => s.TotalAmount),
                    LastOrderDate = c.Sales.Max(s => (DateTime?)s.SaleDate),
                    FirstOrderDate = c.Sales.Min(s => (DateTime?)s.SaleDate)
                })
                .ToListAsync();

            // 5. ViewModel Mapping
            var resultList = new List<CustomerListViewModel>();
            var now = DateTime.UtcNow.Date;

            foreach (var item in rawData)
            {
                var vm = new CustomerListViewModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Phone = item.Phone,
                    Address = item.Address,
                    Email = item.Email,
                    InstagramHandle = item.InstagramHandle,
                    OrderCount = item.OrderCount,
                    TotalSpent = item.TotalSpent,
                    LastOrderDate = item.LastOrderDate
                };

                double? daysSinceLast = item.LastOrderDate.HasValue ? (now - item.LastOrderDate.Value.Date).TotalDays : null;
                double? daysSinceFirst = item.FirstOrderDate.HasValue ? (now - item.FirstOrderDate.Value.Date).TotalDays : null;

                if (item.TotalSpent >= 20000 || item.OrderCount >= 10)
                    vm.IsVip = true;
                else if (item.OrderCount >= 3)
                    vm.IsLoyal = true;

                if (item.OrderCount > 0 && daysSinceFirst.HasValue && daysSinceFirst <= 30)
                    vm.IsNew = true;

                if (daysSinceLast.HasValue)
                {
                    if (daysSinceLast >= 45 && daysSinceLast <= 75) vm.IsRisky = true;
                    else if (daysSinceLast > 75) vm.IsPlus75 = true;
                }

                resultList.Add(vm);
            }

            return new PaginatedResult<CustomerListViewModel>(resultList, totalCount, pageNumber, pageSize);
        }

        /// <summary>
        /// [YENİ] İsme göre müşteriyi bulur, yoksa oluşturur.
        /// (Bulk Import ve Hızlı Satış senaryoları için optimizedir)
        /// </summary>
        public async Task<Customer> GetOrCreateCustomerAsync(string name, string? phone, string? address)
        {
            // 1. Veri Normalizasyonu (Trim ve Null Check)
            var customerName = string.IsNullOrWhiteSpace(name) ? "Misafir" : name.Trim();
            var customerNameUpper = customerName.ToUpperInvariant();

            // 2. Mevcut Müşteriyi Ara (ToUpper ile case-insensitive)
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Name.ToUpper() == customerNameUpper);

            // 3. Varsa Döndür
            if (customer != null)
            {
                return customer;
            }

            // 4. Yoksa Oluştur
            customer = new Customer
            {
                Name = customerName,
                Phone = phone,
                Address = address,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return customer;
        }
    }
}