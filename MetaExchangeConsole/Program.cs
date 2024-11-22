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
        private static StreamReader _reader;
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("JSON path (ENTER for default)...");
                List<OrderBook> books = ReadBooks(Console.ReadLine());

                var bestSellGlobal = FindBestSellGlobal(books);

                int bookIndex = 0;
                
                Console.WriteLine("In which book do you want to seach? (1 - {1})", books.Count);
                while (int.TryParse(Console.ReadLine(), out bookIndex) == false && bookIndex <= books.Count)
                {
                    Console.Clear();
                    Console.WriteLine("Wrong input, try again...");
                    Console.WriteLine("In which book do you want to seach? (1 - {1})", books.Count);
                }
                
                var bestSellOne = FindBestSell(bookIndex - 1, books);
                Console.WriteLine(books);
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
        public static List<OrderBook> ReadBooks(string filePath = null)
        {
            List<OrderBook> ret = new List<OrderBook>();
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = @"C:\Users\Tadej\Documents\Projects\metaExchangeTask_backend_slo_2024\metaExchangeTask_backend_slo_2024\order_books_data\order_books_data";
                }
                using (var reader = new StreamReader(filePath))
                {
                    string? line;
                    while ((line = _reader.ReadLine()) != null)
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
