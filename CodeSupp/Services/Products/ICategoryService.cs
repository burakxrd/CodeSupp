using CodeSupp.Dtos;

namespace CodeSupp.Services.Products
{
    public interface ICategoryService
    {
        // Tüm kategorileri getirir (içindeki ürün sayısıyla birlikte)
        Task<List<CategoryDto>> GetAllCategoriesAsync();

        // Yeni kategori ekler
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto model);

        // Kategoriyi günceller
        Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto model);

        // Kategoriyi siler
        Task DeleteCategoryAsync(int id);
    }
}