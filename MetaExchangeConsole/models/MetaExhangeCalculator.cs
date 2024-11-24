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
        public static List<OrderBook> OrderBooks = null;

        public static MetaExhangeCalculator Instance => _lazyInstance.Value;

        public List<Order> FindBestAll(string type, decimal ammount, List<OrderBook> _orderBooks = null, string path = null)
        {
            if (OrderBooks == null && OrderBooks == null)
            {
                OrderBooks = ReadBooks(path);
            }
            if (_orderBooks != null)
            {
                OrderBooks = _orderBooks;
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

        public List<Order> FindBestOne(OrderBook books, string type, decimal ammount)
        {
            List<Order> orders = type.ToUpper() == "BUY" ? books.Asks.Where(x => x.Order.Amount <= ammount).Select(x => x.Order).ToList() : books.Bids.Where(x => x.Order.Amount <= ammount).Select(x => x.Order).ToList();
            if (type.ToUpper().Equals("BUY"))
            {
                return BestOne(type, ammount, orders);
            }
            else
            {
                return SolveKnapsack(books, type, ammount);
            }
        }

        public static List<Order> BestOne(string type, decimal ammount, List<Order> orders)
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
        public List<Order> SolveKnapsack(OrderBook books, string type, decimal ammount)
        {
            List<Order> items = type.ToUpper() == "BUY" ? books.Asks.Where(x => x.Order.Amount <= ammount).Select(x => x.Order).ToList() : books.Bids.Where(x => x.Order.Amount <= ammount).Select(x => x.Order).ToList();

            int n = items.Count;

            // Dynamic programming dictionary to handle decimal weights
            var dp = new List<Dictionary<decimal, decimal>>();
            for (int i = 0; i <= n; i++)
                dp.Add(new Dictionary<decimal, decimal>());

            dp[0][0] = 0;

            // Backtracking table to store item choices
            var itemSelection = new Dictionary<(int, decimal), bool>();

            // Fill the DP table
            for (int i = 1; i <= n; i++)
            {
                var currentItem = items[i - 1];
                foreach (var entry in dp[i - 1])
                {
                    decimal weight = entry.Key;
                    decimal value = entry.Value;

                    // Case 1: Exclude the item
                    if (!dp[i].ContainsKey(weight) || dp[i][weight] < value)
                        dp[i][weight] = value;

                    // Case 2: Include the item, if within capacity
                    decimal newWeight = weight + currentItem.Amount;
                    decimal newValue = value + currentItem.Price;

                    if (newWeight <= ammount)
                    {
                        if (!dp[i].ContainsKey(newWeight) || dp[i][newWeight] < newValue)
                        {
                            dp[i][newWeight] = newValue;
                            itemSelection[(i, newWeight)] = true;
                        }
                    }
                }
            }

            // Find the maximum value and reconstruct the selected items
            decimal maxValue = 0;
            decimal bestWeight = 0;
            foreach (var entry in dp[n])
            {
                if (entry.Value > maxValue)
                {
                    maxValue = entry.Value;
                    bestWeight = entry.Key;
                }
            }

            // Traceback to find selected items
            var selectedItems = new List<Order>();
            for (int i = n; i > 0 && bestWeight >= 0; i--)
            {
                if (itemSelection.ContainsKey((i, bestWeight)) && itemSelection[(i, bestWeight)])
                {
                    var item = items[i - 1];
                    selectedItems.Add(item);
                    bestWeight -= item.Amount;
                }
            }

            // Prepare the result
            selectedItems.Reverse();
            return selectedItems;
        }

        public static List<Order> OrderByType(string type, List<Order> orders)
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
