using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MarketServiceRecordInfo
    {
        public long Id { get; set; }
        public long MarketServiceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime BuyTime { get; set; }
        public int SettlementFlag { get; set; }
        public decimal Price { get; set; }
    }
}
