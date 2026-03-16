using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeEngine.Domain.Entities
{
    public class Position
    {
        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        public string AssetSymbol { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal AverageCost { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = default!;

        public void IncreasePosition(int quantity, decimal price)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive.", nameof(quantity));

            var totalCost = (AverageCost * Quantity) + (price * quantity);
            Quantity += quantity;
            AverageCost = Quantity > 0 ? totalCost / Quantity : 0;
            UpdatedAt = DateTime.UtcNow;
        }

        public void DecreasePosition(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive.", nameof(quantity));

            if (quantity > Quantity)
                throw new InvalidOperationException("Cannot decrease position by more than current quantity.");

            Quantity -= quantity;
            if (Quantity == 0)
                AverageCost = 0;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}