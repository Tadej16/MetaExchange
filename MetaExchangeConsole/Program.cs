using MetaExchangeConsole.models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MetaExchangeConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("JSON path (ENTER for default)...");
                List<OrderBook> books = ReadBooks(Console.ReadLine());


                string OrderTypeReply = string.Empty;
                string[] optionsBuy = { "B", "BUY"};
                string[] optionsSell = { "S", "SELL"};
                string bookQuestion = string.Format("In which book do you want to seach? (1 - {0})", books.Count);
                string OrderTypeQuestion = "Are you buying or selling?";

                string type = string.Empty;

                bool repeat = true;
                while (repeat)
                {
                    Console.WriteLine(OrderTypeQuestion);

                    do
                    {
                        OrderTypeReply = Console.ReadLine().ToUpper();
                        if (optionsBuy.Any(option => option.Equals(OrderTypeReply)))
                        {
                            type = "Buy";
                        }
                        else if (optionsSell.Any(option => option.Equals(OrderTypeReply)))
                        {
                            type = "Sell";
                        }
                        else
                        {
                            OrderTypeReply = string.Empty;
                            Console.Clear();
                            Console.WriteLine("The selected option does not exist...");
                            Console.WriteLine(OrderTypeQuestion);
                        }
                    }
                    while (string.IsNullOrEmpty(OrderTypeReply));

                    int bookIndex = 0;
                    Console.WriteLine(bookQuestion);
                    while (int.TryParse(Console.ReadLine(), out bookIndex) == false || bookIndex > books.Count || bookIndex <= 0)
                    {
                        Console.Clear();
                        Console.WriteLine("Wrong input, try again...");
                        Console.WriteLine(bookQuestion);
                    }

                    string ammoutnQuestion = string.Format("How {0} are you {1}",
                        type.Equals("Buy") ? "many" : "much",
                        type.Equals("Buy") ? "buying" : "selling");
                    decimal ammount = 0;

                    Console.WriteLine(ammoutnQuestion);
                    while (decimal.TryParse(Console.ReadLine(), out ammount) == false || ammount <= 0)
                    {
                        Console.Clear();
                        Console.WriteLine("Not a number, try again...");
                        Console.WriteLine(ammoutnQuestion);
                    }

                    List<Order> bestGlobal = null;
                    List<Order> bestOne = null;

                    switch (type)
                    {
                        case "Buy":
                            bestGlobal = FindBestGlobal(books, type, ammount);
                            bestOne = FindBestOne(books[bookIndex - 1], type, ammount);
                            break;
                        case "Sell":
                            bestGlobal = FindBestGlobal(books, type, ammount);
                            bestOne = FindBestOne(books[bookIndex - 1], type, ammount);
                            break;
                        default:
                            Console.WriteLine("How did you get here?");
                            break;
                    }

                    Console.WriteLine("The best order for {0} for the selected book is this: ", type);
                    Console.WriteLine(JsonConvert.SerializeObject(bestOne, Formatting.Indented));
                    decimal sumAmmount = bestOne.Sum(x => x.Amount);
                    decimal sumPrice = bestOne.Sum(x => x.Price);
                    Console.WriteLine("Summarized: Ammount({0}), Price({1})", sumAmmount, sumPrice);
                    Console.WriteLine("Or this for all books: ");
                    Console.WriteLine(JsonConvert.SerializeObject(bestGlobal, Formatting.Indented));
                    sumAmmount = bestGlobal.Sum(x => x.Amount);
                    sumPrice = bestGlobal.Sum(x => x.Price);
                    Console.WriteLine("Summarized: Ammount({0}), Price({1})", sumAmmount, sumPrice);
                    Console.WriteLine("type 'EXIT' to exit");
                    if (Console.ReadLine().ToUpper().Equals("EXIT"))
                    {
                        repeat = false;
                    }
                    else
                    {
                        Console.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Console.ReadKey();
            }
        }

        private static List<Order> FindBestGlobal(List<OrderBook> books, string type, decimal ammount)
        {
            List<Order> orders = books
            .SelectMany(book => type.ToUpper() == "BUY" ? book.Bids.Where(x => x.Order.Amount <= ammount).Select(a => a.Order) : book.Asks.Where(x => x.Order.Amount <= ammount).Select(b => b.Order))
            .ToList();
            return FindBest(type, ammount, orders);
        }

        private static List<Order> FindBestOne(OrderBook books, string type, decimal ammount)
        {
            List<Order> orders = type.ToUpper() == "BUY" ? books.Bids.Where(x => x.Order.Amount <= ammount).Select(x => x.Order).ToList() : books.Asks.Where(x => x.Order.Amount <= ammount).Select(x => x.Order).ToList();
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
                    filePath = @"C:\Users\Tadej\Documents\Projects\metaExchangeTask_backend_slo_2024\metaExchangeTask_backend_slo_2024\order_books_data\order_books_data";
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
                Console.WriteLine(e.Message);
            }
            return ret;
        }
    }
}
