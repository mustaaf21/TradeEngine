using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeEngine.Application.Interfaces;
using TradeEngine.Domain.Entities;
using TradeEngine.Domain.Enums;
using TradeEngine.Domain.Exceptions;
using TradeEngine.Infrastructure.Persistence;

namespace TradeEngine.Infrastructure.Service
{
    public class SettlementService : ISettlementService
    {
        private readonly TradeEngineDbContext _context;
        private readonly TimeSpan _settlementDelay = TimeSpan.FromDays(1);

        public SettlementService(TradeEngineDbContext context)
        {
            _context = context;
        }

        public async Task SettleTradeAsync(Guid tradeId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var trade = await _context.Trades.FirstOrDefaultAsync(t => t.Id == tradeId);

                if (trade == null)
                {
                    throw new InvalidOperationException($"Trade {tradeId} not found.");
                }

                if (trade.Status == TradeStatus.Settled)
                {
                    return;
                }

                if (trade.ExecutedAt.Add(_settlementDelay) > DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Trade is not yet eligible for settlement.");
                }

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == trade.OrderId);

                if (order != null && order.Status == OrderStatus.Executed)
                {
                    order.MarkAsSettled();
                }

                trade.MarkAsSettled();

                var ledgerEntry = new LedgerEntry
                {
                    Id = Guid.NewGuid(),
                    AccountId = trade.AccountId,
                    Debit = 0,
                    Credit = 0,
                    Description = $"Settlement: {trade.Side} {trade.Quantity} {trade.AssetSymbol} @ {trade.ExecutionPrice}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.LedgerEntries.Add(ledgerEntry);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task SettlePendingTradesAsync()
        {
            var cutoffTime = DateTime.UtcNow.Subtract(_settlementDelay);

            var pendingTrades = await _context.Trades
                .Where(t => t.Status == TradeStatus.Executed && t.ExecutedAt <= cutoffTime)
                .ToListAsync();

            foreach (var trade in pendingTrades)
            {
                try
                {
                    await SettleTradeAsync(trade.Id);
                }
                catch
                {
                }
            }
        }

        public async Task<List<Trade>> GetPendingSettlementsAsync()
        {
            return await _context.Trades
                .Where(t => t.Status == TradeStatus.Executed)
                .OrderBy(t => t.ExecutedAt)
                .ToListAsync();
        }
    }
}
