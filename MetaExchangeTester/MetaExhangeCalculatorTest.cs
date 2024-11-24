using MetaExchangeConsole.models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace MetaExchangeTester
{
    public class MetaExhangeCalculatorTest
    {
        public OrderBook CreateOrderBook()
        {
            OrderBook book = new OrderBook();
            book.AcqTime = DateTime.Now;
            book.Asks = new List<OrderDetail>();
            book.Bids = new List<OrderDetail>();

            string kind = "Buy";
            for (int i = 0; i < 2; i++)
            {
                if (i > 0) 
                {
                    kind = "Sell";
                }
                for (int j = 1; j <= 5; j++)
                {
                    OrderDetail od = new OrderDetail();
                    od.Order = new Order();
                    od.Order.Amount = (decimal)j;
                    od.Order.Price = (decimal)Math.Pow(j, j);
                    od.Order.Kind = kind;

                    if (kind == "Buy")
                    {
                        book.Bids.Add(od);
                    }
                    else
                    {
                        book.Asks.Add(od);
                    }
                }
            }
            return book;
            /*
             * Result:
             * { Amount = 1, Price = 1, Kind = "Buy" }      { Amount = 1, Price = 1, Kind = "Sell" }
             * { Amount = 2, Price = 4, Kind = "Buy" }      { Amount = 2, Price = 4, Kind = "Sell" }
             * { Amount = 3, Price = 27, Kind = "Buy" }     { Amount = 3, Price = 27, Kind = "Sell" }
             * { Amount = 4, Price = 256, Kind = "Buy" }    { Amount = 4, Price = 256, Kind = "Sell" }
             * { Amount = 5, Price = 3125, Kind = "Buy" }   { Amount = 5, Price = 3125, Kind = "Sell" }
            //*/
        }

        private static List<Order> OptimalBuyOrders()
        {
            List<Order> expectedOrders = new List<Order>();
            Order o1 = new Order() { Amount = 1, Price = 1, Kind = "Sell" };
            Order o2 = new Order() { Amount = 2, Price = 4, Kind = "Sell" };
            Order o4 = new Order() { Amount = 5, Price = 3125, Kind = "Sell" };
            expectedOrders.Add(o1);
            expectedOrders.Add(o2);
            expectedOrders.Add(o4);
            expectedOrders = expectedOrders.OrderBy(x => x.Price).ToList();
            return expectedOrders;
        }

        private static List<Order> OptimalSellOrders()
        {
            List<Order> expectedOrders = new List<Order>();
            Order o1 = new Order() { Amount = 3, Price = 27, Kind = "Buy" };
            Order o4 = new Order() { Amount = 5, Price = 3125, Kind = "Buy" };
            expectedOrders.Add(o1);
            expectedOrders.Add(o4);
            expectedOrders = expectedOrders.OrderBy(x => x.Price).ToList();
            return expectedOrders;
        }

        [Fact]
        public void FindBestOneSellTest()
        {

            OrderBook book = CreateOrderBook();

            List<Order> givenOrders = MetaExhangeCalculator.Instance.FindBestOne(book, "Sell", 8).OrderBy(x => x.Price).ToList(); ;

            List<Order> expectedOrders = OptimalSellOrders();

            expectedOrders = expectedOrders.OrderBy(x => x.Price).ToList();

            string expectedOrdersJSON = JsonConvert.SerializeObject(expectedOrders);
            string givenOrdersJSON = JsonConvert.SerializeObject(givenOrders);
            Assert.Equal(expectedOrdersJSON, givenOrdersJSON);
        }

        [Fact]
        public void FindBestOneBuyTest()
        {

            OrderBook book = CreateOrderBook();

            List<Order> givenOrders = MetaExhangeCalculator.Instance.FindBestOne(book, "Buy", 8).OrderBy(x => x.Price).ToList(); ;

            List<Order> expectedOrders = OptimalBuyOrders();

            expectedOrders = expectedOrders.OrderBy(x => x.Price).ToList();

            string expectedOrdersJSON = JsonConvert.SerializeObject(expectedOrders);
            string givenOrdersJSON = JsonConvert.SerializeObject(givenOrders);
            Assert.Equal(expectedOrdersJSON, givenOrdersJSON);
        }

        [Fact]
        public void FindBestKnapstackBuyTest()
        {
            OrderBook book = CreateOrderBook();
            List<Order> givenOrders = MetaExhangeCalculator.Instance.SolveKnapsack(book, "Buy", 8).OrderBy(x => x.Price).ToList(); ;

            List<Order> expectedOrders = OptimalBuyOrders();

            expectedOrders = expectedOrders.OrderBy(x => x.Price).ToList();

            string expectedOrdersJSON = JsonConvert.SerializeObject(expectedOrders);
            string givenOrdersJSON = JsonConvert.SerializeObject(givenOrders);
            Assert.Equal(expectedOrdersJSON, givenOrdersJSON);
        }

        [Fact]
        public void FindBestKnapstackSellTest()
        {
            OrderBook book = CreateOrderBook();
            List<Order> givenOrders = MetaExhangeCalculator.Instance.SolveKnapsack(book, "Sell", 8).OrderBy(x => x.Price).ToList(); ;

            List<Order> expectedOrders = OptimalSellOrders();

            string expectedOrdersJSON = JsonConvert.SerializeObject(expectedOrders);
            string givenOrdersJSON = JsonConvert.SerializeObject(givenOrders);
            Assert.Equal(expectedOrdersJSON, givenOrdersJSON);
        }
    }
}
