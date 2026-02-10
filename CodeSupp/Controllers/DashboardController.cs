using CodeSupp.Services.Dashboard;
using CodeSupp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSupp.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardViewModel>> GetDashboardStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var stats = await _dashboardService.GetDashboardStatsAsync(startDate, endDate);
            return Ok(stats);
        }

        [HttpGet("finance-summary")]
        public async Task<ActionResult<FinanceSummaryViewModel>> GetFinanceSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var summary = await _dashboardService.GetFinanceSummaryAsync(startDate, endDate);
            return Ok(summary);
        }
    }
}