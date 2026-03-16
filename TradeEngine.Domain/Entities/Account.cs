using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TradeEngine.Domain.Entities
{
    public class Account
    {
        public Guid Id { get; set; }

        public string OwnerName { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public decimal Balance { get; set; }

        public decimal FrozenBalance { get; set; }

        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = default!;

        public decimal AvailableBalance => Balance - FrozenBalance;

        public void FreezeFunds(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));

            if (AvailableBalance < amount)
                throw new InvalidOperationException("Insufficient available funds to freeze.");

            FrozenBalance += amount;
        }

        public void UnfreezeFunds(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));

            if (FrozenBalance < amount)
                throw new InvalidOperationException("Cannot unfreeze more than frozen balance.");

            FrozenBalance -= amount;
        }

        public void DeductBalance(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));

            if (Balance < amount)
                throw new InvalidOperationException("Insufficient balance.");

            Balance -= amount;
        }

        public void AddBalance(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));

            Balance += amount;
        }
    }
}
