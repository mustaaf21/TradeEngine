using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeEngine.Application.Interfaces
{
    public interface IPriceService
    {
        Task<decimal> GetCurrentPriceAsync(string assetSymbol);
        Task<Dictionary<string, decimal>> GetCurrentPricesAsync(IEnumerable<string> assetSymbols);
        void SetPrice(string assetSymbol, decimal price);
    }
}
