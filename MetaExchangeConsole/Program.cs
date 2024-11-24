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


                string OrderTypeReply = string.Empty;
                string[] optionsBuy = { "B", "BUY"};
                string[] optionsSell = { "S", "SELL"};
                string bookQuestion = string.Format("In which book do you want to seach?");
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

                    bestAll = MetaExhangeCalculator.Instance.FindBestAll(type, ammount);

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
    }
}
