// Data/ProductRepository.cs
using System;
using System.Collections.Generic;
using System.Threading;

namespace LegacyOrderService.Data
{
    public class ProductRepository
    {
        
        private readonly Dictionary<string, decimal> _productPrices = new(StringComparer.OrdinalIgnoreCase)  // this make the product retrieve at least not key sensitive 
        {
            ["Widget"] = 12.99m,
            ["Gadget"] = 15.49m,
            ["Doohickey"] = 8.75m
        };

        public decimal GetPrice(string productName)   // double doesn't make sense for a price 
        {
            // Simulate an expensive lookup
            Thread.Sleep(500);

            
            if (_productPrices.TryGetValue(productName, out var price))
                return price;

            throw new Exception("Product not found");
        }
    }
}
