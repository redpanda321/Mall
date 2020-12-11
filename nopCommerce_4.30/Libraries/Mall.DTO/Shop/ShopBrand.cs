using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class ShopBrand
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public long BrandId { get; set; }
        public string BrandName { get; set; }
    }
}
