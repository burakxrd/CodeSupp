using CodeSupp.Enums;
using CodeSupp.Services.Finance;
using CodeSupp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSupp.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class FinanceController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        // 1. HESAP HAREKETLERİ LİSTESİ (Table)
        [HttpGet("transactions")]
        public async Task<ActionResult<PaginatedResult<TransactionViewModel>>> GetTransactions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] TransactionType? type = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? search = null)
        {
            var result = await _financeService.GetTransactionsAsync(
                page, pageSize, type, startDate, endDate, search);

            return Ok(result);
        }

        // 2. ÖZET KARTLARI (Dashboard Cards)
        // Frontend: Sayfa yüklenince veya tarih değişince burası çağrılacak.
        [HttpGet("stats")]
        public async Task<ActionResult<FinanceStatsViewModel>> GetStats(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var stats = await _financeService.GetSummaryStatsAsync(startDate, endDate);
            return Ok(stats);
        }
    }
}