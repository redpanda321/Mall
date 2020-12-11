using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class BonusInfo
    {
        public long Id { get; set; }
        public int Type { get; set; }
        public int Style { get; set; }
        public string Name { get; set; }
        public string MerchantsName { get; set; }
        public string Remark { get; set; }
        public string Blessing { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Qrpath { get; set; }
        public int PriceType { get; set; }
        public decimal FixedAmount { get; set; }
        public decimal RandomAmountStart { get; set; }
        public decimal RandomAmountEnd { get; set; }
        public int ReceiveCount { get; set; }
        public string ImagePath { get; set; }
        public string Description { get; set; }
        public byte IsInvalid { get; set; }
        public decimal ReceivePrice { get; set; }
        public string ReceiveHref { get; set; }
        public byte IsAttention { get; set; }
        public byte IsGuideShare { get; set; }
    }
}
