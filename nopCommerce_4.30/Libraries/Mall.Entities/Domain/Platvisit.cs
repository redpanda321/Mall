using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class PlatVisitInfo
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public long VisitCounts { get; set; }
        public long OrderUserCount { get; set; }
        public long OrderCount { get; set; }
        public long OrderProductCount { get; set; }
        public decimal OrderAmount { get; set; }
        public long OrderPayUserCount { get; set; }
        public long OrderPayCount { get; set; }
        public long SaleCounts { get; set; }
        public decimal SaleAmounts { get; set; }
        public short StatisticFlag { get; set; }
    }
}
