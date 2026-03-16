using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeEngine.Application.Interfaces;

namespace TradeEngine.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("account/{accountId}/pnl")]
        public async Task<IActionResult> GetPnLReport(Guid accountId)
        {
            var report = await _analyticsService.GetPnLReportAsync(accountId);
            return Ok(report);
        }

        [HttpGet("account/{accountId}/portfolio")]
        public async Task<IActionResult> GetPortfolioSummary(Guid accountId)
        {
            var summary = await _analyticsService.GetPortfolioSummaryAsync(accountId);
            return Ok(summary);
        }

        [HttpGet("account/{accountId}/ledger")]
        public async Task<IActionResult> GetLedgerStatement(
            Guid accountId,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var statement = await _analyticsService.GetLedgerStatementAsync(accountId, from, to);
            return Ok(statement);
        }
    }
}
