using MetaExchangeConsole.models;
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
                            type = "B";
                        }
                        else if (optionsSell.Any(option => option.Equals(OrderTypeReply)))
                        {
                            type = "S";
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
                        type.Equals("B") ? "many" : "much",
                        type.Equals("B") ? "buying" : "selling");
                    decimal ammount = 0;

                    Console.WriteLine(ammoutnQuestion);
                    while (decimal.TryParse(Console.ReadLine(), out ammount) == false || ammount <= 0)
                    {
                        Console.Clear();
                        Console.WriteLine("Not a number, try again...");
                        Console.WriteLine(ammoutnQuestion);
                    }

                    switch (type)
                    {
                        case "B":
                            var bestBuyGlobal = FindBestGlobal(books, type);
                            var bestBuyOne = FindBest(books[bookIndex - 1], type);
                            break;
                        case "S":
                            var bestSellGlobal = FindBestGlobal(books, type);
                            var bestSellOne = FindBest(books[bookIndex - 1], type);
                            break;
                        default:
                            Console.WriteLine("How did you get here?");
                            break;
                    }
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

        private static object FindBestGlobal(List<OrderBook> books, string type)
        {
            Console.WriteLine("Not implemented");
            return null;
        }

        private static object FindBest(OrderBook books, string type)
        {
            Console.WriteLine("Not implemented");
            return null;
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
                        OrderBook book = JsonSerializer.Deserialize<OrderBook>(json);
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
