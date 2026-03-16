using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeEngine.Application.DTOs;
using TradeEngine.Application.Interfaces;
using TradeEngine.Domain.Enums;

namespace TradeEngine.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("buy")]
        public async Task<IActionResult> PlaceBuyOrder([FromBody] PlaceOrderRequest request)
        {
            var orderId = await _orderService.PlaceBuyOrderAsync(request);
            return Ok(new { orderId, message = "Buy order placed successfully." });
        }

        [HttpPost("sell")]
        public async Task<IActionResult> PlaceSellOrder([FromBody] PlaceOrderRequest request)
        {
            var orderId = await _orderService.PlaceSellOrderAsync(request);
            return Ok(new { orderId, message = "Sell order placed successfully." });
        }

        [HttpPost("{id}/execute")]
        public async Task<IActionResult> ExecuteOrder(Guid id)
        {
            await _orderService.ExecuteOrderAsync(id);
            return Ok(new { message = "Order executed successfully." });
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            await _orderService.CancelOrderAsync(id);
            return Ok(new { message = "Order cancelled successfully." });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _orderService.GetOrderAsync(id);
            if (order == null)
                return NotFound(new { message = "Order not found." });
            return Ok(order);
        }

        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetAccountOrders(Guid accountId, [FromQuery] OrderStatus? status = null)
        {
            var orders = await _orderService.GetAccountOrdersAsync(accountId, status);
            return Ok(orders);
        }

        [HttpGet("account/{accountId}/trades")]
        public async Task<IActionResult> GetAccountTrades(Guid accountId)
        {
            var trades = await _orderService.GetAccountTradesAsync(accountId);
            return Ok(trades);
        }

        [HttpGet("account/{accountId}/positions")]
        public async Task<IActionResult> GetAccountPositions(Guid accountId)
        {
            var positions = await _orderService.GetAccountPositionsAsync(accountId);
            return Ok(positions);
        }
    }
}
