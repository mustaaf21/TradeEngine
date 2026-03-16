using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeEngine.Application.Interfaces;

namespace TradeEngine.Infrastructure.Service
{
    public class PriceService : IPriceService
    {
        private readonly ConcurrentDictionary<string, decimal> _prices = new();

        public PriceService()
        {
            _prices["AAPL"] = 150.00m;
            _prices["GOOGL"] = 2800.00m;
            _prices["MSFT"] = 300.00m;
            _prices["AMZN"] = 3300.00m;
            _prices["TSLA"] = 250.00m;
            _prices["META"] = 350.00m;
            _prices["NVDA"] = 450.00m;
            _prices["BTC"] = 45000.00m;
            _prices["ETH"] = 3000.00m;
        }

        public Task<decimal> GetCurrentPriceAsync(string assetSymbol)
        {
            if (_prices.TryGetValue(assetSymbol.ToUpperInvariant(), out var price))
            {
                return Task.FromResult(price);
            }

            return Task.FromResult(100.00m);
        }

        public Task<Dictionary<string, decimal>> GetCurrentPricesAsync(IEnumerable<string> assetSymbols)
        {
            var result = new Dictionary<string, decimal>();

            foreach (var symbol in assetSymbols)
            {
                var upperSymbol = symbol.ToUpperInvariant();
                result[upperSymbol] = _prices.TryGetValue(upperSymbol, out var price) ? price : 100.00m;
            }

            return Task.FromResult(result);
        }

        public void SetPrice(string assetSymbol, decimal price)
        {
            _prices[assetSymbol.ToUpperInvariant()] = price;
        }
    }
}
