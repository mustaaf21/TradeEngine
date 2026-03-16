using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeEngine.Application.Interfaces;

namespace TradeEngine.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SettlementController : ControllerBase
    {
        private readonly ISettlementService _settlementService;

        public SettlementController(ISettlementService settlementService)
        {
            _settlementService = settlementService;
        }

        [HttpPost("trade/{tradeId}")]
        public async Task<IActionResult> SettleTrade(Guid tradeId)
        {
            await _settlementService.SettleTradeAsync(tradeId);
            return Ok(new { message = "Trade settled successfully." });
        }

        [HttpPost("process-pending")]
        public async Task<IActionResult> ProcessPendingSettlements()
        {
            await _settlementService.SettlePendingTradesAsync();
            return Ok(new { message = "Pending settlements processed." });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingSettlements()
        {
            var pending = await _settlementService.GetPendingSettlementsAsync();
            return Ok(pending);
        }
    }
}
