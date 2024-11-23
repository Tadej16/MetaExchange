using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaExchangeConsole.models
{
    public class MetaExhangeCalculator
    {
        private static Lazy<MetaExhangeCalculator> _lazyInstance = new Lazy<MetaExhangeCalculator>(() => new MetaExhangeCalculator());
        private static List<OrderBook> OrderBooks = null;

        public static MetaExhangeCalculator Instance => _lazyInstance.Value;

        public List<Order> FindBestAll(string type, decimal ammount, string path = null)
        {
            if (OrderBooks == null)
            {
                OrderBooks = ReadBooks(path);
            }
            decimal sumPrice = 0;
            decimal sumAmmount = 0;
            List<Order> best = null;
            foreach (var book in OrderBooks)
            {
                List<Order> best2 = FindBestOne(book, type, ammount);
                decimal sumPrice2 = best2.Sum(x => x.Price);
                decimal sumAmmount2 = best2.Sum(x => x.Amount);

                if (best == null ||
                    ((type.ToUpper().Equals("BUY") ? sumPrice2 < sumPrice : sumPrice < sumPrice2) &&
                    (sumAmmount2 > sumAmmount && sumAmmount2 <= ammount))
                    )
                {
                    best = best2;
                    sumPrice = sumPrice2;
                    sumAmmount = sumAmmount2;
                }
            }
            return best;
        }

        private List<Order> FindBestOne(OrderBook books, string type, decimal ammount)
        {
            List<Order> orders = type.ToUpper() == "BUY" ? books.Asks.Where(x => x.Order.Amount <= ammount).Select(x => x.Order).ToList() : books.Bids.Where(x => x.Order.Amount <= ammount).Select(x => x.Order).ToList();
            return FindBest(type, ammount, orders);
        }

        private static List<Order> FindBest(string type, decimal ammount, List<Order> orders)
        {
            orders = OrderByType(type, orders);

            Queue<Order> ordersQueue = new Queue<Order>(orders);

            decimal currentAmmount = 0;
            List<Order> ordersToExecute = new List<Order>();
            while (currentAmmount < ammount && ordersQueue.Count > 0)
            {
                Order order = ordersQueue.Dequeue();
                currentAmmount += order.Amount;
                ordersToExecute.Add(order);
                ordersQueue = new Queue<Order>(ordersQueue.Where(x => x.Amount <= ammount - currentAmmount));
            }
            return ordersToExecute;
        }

        private static List<Order> OrderByType(string type, List<Order> orders)
        {
            if (type.ToUpper().Equals("BUY"))
            {
                orders = orders.OrderBy(x => x.PricePerUnit).ToList();
            }
            else // ASKS
            {
                orders = orders.OrderByDescending(x => x.PricePerUnit).ToList();
            }

            return orders;
        }

        public static List<OrderBook> ReadBooks(string filePath = null)
        {
            List<OrderBook> ret = new List<OrderBook>();
            try
            {
#if DEBUG
                if (string.IsNullOrEmpty(filePath))
                {
                    if (Environment.OSVersion.Platform == PlatformID.Unix)
                    {
                        filePath = "/usr/local/bin/order_books_data/order_books_data";  // Linux path
                    }
                    else
                    {
                        filePath = @"C:\Users\Tadej\Documents\Projects\metaExchangeTask_backend_slo_2024\metaExchangeTask_backend_slo_2024\order_books_data\order_books_data";
                    }
                }
#endif
                using (var reader = new StreamReader(filePath))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        //line = _reader.ReadLine();

                        string json = line.Split("\t")[1];

                        // Deserialize JSON to OrderBook object
                        OrderBook book = System.Text.Json.JsonSerializer.Deserialize<OrderBook>(json);
                        book.Asks = book.Asks.OrderBy(x => x.Order.Price).ToList();
                        book.Bids = book.Bids.OrderByDescending(x => x.Order.Price).ToList();
                        ret.Add(book);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return ret;
        }
    }
}
