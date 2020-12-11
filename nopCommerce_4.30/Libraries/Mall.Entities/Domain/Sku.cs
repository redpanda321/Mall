using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class SkuInfo
    {
        public string Id { get; set; }
        public long AutoId { get; set; }
        public long ProductId { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string Version { get; set; }
        public string Sku1 { get; set; }
        public long Stock { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public string ShowPic { get; set; }
        public long SafeStock { get; set; }
    }
}
