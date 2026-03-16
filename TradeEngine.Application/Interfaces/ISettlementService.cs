using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeEngine.Domain.Entities;

namespace TradeEngine.Application.Interfaces
{
    public interface ISettlementService
    {
        Task SettleTradeAsync(Guid tradeId);
        Task SettlePendingTradesAsync();
        Task<List<Trade>> GetPendingSettlementsAsync();
    }
}
