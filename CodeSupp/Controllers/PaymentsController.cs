using CodeSupp.Services.Finance;
using CodeSupp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSupp.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPayments(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = "date",
            [FromQuery] string? sortDir = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null
        )
        {
            var result = await _paymentService.GetPaymentsAsync(
                page, pageSize, search, sortBy, sortDir, startDate, endDate);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            // NotFound durumu bir hata değil, bir durumdur. O yüzden burada kalabilir.
            if (payment == null) return NotFound();

            return Ok(payment);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // [TEMİZLENDİ] Try-Catch Bloğu Kaldırıldı!
            // Eğer müşteri bulunamazsa Servis hata fırlatacak, Middleware yakalayıp JSON dönecek.
            var createdPayment = await _paymentService.CreatePaymentAsync(model);

            return CreatedAtAction(nameof(GetPayment), new { id = createdPayment.Id }, createdPayment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] PaymentViewModel model)
        {
            if (model.Id.HasValue && id != model.Id.Value)
                return BadRequest(new { Message = "ID uyuşmazlığı." });

            if (!ModelState.IsValid) return BadRequest(ModelState);

            // [TEMİZLENDİ] Try-Catch Bloğu Kaldırıldı!
            await _paymentService.UpdatePaymentAsync(id, model);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            await _paymentService.DeletePaymentAsync(id);
            return NoContent();
        }
    }
}