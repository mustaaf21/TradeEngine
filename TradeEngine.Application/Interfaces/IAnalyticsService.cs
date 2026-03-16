using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeEngine.Application.Interfaces
{
    public interface IAnalyticsService
    {
        Task<PnLReport> GetPnLReportAsync(Guid accountId);
        Task<PortfolioSummary> GetPortfolioSummaryAsync(Guid accountId);
        Task<List<LedgerStatement>> GetLedgerStatementAsync(Guid accountId, DateTime? from = null, DateTime? to = null);
    }

    public class PnLReport
    {
        public Guid AccountId { get; set; }
        public decimal RealizedPnL { get; set; }
        public decimal UnrealizedPnL { get; set; }
        public decimal TotalPnL => RealizedPnL + UnrealizedPnL;
        public List<PositionPnL> PositionPnLs { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class PositionPnL
    {
        public string AssetSymbol { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal AverageCost { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal MarketValue => Quantity * CurrentPrice;
        public decimal CostBasis => Quantity * AverageCost;
        public decimal UnrealizedPnL => MarketValue - CostBasis;
        public decimal UnrealizedPnLPercent => CostBasis > 0 ? (UnrealizedPnL / CostBasis) * 100 : 0;
    }

    public class PortfolioSummary
    {
        public Guid AccountId { get; set; }
        public decimal CashBalance { get; set; }
        public decimal AvailableCash { get; set; }
        public decimal FrozenCash { get; set; }
        public decimal TotalPositionsValue { get; set; }
        public decimal TotalPortfolioValue => CashBalance + TotalPositionsValue;
        public List<PositionSummary> Positions { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class PositionSummary
    {
        public string AssetSymbol { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal AverageCost { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal MarketValue => Quantity * CurrentPrice;
        public decimal Weight { get; set; }
    }

    public class LedgerStatement
    {
        public Guid Id { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal NetAmount => Credit - Debit;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal RunningBalance { get; set; }
    }
}
