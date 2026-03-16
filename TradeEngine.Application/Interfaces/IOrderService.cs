using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeEngine.Application.DTOs;
using TradeEngine.Domain.Entities;
using TradeEngine.Domain.Enums;

namespace TradeEngine.Application.Interfaces
{
    public interface IOrderService
    {
        Task<Guid> PlaceBuyOrderAsync(PlaceOrderRequest request);
        Task<Guid> PlaceSellOrderAsync(PlaceOrderRequest request);
        Task<Guid> PlaceOrderAsync(PlaceOrderRequest request, OrderSide side);
        Task ExecuteOrderAsync(Guid orderId);
        Task CancelOrderAsync(Guid orderId);
        Task<Order?> GetOrderAsync(Guid orderId);
        Task<List<Order>> GetAccountOrdersAsync(Guid accountId, OrderStatus? status = null);
        Task<List<Trade>> GetAccountTradesAsync(Guid accountId);
        Task<List<Position>> GetAccountPositionsAsync(Guid accountId);
    }
}
