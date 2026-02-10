using CodeSupp.Dtos;
using CodeSupp.Models;
using CodeSupp.ViewModels;

namespace CodeSupp.Services.Sales
{
    public interface ISaleCommands
    {
        // 1. Satış Oluşturma
        Task<Sale> CreateSaleAsync(CreateSaleViewModel model);

        // 2. Toplu Satış (Bulk)
        Task<SalesProcessResult> CreateBulkSalesAsync(BulkSaleRequestDto request);

        // 3. Satış Düzenleme
        Task<Sale> UpdateSaleAsync(int id, EditSaleViewModel model);

        // 4. Satış Silme
        Task DeleteSaleAsync(int id);

        // 5. Kargo Durumu Güncelleme
        Task<Sale> UpdateShippingStatusAsync(int saleId, UpdateShippingStatusDto dto);
    }
}