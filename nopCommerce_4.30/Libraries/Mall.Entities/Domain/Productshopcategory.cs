using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ProductShopCategoryInfo
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long ShopCategoryId { get; set; }
    }
}
