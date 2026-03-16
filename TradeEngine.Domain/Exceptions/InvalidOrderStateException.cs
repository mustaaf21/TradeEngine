using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeEngine.Domain.Exceptions
{
    public class InvalidOrderStateException : Exception
    {
        public InvalidOrderStateException()
            : base("Order is in an invalid state for this operation.")
        {
        }

        public InvalidOrderStateException(string message)
            : base(message)
        {
        }
    }
}
