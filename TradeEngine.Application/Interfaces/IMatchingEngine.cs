using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeEngine.Domain.Entities;

namespace TradeEngine.Application.Interfaces
{
    public interface IMatchingEngine
    {
        Task<List<Trade>> MatchOrderAsync(Guid orderId);
        Task<OrderBookSnapshot> GetOrderBookAsync(string assetSymbol);
        Task ProcessMarketOrderAsync(Guid orderId);
        Task CheckStopOrdersAsync(string assetSymbol, decimal currentPrice);
    }

    public class OrderBookSnapshot
    {
        public string AssetSymbol { get; set; } = string.Empty;
        public List<OrderBookLevel> Bids { get; set; } = new();
        public List<OrderBookLevel> Asks { get; set; } = new();
        public decimal? BestBid => Bids.FirstOrDefault()?.Price;
        public decimal? BestAsk => Asks.FirstOrDefault()?.Price;
        public decimal? Spread => BestAsk.HasValue && BestBid.HasValue ? BestAsk - BestBid : null;
    }

    public class OrderBookLevel
    {
        public decimal Price { get; set; }
        public int TotalQuantity { get; set; }
        public int OrderCount { get; set; }
    }
}
