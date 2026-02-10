using CodeSupp.Data;
using CodeSupp.Services.Finance;
using CodeSupp.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CodeSupp.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFinanceService _financeService;

        public DashboardService(ApplicationDbContext context, IFinanceService financeService)
        {
            _context = context;
            _financeService = financeService;
        }

        private int CalculateGrowthRate(decimal current, decimal previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return (int)((current - previous) / previous * 100);
        }

        public async Task<DashboardViewModel> GetDashboardStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            // 1. TARİH ARALIĞI
            DateTime currentStart, currentEnd, prevStart, prevEnd;

            if (startDate.HasValue && endDate.HasValue)
            {
                currentStart = startDate.Value;
                currentEnd = endDate.Value.Date.AddDays(1).AddTicks(-1);

                TimeSpan duration = currentEnd - currentStart;
                prevEnd = currentStart.AddSeconds(-1);
                prevStart = prevEnd.Subtract(duration);
            }
            else
            {
                var now = DateTime.Now;
                currentStart = new DateTime(now.Year, now.Month, 1);
                currentEnd = now;

                prevStart = currentStart.AddMonths(-1);
                prevEnd = currentStart.AddSeconds(-1);
            }

            // 2. FİNANSAL VERİLER (FinanceService Entegrasyonu)
            // Kasa Durumu: Gelirler (Satış) - Giderler (Kira, Komisyon, Fatura vb.)
            var currentFinance = await _financeService.GetSummaryStatsAsync(currentStart, currentEnd);
            var prevFinance = await _financeService.GetSummaryStatsAsync(prevStart, prevEnd);

            // [DÜZELTME] ÜRÜN MALİYETİ (COGS) HESABI
            // FinanceService sadece kasadan çıkan parayı bilir. Satılan malın maliyeti (stoktan düşen değer)
            // kasadan o an çıkmaz ama kârdan düşülmelidir.
            var totalProductCost = await _context.SaleItems
                .AsNoTracking()
                .Where(si => si.Sale.SaleDate >= currentStart && si.Sale.SaleDate <= currentEnd)
                .SumAsync(si => si.Quantity * (si.Product != null ? si.Product.CostPrice : 0));

            // GERÇEK NET KÂR: (Kasa Kârı) - (Satılan Malın Maliyeti)
            var netProfit = currentFinance.NetProfit - totalProductCost;

            var currentRevenue = currentFinance.TotalIncome;
            var revenueRate = CalculateGrowthRate(currentRevenue, prevFinance.TotalIncome);


            // 3. OPERASYONEL VERİLER (Sipariş & Gider Detayları)
            var currentSalesStats = await _context.Sales
                .AsNoTracking()
                .Where(s => s.SaleDate >= currentStart && s.SaleDate <= currentEnd)
                .GroupBy(s => 1)
                .Select(g => new
                {
                    OrderCount = g.Count(),
                    TotalShipping = g.Sum(s => s.ShippingCost),
                    TotalCommission = g.Sum(s => s.PlatformCommission),
                    TotalTax = g.Sum(s => s.TaxAmount)
                })
                .FirstOrDefaultAsync();

            var prevOrderCount = await _context.Sales
                .Where(s => s.SaleDate >= prevStart && s.SaleDate <= prevEnd)
                .CountAsync();

            var currentOrderCount = currentSalesStats?.OrderCount ?? 0;
            var orderRate = CalculateGrowthRate(currentOrderCount, prevOrderCount);

            var totalShippingCost = currentSalesStats?.TotalShipping ?? 0;
            var totalCommissions = currentSalesStats?.TotalCommission ?? 0;
            var totalOperationalExpenses = (currentSalesStats?.TotalTax ?? 0) + totalShippingCost + totalCommissions;


            // 4. ÜRÜN & STOK DURUMU
            var productStats = await _context.Products
                .AsNoTracking()
                .GroupBy(p => 1)
                .Select(g => new
                {
                    TotalCount = g.Count(),
                    TotalStock = g.Sum(p => (int?)p.Stock) ?? 0,
                    CriticalStock = g.Count(p => p.Stock > 0 && p.Stock < 10),
                    OutOfStock = g.Count(p => p.Stock <= 0),
                    InventoryValue = g.Sum(p => p.Stock * p.CostPrice),
                    PotentialRevenue = g.Sum(p => p.Stock * p.Price)
                })
                .FirstOrDefaultAsync();


            // 5. LİSTELEMELER (Son Siparişler & Çok Satanlar)
            var recentOrders = await _context.Sales
                .AsNoTracking()
                .Where(s => !startDate.HasValue || (s.SaleDate >= currentStart && s.SaleDate <= currentEnd))
                .Include(s => s.Customer)
                .Include(s => s.Payments)
                .Include(s => s.SaleItems).ThenInclude(si => si.Product)
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .Select(s => new RecentOrderViewModel
                {
                    Id = s.Id,
                    OrderCode = s.OrderCode,
                    CustomerName = s.Customer != null ? s.Customer.Name : "Misafir",
                    Amount = s.TotalAmount,
                    Date = s.SaleDate,
                    ItemCount = s.SaleItems.Sum(i => i.Quantity),
                    RemainingAmount = s.TotalAmount - s.Payments.Sum(p => p.Amount),
                    ProductNames = s.SaleItems.Select(si => si.Product.Name).ToList()
                })
                .ToListAsync();

            // Durum Ataması (Client-Side Logic)
            foreach (var order in recentOrders)
            {
                order.Status = order.RemainingAmount <= 0.01m ? "Tamamlandı" :
                               (order.Amount - order.RemainingAmount) > 0 ? "Kısmi" : "Bekliyor";
            }

            var topProducts = await _context.SaleItems
                .AsNoTracking()
                .Where(si => !startDate.HasValue || (si.Sale.SaleDate >= currentStart && si.Sale.SaleDate <= currentEnd))
                .GroupBy(si => new { si.ProductId, si.Product.Name })
                .Select(g => new { Name = g.Key.Name, Count = g.Sum(si => si.Quantity) })
                .OrderByDescending(x => x.Count)
                .Take(4)
                .ToListAsync();

            int maxSales = topProducts.Any() ? topProducts.Max(x => x.Count) : 1;
            var topProductViewModels = topProducts.Select(p => new TopProductViewModel
            {
                Name = p.Name,
                SalesCount = p.Count,
                Percentage = (int)((double)p.Count / maxSales * 100)
            }).ToList();


            // 6. GRAFİK VERİSİ
            var (chartLabels, chartRevenue) = await PrepareChartDataAsync(currentStart, currentEnd, startDate.HasValue);


            // SONUÇ
            return new DashboardViewModel
            {
                // Finansal
                TotalRevenue = currentRevenue,
                RevenueRate = revenueRate,
                NetProfit = netProfit, // [DÜZELTİLDİ] Artık COGS düşülmüş net ticari kâr

                // Operasyonel
                TotalOrders = currentOrderCount,
                OrderRate = orderRate,
                TotalCustomers = await _context.Customers.CountAsync(),

                // Detay Giderler
                TotalShippingCost = totalShippingCost,
                TotalCommissions = totalCommissions,
                TotalOperationalExpenses = totalOperationalExpenses,

                // Stok
                TotalProducts = productStats?.TotalCount ?? 0,
                TotalStockCount = productStats?.TotalStock ?? 0,
                CriticalStockCount = productStats?.CriticalStock ?? 0,
                OutOfStockCount = productStats?.OutOfStock ?? 0,
                TotalInventoryValue = productStats?.InventoryValue ?? 0,
                PotentialRevenue = productStats?.PotentialRevenue ?? 0,

                // Listeler
                RecentOrders = recentOrders,
                TopSellingProducts = topProductViewModels,
                Last6MonthsRevenue = chartRevenue,
                Last6MonthsLabels = chartLabels
            };
        }

        private async Task<(List<string> Labels, List<decimal> Data)> PrepareChartDataAsync(DateTime start, DateTime end, bool isCustomRange)
        {
            var labels = new List<string>();
            var data = new List<decimal>();
            var culture = new CultureInfo("tr-TR");

            if (!isCustomRange) // Varsayılan: Son 6 Ay
            {
                var sixMonthsAgo = DateTime.Now.AddMonths(-5);
                var queryStart = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

                var dbData = await _context.Sales
                    .AsNoTracking()
                    .Where(s => s.SaleDate >= queryStart)
                    .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
                    .Select(g => new { Date = g.Key, Total = g.Sum(s => s.TotalAmount) })
                    .ToListAsync();

                for (int i = 5; i >= 0; i--)
                {
                    var target = DateTime.Now.AddMonths(-i);
                    var match = dbData.FirstOrDefault(d => d.Date.Year == target.Year && d.Date.Month == target.Month);
                    labels.Add(target.ToString("MMMM", culture));
                    data.Add(match?.Total ?? 0);
                }
            }
            else // Özel Aralık: Günlük
            {
                var dbData = await _context.Sales
                    .AsNoTracking()
                    .Where(s => s.SaleDate >= start && s.SaleDate <= end)
                    .GroupBy(s => s.SaleDate.Date)
                    .Select(g => new { Date = g.Key, Total = g.Sum(s => s.TotalAmount) })
                    .ToListAsync();

                for (var day = start.Date; day <= end.Date; day = day.AddDays(1))
                {
                    var match = dbData.FirstOrDefault(d => d.Date == day);
                    labels.Add(day.ToString("dd MMM", culture));
                    data.Add(match?.Total ?? 0);
                }
            }

            return (labels, data);
        }

        public async Task<FinanceSummaryViewModel> GetFinanceSummaryAsync(DateTime? startDate, DateTime? endDate)
        {
            var stats = await _financeService.GetSummaryStatsAsync(startDate, endDate);
            return new FinanceSummaryViewModel
            {
                TotalIncome = stats.TotalIncome,
                TotalExpense = stats.TotalExpense,
                NetBalance = stats.NetProfit
            };
        }
    }
}