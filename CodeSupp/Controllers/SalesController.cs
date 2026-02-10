using CodeSupp.Dtos;
using CodeSupp.Services.Sales;
using CodeSupp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSupp.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleQueries _saleQueries;
        private readonly ISaleCommands _saleCommands;

        public SalesController(ISaleQueries saleQueries, ISaleCommands saleCommands)
        {
            _saleQueries = saleQueries;
            _saleCommands = saleCommands;
        }

        // --- READ (Queries) ---

        [HttpGet]
        public async Task<IActionResult> GetSales([FromQuery] SaleFilterDto filter)
        {
            var result = await _saleQueries.GetSalesAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSale(int id)
        {
            var sale = await _saleQueries.GetSaleByIdAsync(id);
            if (sale == null) return NotFound(new { Message = "Sipariş bulunamadı." });

            return Ok(sale);
        }

        [HttpGet("CreateData")]
        public async Task<IActionResult> GetCreateData()
        {
            var data = await _saleQueries.GetCreateSaleDataAsync();

            return Ok(new
            {
                AvailableCustomers = data.Customers,
                AvailableProducts = data.Products
            });
        }

        // --- WRITE (Commands) ---

        [HttpPost]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleViewModel viewModel)
        {
            var sale = await _saleCommands.CreateSaleAsync(viewModel);
            return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulkSale([FromBody] BulkSaleRequestDto request)
        {
            var result = await _saleCommands.CreateBulkSalesAsync(request);

            if (result.SuccessCount == 0 && result.FailCount > 0)
                return BadRequest(new { Message = result.Message, Errors = result.Errors });

            return Ok(new { Message = result.Message, Errors = result.Errors });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditSale(int id, [FromBody] EditSaleViewModel viewModel)
        {
            if (id != viewModel.Id) return BadRequest(new { Message = "ID uyuşmazlığı." });

            var updatedSale = await _saleCommands.UpdateSaleAsync(id, viewModel);
            return Ok(updatedSale);
        }

        [HttpPatch("{id}/shipping")]
        public async Task<IActionResult> UpdateShippingStatus(int id, [FromBody] UpdateShippingStatusDto dto)
        {
            var updatedSale = await _saleCommands.UpdateShippingStatusAsync(id, dto);
            return Ok(updatedSale);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSale(int id)
        {
            await _saleCommands.DeleteSaleAsync(id);
            return NoContent();
        }
    }
}