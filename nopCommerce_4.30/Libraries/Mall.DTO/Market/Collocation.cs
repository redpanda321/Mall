using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class Collocation
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ShortDesc { get; set; }
        public DateTime CreateTime { get; set; }

        public List<CollocationProduct> Products { get; set; }

    }

    public class CollocationProduct
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public int DisplaySequence { get; set; }
        public bool IsMain { get; set; }

        public List<CollocationSKU> SKUs { get; set; }
    }

    public class CollocationSKU
    {
        public long Id { get; set; }
        public string SKUId { get; set; }
        public decimal Price { get; set; }
        public decimal SKUPrice { get; set; }
    }
}
