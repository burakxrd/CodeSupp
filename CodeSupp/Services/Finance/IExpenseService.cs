using CodeSupp.Models;
using CodeSupp.ViewModels;

namespace CodeSupp.Services.Finance
{
    public interface IExpenseService
    {
        // [GÜNCELLENDİ] Artık ViewModel listesi dönüyor
        Task<PaginatedResult<ExpenseViewModel>> GetExpensesAsync(
            int page,
            int pageSize,
            string? search,
            string? sortBy,
            string? sortDir,
            DateTime? startDate,
            DateTime? endDate
        );

        Task<Expense?> GetExpenseByIdAsync(int id);
        Task<Expense> CreateExpenseAsync(Expense expense);
        Task UpdateExpenseAsync(Expense expense);
        Task DeleteExpenseAsync(int id);
    }
}