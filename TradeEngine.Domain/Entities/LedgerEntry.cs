using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeEngine.Domain.Entities
{
    public class LedgerEntry
    {
        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}