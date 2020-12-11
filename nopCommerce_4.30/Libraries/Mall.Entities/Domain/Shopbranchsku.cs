using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopBranchSKUInfo
    {
        public int Id { get; set; }
        public long ProductId { get; set; }
        public string SkuId { get; set; }
        public long ShopId { get; set; }
        public long ShopBranchId { get; set; }
        public int Stock { get; set; }
        public int Status { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
