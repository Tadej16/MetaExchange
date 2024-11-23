﻿using MetaExchangeConsole.models;
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

                    List<Order> bestAll = null;

                    bestAll = FindBestAll(books, type, ammount);

                    Console.WriteLine("The best order combination for {0} is this: ", type);
                    Console.WriteLine(JsonConvert.SerializeObject(bestAll, Formatting.Indented));
                    decimal sumAmmount = bestAll.Sum(x => x.Amount);
                    decimal sumPrice = bestAll.Sum(x => x.Price);
                    Console.WriteLine("Summarized: Ammount({0}), Price({1})", sumAmmount, sumPrice);
                    
                    //Console.WriteLine("Or knapstack: ");
                    //Console.WriteLine(JsonConvert.SerializeObject(bestKnapsack, Formatting.Indented));
                    //sumAmmount = bestKnapsack.Sum(x => x.Amount);
                    //sumPrice = bestKnapsack.Sum(x => x.Price);
                    //Console.WriteLine("Summarized: Ammount({0}), Price({1})", sumAmmount, sumPrice);

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
        private static List<Order> FindBestAll(List<OrderBook> books, string type, decimal ammount)
        {
            decimal sumPrice = 0;
            decimal sumAmmount = 0;
            List<Order> best = null;
            foreach (var book in books)
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

        private static List<Order> FindBestGlobal(List<OrderBook> books, string type, decimal ammount)
        {
            List<Order> orders = books
            .SelectMany(book => type.ToUpper() == "BUY" ? book.Bids.Where(x => x.Order.Amount <= ammount).Select(a => a.Order) : book.Asks.Where(x => x.Order.Amount <= ammount).Select(b => b.Order))
            .ToList();
            return FindBest(type, ammount, orders);
        }

        private static List<Order> FindBestOne(OrderBook books, string type, decimal ammount)
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
