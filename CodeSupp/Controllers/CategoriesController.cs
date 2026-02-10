using CodeSupp.Dtos;
using CodeSupp.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSupp.Controllers
{
    [Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/categories
        // Tüm kategorileri listeler
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            return Ok(result);
        }

        // POST: api/categories
        // Yeni kategori oluşturur
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto model)
        {
            // Model validasyonu (Data Annotations)
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _categoryService.CreateCategoryAsync(model);
            return Ok(result);
        }

        // PUT: api/categories/5
        // Kategoriyi günceller
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto model)
        {
            if (id != model.Id) return BadRequest(new { message = "ID uyuşmazlığı." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _categoryService.UpdateCategoryAsync(model);
            return Ok(result);
        }

        // DELETE: api/categories/5
        // Kategoriyi siler
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return Ok(new { message = "Kategori başarıyla silindi." });
        }
    }
}