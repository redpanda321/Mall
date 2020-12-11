using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FlashSaleDetailInfo
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string SkuId { get; set; }
        public decimal Price { get; set; }
        public int TotalCount { get; set; }
        public long FlashSaleId { get; set; }
    }
}
