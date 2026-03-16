using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeEngine.Domain.Exceptions
{
    public class OrderNotFoundException : Exception
    {
        public OrderNotFoundException()
            : base("Order not found.")
        {
        }

        public OrderNotFoundException(Guid orderId)
            : base($"Order with ID {orderId} not found.")
        {
        }
    }
}
