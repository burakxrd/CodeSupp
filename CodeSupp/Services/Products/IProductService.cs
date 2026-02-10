using CodeSupp.Models;

namespace CodeSupp.Services.Products
{
    public interface IProductService
    {
        /// <summary>
        /// Ürün isminden akıllı kod üretir.
        /// </summary>
        Task<string> GenerateSmartCodeAsync(string productName);

        /// <summary>
        /// Stok düzeltme işlemini yapar.
        /// [GÜNCELLEME] TenantId parametresi kaldırıldı.
        /// </summary>
        Task<int> AdjustStockAsync(int productId, int adjustmentAmount, string reason, decimal? specificCost = null);

        /// <summary>
        /// Ortalama maliyetleri hesaplar.
        /// </summary>
        Task<Dictionary<int, decimal>> CalculateAverageCostsAsync(List<int> productIds);

        /// <summary>
        /// Açılış stoğu logu atar.
        /// [GÜNCELLEME] TenantId parametresi kaldırıldı.
        /// </summary>
        Task AddOpeningStockLogAsync(int productId, int quantity, decimal unitCost);

        /// <summary>
        /// Business kuralları işleterek ürün oluşturur.
        /// </summary>
        Task<Product> CreateProductAsync(Product product);

        /// <summary>
        /// Ürünü günceller.
        /// </summary>
        Task UpdateProductAsync(Product product);

        /// <summary>
        /// Ürünü güvenli şekilde siler.
        /// </summary>
        Task DeleteProductAsync(int id);
    }
}