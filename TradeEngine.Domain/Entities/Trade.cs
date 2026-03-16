using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradeEngine.Domain.Enums;

namespace TradeEngine.Domain.Entities
{
    public class Trade
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public Guid AccountId { get; set; }

        public string AssetSymbol { get; set; } = string.Empty;

        public OrderSide Side { get; set; }

        public int Quantity { get; set; }

        public decimal ExecutionPrice { get; set; }

        public decimal TotalValue => Quantity * ExecutionPrice;

        public TradeStatus Status { get; set; }

        public DateTime ExecutedAt { get; set; }

        public DateTime? SettledAt { get; set; }

        public void MarkAsSettled()
        {
            Status = TradeStatus.Settled;
            SettledAt = DateTime.UtcNow;
        }
    }
}
