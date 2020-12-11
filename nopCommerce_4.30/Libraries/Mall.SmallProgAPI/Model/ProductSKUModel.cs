using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.SmallProgAPI.Model
{
    public class ProductSKUModel
    {
        public string SkuId { get; set; }
        public decimal Price { get; set; }
        public long Stock { get; set; }
        public int cartCount { get; set; }
    }

    public class ProductSKUItem
    {
        public long ValueId { get; set; }
        public bool UseAttributeImage { get; set; }
        public string Value { get; set; }
        public string ImageUrl { get; set; }
    }
}
