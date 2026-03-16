using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeEngine.Domain.Enums;

namespace TradeEngine.Application.DTOs
{
    public class PlaceOrderRequest
    {
        public Guid AccountId { get; set; }

        public string AssetSymbol { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal? StopPrice { get; set; }

        public OrderType OrderType { get; set; } = OrderType.Limit;

        public string? IdempotencyKey { get; set; }
    }
}
