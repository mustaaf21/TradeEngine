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
        Executed = 2,
        Settled = 3,
        Cancelled = 4
    }

    public enum TradeStatus
    {
        Executed = 1,
        Settled = 2
    }
}
