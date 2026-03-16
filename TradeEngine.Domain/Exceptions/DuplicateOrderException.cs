using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeEngine.Domain.Exceptions
{
    public class DuplicateOrderException : Exception
    {
        public DuplicateOrderException()
            : base("Duplicate order detected.")
        {
        }

        public DuplicateOrderException(string idempotencyKey)
            : base($"Order with idempotency key '{idempotencyKey}' already exists.")
        {
        }
    }
}
