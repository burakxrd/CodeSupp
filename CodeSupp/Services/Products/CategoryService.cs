using CodeSupp.Data;
using CodeSupp.Dtos;
using CodeSupp.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSupp.Services.Products
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm kategorileri ürün sayılarıyla birlikte listeler.
        /// </summary>
        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            // Global Tenant Filter devrede.
            // Select projeksiyonu ile sadece ihtiyacımız olan alanları çekiyoruz.
            // c.Products.Count SQL tarafında 'COUNT(*)' olarak çalışır, performanslıdır.
            return await _context.Categories
                .AsNoTracking()
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type,
                    ProductCount = c.Products.Count
                })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto model)
        {
            // 1. Sınır Kontrolü (Soft Limit)
            var currentCount = await _context.Categories.CountAsync();
            if (currentCount >= 50)
            {
                throw new InvalidOperationException("Mevcut paketinizde en fazla 50 kategori oluşturabilirsiniz.");
            }

            // 2. İsim Kontrolü
            var exists = await _context.Categories.AnyAsync(c => c.Name == model.Name);
            if (exists)
            {
                throw new InvalidOperationException($"'{model.Name}' adında bir kategori zaten var.");
            }

            var category = new Category
            {
                Name = model.Name,
                Type = model.Type,
                // TenantId DbContext tarafından otomatik atanıyor.
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type,
                ProductCount = 0
            };
        }

        public async Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto model)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == model.Id);
            if (category == null) throw new KeyNotFoundException("Kategori bulunamadı.");

            // İsim değişiyorsa çakışma kontrolü
            if (category.Name != model.Name)
            {
                var exists = await _context.Categories.AnyAsync(c => c.Name == model.Name && c.Id != model.Id);
                if (exists) throw new InvalidOperationException($"'{model.Name}' adında başka bir kategori zaten var.");
            }

            category.Name = model.Name;
            category.Type = model.Type;

            await _context.SaveChangesAsync();

            // Güncel ürün sayısını çekip dönelim
            var productCount = await _context.Products.CountAsync(p => p.CategoryId == category.Id);

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type,
                ProductCount = productCount
            };
        }

        public async Task DeleteCategoryAsync(int id)
        {
            // [OPTIMIZASYON] Önceki kodda Include(Products) vardı.
            // Bu, kategori silerken binlerce ürünü belleğe çekiyordu.
            // Bunun yerine sadece kategoriyi çekiyoruz.
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) throw new KeyNotFoundException("Kategori bulunamadı.");

            // Ürün var mı kontrolünü veritabanı seviyesinde (AnyAsync/EXISTS) yapıyoruz.
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);

            if (hasProducts)
            {
                // Sayıyı merak ediyorsak ayrı bir count atabiliriz ama hata mesajı için 'ürünler var' demek yeterli.
                // İlla sayı lazımsa CountAsync kullanırız.
                var count = await _context.Products.CountAsync(p => p.CategoryId == id);
                throw new InvalidOperationException($"Bu kategoriye bağlı {count} adet ürün var. Lütfen önce ürünleri silin veya başka kategoriye taşıyın.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}