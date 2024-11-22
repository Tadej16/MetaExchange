using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaExchangeConsole.models
{
    public class Order
    {
        public string Type { get; set; } // Buy or Sell
        public string Kind { get; set; } // Limit or Market
        public decimal Amount { get; set; } // Amount of BTC
        public decimal Price { get; set; } // Price in EUR
    }
}
