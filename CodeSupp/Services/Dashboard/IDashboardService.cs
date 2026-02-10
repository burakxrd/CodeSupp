using CodeSupp.ViewModels;

namespace CodeSupp.Services.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<FinanceSummaryViewModel> GetFinanceSummaryAsync(DateTime? startDate, DateTime? endDate);
    }
}