using CodeSupp.Data;
using CodeSupp.Enums;
using CodeSupp.Models;
using CodeSupp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CodeSupp.Services.Finance
{
    public class FinanceService : IFinanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FinanceService> _logger;

        public FinanceService(ApplicationDbContext context, ILogger<FinanceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Raporlama ve Listeleme (Read Operations)

        public async Task<PaginatedResult<TransactionViewModel>> GetTransactionsAsync(
            int page,
            int pageSize,
            TransactionType? typeFilter = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? search = null)
        {
            var query = _context.Transactions
                .AsNoTracking()
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .AsQueryable();

            if (typeFilter.HasValue)
                query = query.Where(t => t.Type == typeFilter.Value);

            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(t => t.Date <= end);
            }

            // ✅ DÜZELTİLDİ: ToUpper ile case-insensitive arama
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchUpper = search.Trim().ToUpperInvariant();
                query = query.Where(t => t.Description != null && t.Description.ToUpper().Contains(searchUpper));
            }

            var totalCount = await query.CountAsync();

            var list = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TransactionViewModel
                {
                    Id = t.Id,
                    Date = t.Date,
                    Type = t.Type,
                    Category = t.Category,
                    PaymentMethod = t.PaymentMethod,
                    Amount = t.Amount,
                    Description = t.Description ?? ""
                })
                .ToListAsync();

            return new PaginatedResult<TransactionViewModel>(list, totalCount, page, pageSize);
        }

        public async Task<FinanceStatsViewModel> GetSummaryStatsAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Transactions.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(t => t.Date <= end);
            }

            var stats = await query
                .GroupBy(t => 1)
                .Select(g => new
                {
                    TotalIncome = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    TotalExpense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                })
                .FirstOrDefaultAsync();

            var income = stats?.TotalIncome ?? 0;
            var expense = stats?.TotalExpense ?? 0;

            return new FinanceStatsViewModel
            {
                TotalIncome = income,
                TotalExpense = expense,
                NetProfit = income - expense
            };
        }

        #endregion

        #region İşlem Yönetimi (Write Operations)

        public async Task RegisterSaleRevenueAsync(Sale sale, PaymentMethod paymentMethod = PaymentMethod.CreditCard)
        {
            if (sale.TotalAmount <= 0) return;

            var payment = new Payment
            {
                CustomerId = sale.CustomerId,
                SaleId = sale.Id,
                Amount = sale.TotalAmount,
                Date = sale.SaleDate,
                CreatedAt = DateTime.UtcNow,
                Description = $"Sipariş #{sale.OrderCode}",
                Category = TransactionCategory.Sale,
                PaymentMethod = paymentMethod,
                TenantId = sale.TenantId
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var incomeRecord = new Transaction
            {
                Type = TransactionType.Income,
                Category = TransactionCategory.Sale,
                PaymentMethod = payment.PaymentMethod,
                Amount = payment.Amount,
                Description = payment.Description,
                Date = payment.Date,
                PaymentId = payment.Id,
                TenantId = sale.TenantId
            };
            _context.Transactions.Add(incomeRecord);

            if (sale.PlatformCommission > 0)
            {
                var commissionRecord = new Transaction
                {
                    Type = TransactionType.Expense,
                    Category = TransactionCategory.OtherExpense,
                    PaymentMethod = PaymentMethod.Other,
                    Amount = sale.PlatformCommission,
                    Description = $"Sipariş #{sale.OrderCode} Komisyon Kesintisi",
                    Date = sale.SaleDate,
                    TenantId = sale.TenantId
                };
                _context.Transactions.Add(commissionRecord);
            }

            if (sale.ShippingCost > 0)
            {
                var shippingRecord = new Transaction
                {
                    Type = TransactionType.Expense,
                    Category = TransactionCategory.OtherExpense,
                    PaymentMethod = PaymentMethod.Other,
                    Amount = sale.ShippingCost,
                    Description = $"Sipariş #{sale.OrderCode} Kargo Maliyeti",
                    Date = sale.SaleDate,
                    TenantId = sale.TenantId
                };
                _context.Transactions.Add(shippingRecord);
            }

            await _context.SaveChangesAsync();
        }

        public async Task AddManualIncomeAsync(decimal amount, TransactionCategory category, PaymentMethod method, string description, DateTime date)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var payment = new Payment
                {
                    Amount = amount,
                    Category = category,
                    PaymentMethod = method,
                    Description = description,
                    Date = date,
                    CreatedAt = DateTime.UtcNow,
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                var ledgerRecord = new Transaction
                {
                    Type = TransactionType.Income,
                    Category = category,
                    PaymentMethod = method,
                    Amount = amount,
                    Description = description,
                    Date = date,
                    PaymentId = payment.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Transactions.Add(ledgerRecord);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manuel gelir eklenirken hata oluştu.");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task AddManualExpenseAsync(decimal amount, TransactionCategory category, PaymentMethod method, string description, DateTime date)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var expense = new Expense
                {
                    Amount = amount,
                    Category = category,
                    PaymentMethod = method,
                    Description = description,
                    Date = date,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();

                var ledgerRecord = new Transaction
                {
                    Type = TransactionType.Expense,
                    Category = category,
                    PaymentMethod = method,
                    Amount = amount,
                    Description = description,
                    Date = date,
                    ExpenseId = expense.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Transactions.Add(ledgerRecord);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manuel gider eklenirken hata oluştu.");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RefundSaleAsync(int saleId, decimal refundAmount, string reason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var sale = await _context.Sales.FindAsync(saleId);
                if (sale == null) throw new Exception("Satış bulunamadı.");

                var refundExpense = new Expense
                {
                    Amount = refundAmount,
                    Category = TransactionCategory.Refund,
                    PaymentMethod = PaymentMethod.Other,
                    Description = $"İADE - Sipariş #{sale.OrderCode}: {reason}",
                    Date = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Expenses.Add(refundExpense);
                await _context.SaveChangesAsync();

                var ledgerRecord = new Transaction
                {
                    Type = TransactionType.Expense,
                    Category = TransactionCategory.Refund,
                    PaymentMethod = PaymentMethod.Other,
                    Amount = refundAmount,
                    Description = refundExpense.Description,
                    Date = DateTime.UtcNow,
                    ExpenseId = refundExpense.Id,
                    TenantId = sale.TenantId
                };
                _context.Transactions.Add(ledgerRecord);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        #endregion
    }
}