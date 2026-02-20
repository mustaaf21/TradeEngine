using System;
using System.Collections.Generic;
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
    }
}