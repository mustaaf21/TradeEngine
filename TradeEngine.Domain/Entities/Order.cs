using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public int FilledQuantity { get; set; }

        public decimal Price { get; set; }

        public decimal? StopPrice { get; set; }

        public OrderType Type { get; set; } = OrderType.Limit;

        public OrderSide Side { get; set; }

        public OrderStatus Status { get; set; }

        public string? IdempotencyKey { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ExecutedAt { get; set; }

        public DateTime? SettledAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = default!;

        public int RemainingQuantity => Quantity - FilledQuantity;

        public decimal TotalValue => Quantity * Price;

        public bool CanExecute => Status == OrderStatus.Created;

        public bool CanCancel => Status == OrderStatus.Created || Status == OrderStatus.PartiallyFilled;

        public void MarkAsExecuted()
        {
            Status = OrderStatus.Executed;
            ExecutedAt = DateTime.UtcNow;
        }

        public void MarkAsPartiallyFilled(int filledQty)
        {
            FilledQuantity += filledQty;
            if (FilledQuantity >= Quantity)
            {
                MarkAsExecuted();
            }
            else
            {
                Status = OrderStatus.PartiallyFilled;
            }
        }

        public void MarkAsCancelled()
        {
            Status = OrderStatus.Cancelled;
        }

        public void MarkAsSettled()
        {
            Status = OrderStatus.Settled;
            SettledAt = DateTime.UtcNow;
        }
    }
}