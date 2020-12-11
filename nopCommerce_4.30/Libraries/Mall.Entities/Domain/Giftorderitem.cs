using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class GiftOrderItemInfo
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long GiftId { get; set; }
        public int Quantity { get; set; }
        public int SaleIntegral { get; set; }
        public string GiftName { get; set; }
        public decimal GiftValue { get; set; }
        public string ImagePath { get; set; }
    }
}
