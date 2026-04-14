using System;
using LegacyOrderService.Models;
using LegacyOrderService.Data;
using System.Globalization;



namespace LegacyOrderService
{
    
    
    class Program
    {
        static void Main(string[] args)
        {
            // Without this config, for some european culture the Price will be converted as 99,99 instead of 99.99 (example)  and this will brake the SQL query
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            
            Console.WriteLine("Welcome to Order Processor!");
            Console.WriteLine("Enter customer name:");
            string name = Console.ReadLine();

            Console.WriteLine("Enter product name:");
            string product = Console.ReadLine();
            var productRepo = new ProductRepository();
            
            // price is used only to be printed - fix assigned price to the order 
            decimal price = productRepo.GetPrice(product);  


            Console.WriteLine("Enter quantity:");
            int qty = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Processing order...");

            Order order = new Order();
            order.CustomerName = name;
            order.ProductName = product;
            order.Quantity = qty;
            order.Price = price;    //  here the field indicate price but is not clear if the price is unitPrice or the Total. It would be better add the Total as an other filed 

            var total = order.Quantity * order.Price;  // total is calculated but never used 

            Console.WriteLine("Order complete!");
            Console.WriteLine("Customer: " + order.CustomerName);
            Console.WriteLine("Product: " + order.ProductName);
            Console.WriteLine("Quantity: " + order.Quantity);
            Console.WriteLine("Total: $" + total);   //   now total is correct on screen but not saved. 

            Console.WriteLine("Saving order to database...");
            var repo = new OrderRepository();
            repo.Save(order);
            Console.WriteLine("Done.");
        }
    }
}
