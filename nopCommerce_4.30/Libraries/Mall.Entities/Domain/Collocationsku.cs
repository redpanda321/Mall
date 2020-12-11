using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class CollocationSKUInfo
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string SkuId { get; set; }
        public long ColloProductId { get; set; }
        public decimal Price { get; set; }
        public decimal SkuPirce { get; set; }
    }
}
