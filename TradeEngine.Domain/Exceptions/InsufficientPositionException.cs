using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeEngine.Domain.Exceptions
{
    public class InsufficientPositionException : Exception
    {
        public InsufficientPositionException()
            : base("Insufficient position quantity for this operation.")
        {
        }

        public InsufficientPositionException(string assetSymbol, int requested, int available)
            : base($"Insufficient position for {assetSymbol}. Requested: {requested}, Available: {available}")
        {
        }
    }
}
