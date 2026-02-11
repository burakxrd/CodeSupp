using AutoMapper;
using CodeSupp.Data;
using CodeSupp.Dtos;
using CodeSupp.Models;
using CodeSupp.Services.Infrastructure;
using CodeSupp.Services.Products;
using CodeSupp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeSupp.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductsController(
            ApplicationDbContext context,
            IFileService fileService,
            IProductService productService,
            IMapper mapper)
        {
            _context = context;
            _fileService = fileService;
            _productService = productService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? sort = "newest")
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = TextNormalizer.NormalizeTurkish(search);

                query = query.Where(p =>
                    p.SearchText != null && p.SearchText.Contains(normalizedSearch)
                );
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            switch (sort?.ToLower())
            {
                case "oldest": query = query.OrderBy(p => p.CreatedDate); break;
                case "price_asc": query = query.OrderBy(p => p.Price); break;
                case "price_desc": query = query.OrderByDescending(p => p.Price); break;
                case "stock_asc": query = query.OrderBy(p => p.Stock); break;
                case "stock_desc": query = query.OrderByDescending(p => p.Stock); break;

                case "bestsellers":
                    query = query.Select(p => new {
                        Product = p,
                        SalesCount = _context.SaleItems.Where(si => si.ProductId == p.Id).Sum(si => si.Quantity)
                    })
                    .OrderByDescending(x => x.SalesCount)
                    .Select(x => x.Product);
                    break;

                case "newest":
                default:
                    query = query.OrderByDescending(p => p.CreatedDate);
                    break;
            }

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagedViewModels = _mapper.Map<List<ProductIndexViewModel>>(products);

            if (pagedViewModels.Any())
            {
                var productIds = pagedViewModels.Select(p => p.Id).ToList();

                var costs = await _productService.CalculateAverageCostsAsync(productIds);

                var salesCounts = await _context.SaleItems
                    .Where(si => productIds.Contains(si.ProductId))
                    .GroupBy(si => si.ProductId)
                    .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(si => si.Quantity) })
                    .ToDictionaryAsync(x => x.ProductId, x => x.TotalSold);

                foreach (var vm in pagedViewModels)
                {
                    if (costs.TryGetValue(vm.Id, out decimal cost) && cost > 0)
                        vm.AverageUnitCostInTRY = cost;
                    else
                        vm.AverageUnitCostInTRY = vm.CostPrice;

                    vm.TotalSales = salesCounts.TryGetValue(vm.Id, out int sales) ? sales : 0;
                }
            }

            return Ok(new PaginatedResult<ProductIndexViewModel>(pagedViewModels, totalCount, page, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] Product product, [FromForm] IFormFile? image)
        {
            ModelState.Remove(nameof(product.TenantId));
            ModelState.Remove(nameof(product.Category));

            if (!ModelState.IsValid) return BadRequest(ModelState);

            product.Id = 0;
            product.CreatedDate = DateTime.UtcNow;

            if (image != null)
            {
                product.ImagePath = await _fileService.UploadImageAsync(image, "products");
            }

            await _productService.CreateProductAsync(product);

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] Product product, [FromForm] IFormFile? image)
        {
            if (id != product.Id) return BadRequest(new { Message = "ID uyuşmazlığı" });

            ModelState.Remove(nameof(product.TenantId));
            ModelState.Remove(nameof(product.Category));

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var productInDb = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (productInDb == null) return NotFound();

            if (image != null)
            {
                if (!string.IsNullOrEmpty(productInDb.ImagePath))
                    _fileService.DeleteFile(productInDb.ImagePath);

                productInDb.ImagePath = await _fileService.UploadImageAsync(image, "products");
            }

            productInDb.Name = product.Name;
            productInDb.Description = product.Description;
            productInDb.Price = product.Price;
            productInDb.CategoryId = product.CategoryId;
            productInDb.Type = product.Type;
            productInDb.CostPrice = product.CostPrice;
            productInDb.DiscountedPrice = product.DiscountedPrice;
            productInDb.ShippingType = product.ShippingType;
            productInDb.ShippingPrice = product.ShippingPrice;
            productInDb.Size = product.Size;
            productInDb.Color = product.Color;

            if (!string.IsNullOrWhiteSpace(product.Code))
            {
                productInDb.Code = product.Code;
            }

            await _productService.UpdateProductAsync(productInDb);

            return Ok(new { Message = "Ürün bilgileri güncellendi." });
        }

        [HttpPost("{id}/adjust-stock")]
        public async Task<IActionResult> AdjustStock(int id, [FromBody] StockAdjustmentDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var newStock = await _productService.AdjustStockAsync(id, model.AdjustmentAmount, model.Reason);
                return Ok(new { Message = "Stok düzeltme işlemi başarıyla kaydedildi.", NewStock = newStock });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            try
            {
                if (!string.IsNullOrEmpty(product.ImagePath))
                    _fileService.DeleteFile(product.ImagePath);

                await _productService.DeleteProductAsync(id);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Silme işlemi sırasında hata oluştu.", Details = ex.Message });
            }
            return NoContent();
        }
    }
}