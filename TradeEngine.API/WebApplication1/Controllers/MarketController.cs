using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeEngine.Application.Interfaces;

namespace TradeEngine.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketController : ControllerBase
    {
        private readonly IMatchingEngine _matchingEngine;
        private readonly IPriceService _priceService;

        public MarketController(IMatchingEngine matchingEngine, IPriceService priceService)
        {
            _matchingEngine = matchingEngine;
            _priceService = priceService;
        }

        [HttpGet("orderbook/{assetSymbol}")]
        public async Task<IActionResult> GetOrderBook(string assetSymbol)
        {
            var orderBook = await _matchingEngine.GetOrderBookAsync(assetSymbol);
            return Ok(orderBook);
        }

        [HttpGet("price/{assetSymbol}")]
        public async Task<IActionResult> GetPrice(string assetSymbol)
        {
            var price = await _priceService.GetCurrentPriceAsync(assetSymbol);
            return Ok(new { symbol = assetSymbol, price });
        }

        [Authorize]
        [HttpPost("match/{orderId}")]
        public async Task<IActionResult> MatchOrder(Guid orderId)
        {
            var trades = await _matchingEngine.MatchOrderAsync(orderId);
            return Ok(new { message = "Order matching completed.", tradesExecuted = trades.Count, trades });
        }

        [Authorize]
        [HttpPost("price/{assetSymbol}")]
        public IActionResult SetPrice(string assetSymbol, [FromBody] decimal price)
        {
            _priceService.SetPrice(assetSymbol, price);
            return Ok(new { message = $"Price for {assetSymbol} set to {price}." });
        }
    }
}
