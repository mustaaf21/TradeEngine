using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeEngine.Domain.Enums
{
    public enum OrderType
    {
        Market = 1,
        Limit = 2,
        StopLoss = 3,
        StopLimit = 4
    }
}
