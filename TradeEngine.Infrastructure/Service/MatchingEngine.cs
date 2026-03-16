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
    public class MatchingEngine : IMatchingEngine
    {
        private readonly TradeEngineDbContext _context;

        public MatchingEngine(TradeEngineDbContext context)
        {
            _context = context;
        }

        public async Task<List<Trade>> MatchOrderAsync(Guid orderId)
        {
            var trades = new List<Trade>();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    throw new OrderNotFoundException(orderId);
                }

                if (order.Status != OrderStatus.Created && order.Status != OrderStatus.PartiallyFilled)
                {
                    return trades;
                }

                var opposingSide = order.Side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;

                var matchingOrders = await GetMatchingOrdersAsync(order.AssetSymbol, opposingSide, order.Price, order.Side);

                foreach (var matchingOrder in matchingOrders)
                {
                    if (order.RemainingQuantity <= 0)
                        break;

                    var matchQuantity = Math.Min(order.RemainingQuantity, matchingOrder.RemainingQuantity);
                    var executionPrice = matchingOrder.Price;

                    var buyOrder = order.Side == OrderSide.Buy ? order : matchingOrder;
                    var sellOrder = order.Side == OrderSide.Sell ? order : matchingOrder;

                    var buyerTrade = await ExecuteMatchAsync(buyOrder, matchQuantity, executionPrice);
                    var sellerTrade = await ExecuteMatchAsync(sellOrder, matchQuantity, executionPrice);

                    trades.Add(buyerTrade);
                    trades.Add(sellerTrade);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return trades;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<List<Order>> GetMatchingOrdersAsync(string assetSymbol, OrderSide side, decimal price, OrderSide incomingOrderSide)
        {
            var query = _context.Orders
                .Where(o => o.AssetSymbol == assetSymbol
                    && o.Side == side
                    && (o.Status == OrderStatus.Created || o.Status == OrderStatus.PartiallyFilled)
                    && o.Type == OrderType.Limit);

            if (incomingOrderSide == OrderSide.Buy)
            {
                query = query.Where(o => o.Price <= price)
                    .OrderBy(o => o.Price)
                    .ThenBy(o => o.CreatedAt);
            }
            else
            {
                query = query.Where(o => o.Price >= price)
                    .OrderByDescending(o => o.Price)
                    .ThenBy(o => o.CreatedAt);
            }

            return await query.ToListAsync();
        }

        private async Task<Trade> ExecuteMatchAsync(Order order, int quantity, decimal price)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == order.AccountId);

            if (account == null)
            {
                throw new AccountNotFoundException();
            }

            var totalAmount = quantity * price;

            if (order.Side == OrderSide.Buy)
            {
                var frozenAmount = quantity * order.Price;
                account.UnfreezeFunds(frozenAmount);
                account.DeductBalance(totalAmount);

                var position = await _context.Positions
                    .FirstOrDefaultAsync(p => p.AccountId == account.Id && p.AssetSymbol == order.AssetSymbol);

                if (position == null)
                {
                    position = new Position
                    {
                        Id = Guid.NewGuid(),
                        AccountId = account.Id,
                        AssetSymbol = order.AssetSymbol,
                        Quantity = 0,
                        AverageCost = 0
                    };
                    _context.Positions.Add(position);
                }

                position.IncreasePosition(quantity, price);

                var ledgerEntry = new LedgerEntry
                {
                    Id = Guid.NewGuid(),
                    AccountId = account.Id,
                    Debit = totalAmount,
                    Credit = 0,
                    Description = $"Buy {quantity} {order.AssetSymbol} @ {price}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.LedgerEntries.Add(ledgerEntry);
            }
            else
            {
                var position = await _context.Positions
                    .FirstOrDefaultAsync(p => p.AccountId == account.Id && p.AssetSymbol == order.AssetSymbol);

                if (position == null || position.Quantity < quantity)
                {
                    throw new InsufficientPositionException(order.AssetSymbol, quantity, position?.Quantity ?? 0);
                }

                position.DecreasePosition(quantity);
                account.AddBalance(totalAmount);

                var ledgerEntry = new LedgerEntry
                {
                    Id = Guid.NewGuid(),
                    AccountId = account.Id,
                    Debit = 0,
                    Credit = totalAmount,
                    Description = $"Sell {quantity} {order.AssetSymbol} @ {price}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.LedgerEntries.Add(ledgerEntry);
            }

            order.MarkAsPartiallyFilled(quantity);

            var trade = new Trade
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                AccountId = order.AccountId,
                AssetSymbol = order.AssetSymbol,
                Side = order.Side,
                Quantity = quantity,
                ExecutionPrice = price,
                Status = TradeStatus.Executed,
                ExecutedAt = DateTime.UtcNow
            };

            _context.Trades.Add(trade);

            return trade;
        }

        public async Task<OrderBookSnapshot> GetOrderBookAsync(string assetSymbol)
        {
            var bids = await _context.Orders
                .Where(o => o.AssetSymbol == assetSymbol
                    && o.Side == OrderSide.Buy
                    && (o.Status == OrderStatus.Created || o.Status == OrderStatus.PartiallyFilled)
                    && o.Type == OrderType.Limit)
                .GroupBy(o => o.Price)
                .Select(g => new OrderBookLevel
                {
                    Price = g.Key,
                    TotalQuantity = g.Sum(o => o.Quantity - o.FilledQuantity),
                    OrderCount = g.Count()
                })
                .OrderByDescending(l => l.Price)
                .Take(10)
                .ToListAsync();

            var asks = await _context.Orders
                .Where(o => o.AssetSymbol == assetSymbol
                    && o.Side == OrderSide.Sell
                    && (o.Status == OrderStatus.Created || o.Status == OrderStatus.PartiallyFilled)
                    && o.Type == OrderType.Limit)
                .GroupBy(o => o.Price)
                .Select(g => new OrderBookLevel
                {
                    Price = g.Key,
                    TotalQuantity = g.Sum(o => o.Quantity - o.FilledQuantity),
                    OrderCount = g.Count()
                })
                .OrderBy(l => l.Price)
                .Take(10)
                .ToListAsync();

            return new OrderBookSnapshot
            {
                AssetSymbol = assetSymbol,
                Bids = bids,
                Asks = asks
            };
        }

        public async Task ProcessMarketOrderAsync(Guid orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    throw new OrderNotFoundException(orderId);
                }

                if (order.Type != OrderType.Market)
                {
                    throw new InvalidOrderStateException("Order is not a market order.");
                }

                var opposingSide = order.Side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;

                var matchingOrders = await _context.Orders
                    .Where(o => o.AssetSymbol == order.AssetSymbol
                        && o.Side == opposingSide
                        && (o.Status == OrderStatus.Created || o.Status == OrderStatus.PartiallyFilled)
                        && o.Type == OrderType.Limit)
                    .OrderBy(o => order.Side == OrderSide.Buy ? o.Price : -o.Price)
                    .ThenBy(o => o.CreatedAt)
                    .ToListAsync();

                foreach (var matchingOrder in matchingOrders)
                {
                    if (order.RemainingQuantity <= 0)
                        break;

                    var matchQuantity = Math.Min(order.RemainingQuantity, matchingOrder.RemainingQuantity);
                    var executionPrice = matchingOrder.Price;

                    await ExecuteMatchAsync(order, matchQuantity, executionPrice);
                    await ExecuteMatchAsync(matchingOrder, matchQuantity, executionPrice);
                }

                if (order.RemainingQuantity > 0)
                {
                    order.MarkAsCancelled();
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CheckStopOrdersAsync(string assetSymbol, decimal currentPrice)
        {
            var triggeredStopOrders = await _context.Orders
                .Where(o => o.AssetSymbol == assetSymbol
                    && (o.Type == OrderType.StopLoss || o.Type == OrderType.StopLimit)
                    && o.Status == OrderStatus.Created
                    && o.StopPrice.HasValue)
                .ToListAsync();

            foreach (var order in triggeredStopOrders)
            {
                bool shouldTrigger = false;

                if (order.Side == OrderSide.Sell && currentPrice <= order.StopPrice)
                {
                    shouldTrigger = true;
                }
                else if (order.Side == OrderSide.Buy && currentPrice >= order.StopPrice)
                {
                    shouldTrigger = true;
                }

                if (shouldTrigger)
                {
                    if (order.Type == OrderType.StopLoss)
                    {
                        order.Type = OrderType.Market;
                        await ProcessMarketOrderAsync(order.Id);
                    }
                    else if (order.Type == OrderType.StopLimit)
                    {
                        order.Type = OrderType.Limit;
                        await MatchOrderAsync(order.Id);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
