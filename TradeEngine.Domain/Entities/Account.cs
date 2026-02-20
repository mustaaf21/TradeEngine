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

        public required string OwnerName { get; set; }

        public decimal Balance { get; set; }

        public decimal FrozenBalance { get; set; }

        [Timestamp]
        public required byte[] RowVersion { get; set; }
    }
}
