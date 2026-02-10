using CodeSupp.Models;
using CodeSupp.ViewModels;

namespace CodeSupp.Services.Products
{
    public interface IPurchaseService
    {
        /// <summary>
        /// Satın alma geçmişini sayfalı olarak getirir.
        /// </summary>
        Task<PaginatedResult<ProductPurchaseViewModel>> GetPurchaseHistoryAsync(int page, int pageSize);

        /// <summary>
        /// Tek bir satın alma kaydının detayını getirir.
        /// </summary>
        Task<ProductPurchaseViewModel?> GetPurchaseByIdAsync(int id);

        /// <summary>
        /// Yeni satın alma formu için gerekli dropdown (ürün listesi vb.) verilerini hazırlar.
        /// </summary>
        Task<ProductPurchaseViewModel> GetCreatePurchaseDataAsync();

        /// <summary>
        /// Yeni bir satın alma işlemi gerçekleştirir. 
        /// Stok artışı ve Gider (Expense) kaydını otomatik yönetir.
        /// </summary>
        Task<ProductPurchaseHistory> CreatePurchaseAsync(ProductPurchaseViewModel model);

        /// <summary>
        /// Var olan bir satın almayı günceller.
        /// Stok farklarını (Eski miktar vs Yeni miktar) ve Gider güncellemelerini yönetir.
        /// </summary>
        Task UpdatePurchaseAsync(int id, ProductPurchaseViewModel model);

        /// <summary>
        /// Satın alma kaydını siler.
        /// İlgili stoğu geri düşer ve Gider kaydını siler.
        /// </summary>
        Task DeletePurchaseAsync(int id);

        /// <summary>
        /// Toplu satın alma işlemi yapar. Transaction (Atomicity) yönetimini kendi içinde halleder.
        /// </summary>
        /// <returns>İşlem sonucu mesajı ve etkilenen kayıt sayısı döner.</returns>
        Task<(bool IsSuccess, string Message, int SuccessCount)> CreateBulkPurchaseAsync(List<ProductPurchaseViewModel> models);
    }
}