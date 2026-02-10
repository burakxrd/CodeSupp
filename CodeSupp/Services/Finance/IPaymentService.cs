using CodeSupp.Models;
using CodeSupp.ViewModels;

namespace CodeSupp.Services.Finance
{
    public interface IPaymentService
    {
        // Listeleme (Filtreli, Sayfalı, Sıralı)
        Task<PaginatedResult<PaymentListViewModel>> GetPaymentsAsync(
            int page,
            int pageSize,
            string? search,
            string? sortBy,
            string? sortDir,
            DateTime? startDate,
            DateTime? endDate
        );

        // Tekli Getir
        Task<Payment?> GetPaymentByIdAsync(int id);

        // Ekle
        Task<Payment> CreatePaymentAsync(PaymentViewModel model);

        // Güncelle
        Task UpdatePaymentAsync(int id, PaymentViewModel model);

        // Sil
        Task DeletePaymentAsync(int id);
    }
}