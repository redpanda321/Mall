using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class ProductShopCategory
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long ShopCategoryId { get; set; }
        public string ShopCategoryName { get; set; }
    }
}
