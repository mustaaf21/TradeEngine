using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeEngine.Domain.Enums
{
    public enum OrderStatus
    {
        Created = 1,
        PartiallyFilled = 2,
        Executed = 3,
        Settled = 4,
        Cancelled = 5
    }
}
