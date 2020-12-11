using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.API.Model
{
    public class ProductSKUModel
    {
        public string SkuId { get; set; }
        public decimal Price { get; set; }
        public long Stock { get; set; }
        public int cartCount { get; set; }
    }
}
