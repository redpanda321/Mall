using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class GiftInfo
    {
        public long Id { get; set; }
        public string GiftName { get; set; }
        public int NeedIntegral { get; set; }
        public int LimtQuantity { get; set; }
        public int StockQuantity { get; set; }
        public DateTime EndDate { get; set; }
        public int NeedGrade { get; set; }
        public int VirtualSales { get; set; }
        public int RealSales { get; set; }
        public GiftSalesStatus SalesStatus { get; set; }
        public string ImagePath { get; set; }
        public int Sequence { get; set; }
        public decimal GiftValue { get; set; }
        public string Description { get; set; }
        public DateTime AddDate { get; set; }
    }
}
