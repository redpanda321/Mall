using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopBonusInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public decimal RandomAmountStart { get; set; }
        public decimal RandomAmountEnd { get; set; }
        public int UseState { get; set; }
        public decimal UsrStatePrice { get; set; }
        public decimal GrantPrice { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public DateTime BonusDateStart { get; set; }
        public DateTime BonusDateEnd { get; set; }
        public string ShareTitle { get; set; }
        public string ShareDetail { get; set; }
        public string ShareImg { get; set; }
        public byte SynchronizeCard { get; set; }
        public string CardTitle { get; set; }
        public string CardColor { get; set; }
        public string CardSubtitle { get; set; }
        public byte IsInvalid { get; set; }
        public int ReceiveCount { get; set; }
        public string Qrpath { get; set; }
        public int WxcardState { get; set; }
    }
}
