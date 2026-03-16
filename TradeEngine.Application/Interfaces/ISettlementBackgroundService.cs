using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeEngine.Application.Interfaces
{
    public interface ISettlementBackgroundService
    {
        Task RunSettlementAsync(CancellationToken stoppingToken);
    }
}
