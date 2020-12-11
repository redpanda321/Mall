using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class FlashSaleDetailModel
    {
        public long Id { get; set; }

        public string SkuId { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public string Version { get; set; }

        public int Stock { get; set; }

        public int TotalCount { get; set; }

        public decimal CostPrice { get; set; }

        public decimal SalePrice { get; set; }

        public decimal Price { get; set; }

        public int minMath { get; set; }
    }
}
