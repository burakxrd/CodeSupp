using AutoMapper;
using CodeSupp.Data;
using CodeSupp.Enums;
using CodeSupp.Models;
using CodeSupp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CodeSupp.Services.Finance
{
    public class ExpenseService : IExpenseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ExpenseService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ExpenseViewModel>> GetExpensesAsync(
            int page,
            int pageSize,
            string? search,
            string? sortBy,
            string? sortDir,
            DateTime? startDate,
            DateTime? endDate)
        {
            var query = _context.Expenses.AsNoTracking();

            // ✅ DÜZELTİLDİ: ToUpper ile case-insensitive arama
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchUpper = search.Trim().ToUpperInvariant();
                query = query.Where(e => e.Description != null && e.Description.ToUpper().Contains(searchUpper));
            }

            if (startDate.HasValue) query = query.Where(e => e.Date >= startDate.Value);
            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(e => e.Date <= end);
            }

            bool isAsc = sortDir == "asc";
            query = sortBy switch
            {
                "amount" => isAsc ? query.OrderBy(e => e.Amount) : query.OrderByDescending(e => e.Amount),
                "date" => isAsc ? query.OrderBy(e => e.Date) : query.OrderByDescending(e => e.Date),
                _ => query.OrderByDescending(e => e.Date)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = _mapper.Map<List<ExpenseViewModel>>(items);

            return new PaginatedResult<ExpenseViewModel>(model, totalCount, page, pageSize);
        }

        public async Task<Expense?> GetExpenseByIdAsync(int id)
        {
            return await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Expense> CreateExpenseAsync(Expense expense)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                expense.CreatedAt = DateTime.UtcNow;
                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();

                var financeRecord = new Transaction
                {
                    Type = TransactionType.Expense,
                    Category = expense.Category,
                    PaymentMethod = expense.PaymentMethod,
                    Amount = expense.Amount,
                    Description = expense.Description,
                    Date = expense.Date,
                    TenantId = expense.TenantId,
                    ExpenseId = expense.Id
                };

                _context.Transactions.Add(financeRecord);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return expense;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateExpenseAsync(Expense expense)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Expenses.Update(expense);

                var financeRecord = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.ExpenseId == expense.Id);

                if (financeRecord != null)
                {
                    financeRecord.Amount = expense.Amount;
                    financeRecord.Description = expense.Description;
                    financeRecord.Date = expense.Date;
                    financeRecord.Category = expense.Category;
                    financeRecord.PaymentMethod = expense.PaymentMethod;
                    _context.Transactions.Update(financeRecord);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteExpenseAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);

                if (expense != null)
                {
                    var financeRecord = await _context.Transactions
                        .FirstOrDefaultAsync(t => t.ExpenseId == id);

                    if (financeRecord != null)
                    {
                        _context.Transactions.Remove(financeRecord);
                    }

                    _context.Expenses.Remove(expense);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}