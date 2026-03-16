using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeEngine.Application.Interfaces;
using TradeEngine.Domain.Enums;
using TradeEngine.Domain.Exceptions;
using TradeEngine.Infrastructure.Persistence;

namespace TradeEngine.Infrastructure.Service
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly TradeEngineDbContext _context;
        private readonly IPriceService _priceService;

        public AnalyticsService(TradeEngineDbContext context, IPriceService priceService)
        {
            _context = context;
            _priceService = priceService;
        }

        public async Task<PnLReport> GetPnLReportAsync(Guid accountId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null)
            {
                throw new AccountNotFoundException();
            }

            var positions = await _context.Positions
                .Where(p => p.AccountId == accountId && p.Quantity > 0)
                .ToListAsync();

            var trades = await _context.Trades
                .Where(t => t.AccountId == accountId)
                .ToListAsync();

            decimal realizedPnL = 0;
            var positionPnLs = new List<PositionPnL>();

            var sellTrades = trades.Where(t => t.Side == OrderSide.Sell).ToList();
            foreach (var sellTrade in sellTrades)
            {
                var avgCostAtSale = await GetAverageCostAtTimeAsync(accountId, sellTrade.AssetSymbol, sellTrade.ExecutedAt);
                var costBasis = sellTrade.Quantity * avgCostAtSale;
                var saleProceeds = sellTrade.Quantity * sellTrade.ExecutionPrice;
                realizedPnL += saleProceeds - costBasis;
            }

            foreach (var position in positions)
            {
                var currentPrice = await _priceService.GetCurrentPriceAsync(position.AssetSymbol);

                positionPnLs.Add(new PositionPnL
                {
                    AssetSymbol = position.AssetSymbol,
                    Quantity = position.Quantity,
                    AverageCost = position.AverageCost,
                    CurrentPrice = currentPrice
                });
            }

            return new PnLReport
            {
                AccountId = accountId,
                RealizedPnL = realizedPnL,
                UnrealizedPnL = positionPnLs.Sum(p => p.UnrealizedPnL),
                PositionPnLs = positionPnLs,
                GeneratedAt = DateTime.UtcNow
            };
        }

        public async Task<PortfolioSummary> GetPortfolioSummaryAsync(Guid accountId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null)
            {
                throw new AccountNotFoundException();
            }

            var positions = await _context.Positions
                .Where(p => p.AccountId == accountId && p.Quantity > 0)
                .ToListAsync();

            var positionSummaries = new List<PositionSummary>();
            decimal totalPositionsValue = 0;

            foreach (var position in positions)
            {
                var currentPrice = await _priceService.GetCurrentPriceAsync(position.AssetSymbol);
                var marketValue = position.Quantity * currentPrice;
                totalPositionsValue += marketValue;

                positionSummaries.Add(new PositionSummary
                {
                    AssetSymbol = position.AssetSymbol,
                    Quantity = position.Quantity,
                    AverageCost = position.AverageCost,
                    CurrentPrice = currentPrice,
                    Weight = 0
                });
            }

            var totalPortfolioValue = account.Balance + totalPositionsValue;

            foreach (var ps in positionSummaries)
            {
                ps.Weight = totalPortfolioValue > 0 ? (ps.MarketValue / totalPortfolioValue) * 100 : 0;
            }

            return new PortfolioSummary
            {
                AccountId = accountId,
                CashBalance = account.Balance,
                AvailableCash = account.AvailableBalance,
                FrozenCash = account.FrozenBalance,
                TotalPositionsValue = totalPositionsValue,
                Positions = positionSummaries,
                GeneratedAt = DateTime.UtcNow
            };
        }

        public async Task<List<LedgerStatement>> GetLedgerStatementAsync(Guid accountId, DateTime? from = null, DateTime? to = null)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null)
            {
                throw new AccountNotFoundException();
            }

            var query = _context.LedgerEntries
                .Where(l => l.AccountId == accountId);

            if (from.HasValue)
            {
                query = query.Where(l => l.CreatedAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(l => l.CreatedAt <= to.Value);
            }

            var entries = await query.OrderBy(l => l.CreatedAt).ToListAsync();

            var statements = new List<LedgerStatement>();
            decimal runningBalance = 0;

            if (from.HasValue)
            {
                var priorEntries = await _context.LedgerEntries
                    .Where(l => l.AccountId == accountId && l.CreatedAt < from.Value)
                    .ToListAsync();

                runningBalance = priorEntries.Sum(e => e.Credit - e.Debit);
            }

            foreach (var entry in entries)
            {
                runningBalance += entry.Credit - entry.Debit;

                statements.Add(new LedgerStatement
                {
                    Id = entry.Id,
                    Debit = entry.Debit,
                    Credit = entry.Credit,
                    Description = entry.Description,
                    CreatedAt = entry.CreatedAt,
                    RunningBalance = runningBalance
                });
            }

            return statements;
        }

        private async Task<decimal> GetAverageCostAtTimeAsync(Guid accountId, string assetSymbol, DateTime asOfTime)
        {
            var buyTrades = await _context.Trades
                .Where(t => t.AccountId == accountId
                    && t.AssetSymbol == assetSymbol
                    && t.Side == OrderSide.Buy
                    && t.ExecutedAt < asOfTime)
                .ToListAsync();

            if (!buyTrades.Any())
                return 0;

            var totalCost = buyTrades.Sum(t => t.Quantity * t.ExecutionPrice);
            var totalQuantity = buyTrades.Sum(t => t.Quantity);

            return totalQuantity > 0 ? totalCost / totalQuantity : 0;
        }
    }
}
