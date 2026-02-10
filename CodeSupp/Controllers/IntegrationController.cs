using CodeSupp.Services.Integration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSupp.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class IntegrationController : ControllerBase
    {
        private readonly IIntegrationService _integrationService;

        public IntegrationController(IIntegrationService integrationService)
        {
            _integrationService = integrationService;
        }

        // docType: 'invoice' (Alım) veya 'order' (Satış/DM)
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeDocument([FromForm] IFormFile file, [FromForm] string docType)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya yüklenmedi.");

            try
            {
                // Tüm kirli işleri servis hallediyor
                var result = await _integrationService.AnalyzeDocumentAsync(file, docType ?? "invoice");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}