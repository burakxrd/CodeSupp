using CodeSupp.Services.Products;
using CodeSupp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSupp.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/product-purchase")]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _service;

        public PurchaseController(IPurchaseService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _service.GetPurchaseHistoryAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetPurchaseByIdAsync(id);
            if (result == null) return NotFound(new { Message = "Kayıt bulunamadı." });
            return Ok(result);
        }

        [HttpGet("create-data")]
        public async Task<IActionResult> GetCreateData()
        {
            var data = await _service.GetCreatePurchaseDataAsync();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductPurchaseViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.CreatePurchaseAsync(model);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductPurchaseViewModel model)
        {
            if (id != model.Id) return BadRequest(new { Message = "ID uyuşmazlığı." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _service.UpdatePurchaseAsync(id, model);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeletePurchaseAsync(id);
            return NoContent();
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate([FromBody] List<ProductPurchaseViewModel> models)
        {
            var result = await _service.CreateBulkPurchaseAsync(models);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Message = result.Message });
            }

            return Ok(new { Message = result.Message, Count = result.SuccessCount });
        }
    }
}