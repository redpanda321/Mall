using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class DistributionRankingInfo
    {
        public long Id { get; set; }
        public long BatchId { get; set; }
        public long MemberId { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public decimal Settlement { get; set; }
        public decimal NoSettlement { get; set; }
    }
}
