using System;
using System.Collections.Generic;

namespace BestBuyProductAvailabilityNotifier
{
    public class ProductSearchResultModel
    {
        public IEnumerable<Product> products { get; set; }

        public override string ToString()
        {
            return "Products: " + Environment.NewLine + string.Join(Environment.NewLine, products);
        }
    }
}
