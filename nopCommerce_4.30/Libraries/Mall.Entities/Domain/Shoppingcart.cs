using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShoppingCartInfo
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long ShopBranchId { get; set; }
        public long ProductId { get; set; }
        public string SkuId { get; set; }
        public long Quantity { get; set; }
        public DateTime AddTime { get; set; }
    }
}
