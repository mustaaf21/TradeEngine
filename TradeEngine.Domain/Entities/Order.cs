using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradeEngine.Domain.Enums;

namespace TradeEngine.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        public string AssetSymbol { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public byte[] RowVersion { get; set; } = default!;
    }
}