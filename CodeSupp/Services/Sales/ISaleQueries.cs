using CodeSupp.Dtos;
using CodeSupp.Models;
using CodeSupp.ViewModels;

namespace CodeSupp.Services.Sales
{
    public interface ISaleQueries
    {
        // 1. Gelişmiş Filtreleme ile Listeleme
        Task<PaginatedResult<SaleListViewModel>> GetSalesAsync(SaleFilterDto filter);

        // 2. Tekil Satış Getirme
        Task<Sale?> GetSaleByIdAsync(int id);

        // 3. Create Ekranı İçin Gerekli Datayı Getirme
        Task<CreateSaleDataViewModel> GetCreateSaleDataAsync();
    }
}