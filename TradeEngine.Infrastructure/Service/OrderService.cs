using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeEngine.Application.DTOs;
using TradeEngine.Application.Interfaces;
using TradeEngine.Domain.Entities;
using TradeEngine.Domain.Enums;
using TradeEngine.Domain.Exceptions;
using TradeEngine.Infrastructure.Persistence;

namespace TradeEngine.Infrastructure.Service
{
    public class OrderService : IOrderService
    {
        private readonly TradeEngineDbContext _context;

        public OrderService(TradeEngineDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> PlaceBuyOrderAsync(PlaceOrderRequest request)
        {
            return await PlaceOrderAsync(request, OrderSide.Buy);
        }

        public async Task<Guid> PlaceSellOrderAsync(PlaceOrderRequest request)
        {
            return await PlaceOrderAsync(request, OrderSide.Sell);
        }

        public async Task<Guid> PlaceOrderAsync(PlaceOrderRequest request, OrderSide side)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (!string.IsNullOrEmpty(request.IdempotencyKey))
                {
                    var existingOrder = await _context.Orders
                        .FirstOrDefaultAsync(o => o.IdempotencyKey == request.IdempotencyKey);
                    if (existingOrder != null)
                    {
                        throw new DuplicateOrderException(request.IdempotencyKey);
                    }
                }

                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId);

                if (account == null)
                {
                    throw new AccountNotFoundException();
                }

                var requiredAmount = request.Quantity * request.Price;

                if (side == OrderSide.Buy)
                {
                    if (account.AvailableBalance < requiredAmount)
                    {
                        throw new InsufficientFundsException();
                    }
                    account.FreezeFunds(requiredAmount);
                }
                else
                {
                    var position = await _context.Positions
                        .FirstOrDefaultAsync(p => p.AccountId == request.AccountId && p.AssetSymbol == request.AssetSymbol);

                    if (position == null || position.Quantity < request.Quantity)
                    {
                        var available = position?.Quantity ?? 0;
                        throw new InsufficientPositionException(request.AssetSymbol, request.Quantity, available);
                    }
                }

                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    AccountId = request.AccountId,
                    AssetSymbol = request.AssetSymbol,
                    Quantity = request.Quantity,
                    FilledQuantity = 0,
                    Price = request.Price,
                    StopPrice = request.StopPrice,
                    Type = request.OrderType,
                    Side = side,
                    Status = OrderStatus.Created,
                    IdempotencyKey = request.IdempotencyKey,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Orders.Add(order);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return order.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ExecuteOrderAsync(Guid orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    throw new OrderNotFoundException(orderId);
                }

                if (!order.CanExecute)
                {
                    throw new InvalidOrderStateException($"Order {orderId} cannot be executed. Current status: {order.Status}");
                }

                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == order.AccountId);

                if (account == null)
                {
                    throw new AccountNotFoundException();
                }

                var totalAmount = order.Quantity * order.Price;

                if (order.Side == OrderSide.Buy)
                {
                    account.UnfreezeFunds(totalAmount);
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

                    position.IncreasePosition(order.Quantity, order.Price);

                    var ledgerEntry = new LedgerEntry
                    {
                        Id = Guid.NewGuid(),
                        AccountId = account.Id,
                        Debit = totalAmount,
                        Credit = 0,
                        Description = $"Buy {order.Quantity} {order.AssetSymbol} @ {order.Price}",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.LedgerEntries.Add(ledgerEntry);
                }
                else
                {
                    var position = await _context.Positions
                        .FirstOrDefaultAsync(p => p.AccountId == account.Id && p.AssetSymbol == order.AssetSymbol);

                    if (position == null || position.Quantity < order.Quantity)
                    {
                        var available = position?.Quantity ?? 0;
                        throw new InsufficientPositionException(order.AssetSymbol, order.Quantity, available);
                    }

                    position.DecreasePosition(order.Quantity);
                    account.AddBalance(totalAmount);

                    var ledgerEntry = new LedgerEntry
                    {
                        Id = Guid.NewGuid(),
                        AccountId = account.Id,
                        Debit = 0,
                        Credit = totalAmount,
                        Description = $"Sell {order.Quantity} {order.AssetSymbol} @ {order.Price}",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.LedgerEntries.Add(ledgerEntry);
                }

                var trade = new Trade
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    AccountId = order.AccountId,
                    AssetSymbol = order.AssetSymbol,
                    Side = order.Side,
                    Quantity = order.Quantity,
                    ExecutionPrice = order.Price,
                    Status = TradeStatus.Executed,
                    ExecutedAt = DateTime.UtcNow
                };

                _context.Trades.Add(trade);

                order.MarkAsExecuted();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CancelOrderAsync(Guid orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    throw new OrderNotFoundException(orderId);
                }

                if (!order.CanCancel)
                {
                    throw new InvalidOrderStateException($"Order {orderId} cannot be cancelled. Current status: {order.Status}");
                }

                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == order.AccountId);

                if (account == null)
                {
                    throw new AccountNotFoundException();
                }

                if (order.Side == OrderSide.Buy)
                {
                    var frozenAmount = order.RemainingQuantity * order.Price;
                    account.UnfreezeFunds(frozenAmount);
                }

                order.MarkAsCancelled();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Order?> GetOrderAsync(Guid orderId)
        {
            return await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<Order>> GetAccountOrdersAsync(Guid accountId, OrderStatus? status = null)
        {
            var query = _context.Orders.Where(o => o.AccountId == accountId);

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        }

        public async Task<List<Trade>> GetAccountTradesAsync(Guid accountId)
        {
            return await _context.Trades
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.ExecutedAt)
                .ToListAsync();
        }

        public async Task<List<Position>> GetAccountPositionsAsync(Guid accountId)
        {
            return await _context.Positions
                .Where(p => p.AccountId == accountId && p.Quantity > 0)
                .ToListAsync();
        }
    }
}
