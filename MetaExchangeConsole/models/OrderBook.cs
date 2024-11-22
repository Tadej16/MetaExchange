using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaExchangeConsole.models
{
    public class OrderBook
    {
        public DateTime AcqTime { get; set; }
        public List<OrderDetail> Bids { get; set; }
        public List<OrderDetail> Asks { get; set; }
    }
}
