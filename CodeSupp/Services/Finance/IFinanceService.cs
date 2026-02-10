using CodeSupp.Enums;
using CodeSupp.Models;
using CodeSupp.ViewModels;

namespace CodeSupp.Services.Finance
{
    public interface IFinanceService
    {
        // Raporlama
        Task<PaginatedResult<TransactionViewModel>> GetTransactionsAsync(
            int page,
            int pageSize,
            TransactionType? typeFilter = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? search = null);

        Task<FinanceStatsViewModel> GetSummaryStatsAsync(DateTime? startDate, DateTime? endDate);

        // İşlemler (Write Operations)

        // [GÜNCELLEME] Ödeme yöntemi parametresi eklendi
        Task RegisterSaleRevenueAsync(Sale sale, PaymentMethod paymentMethod = PaymentMethod.CreditCard);

        // [YENİ] Manuel Gelir Ekleme (Sermaye vb.)
        Task AddManualIncomeAsync(decimal amount, TransactionCategory category, PaymentMethod method, string description, DateTime date);

        // [YENİ] Manuel Gider Ekleme (Kira, Maaş vb.)
        Task AddManualExpenseAsync(decimal amount, TransactionCategory category, PaymentMethod method, string description, DateTime date);

        // [YENİ] İade İşlemi
        Task RefundSaleAsync(int saleId, decimal refundAmount, string reason);
    }
}