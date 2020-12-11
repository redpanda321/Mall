using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class PlatAccountItemInfo
    {
        public long Id { get; set; }
        public string AccountNo { get; set; }
        public long AccoutId { get; set; }
        public DateTime CreateTime { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public int TradeType { get; set; }
        public short IsIncome { get; set; }
        public string ReMark { get; set; }
        public string DetailId { get; set; }
    }
}
