using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopVistiInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public long? ShopBranchId { get; set; }
        public DateTime Date { get; set; }
        public long VistiCounts { get; set; }
        public long OrderUserCount { get; set; }
        public long OrderCount { get; set; }
        public long OrderProductCount { get; set; }
        public decimal OrderAmount { get; set; }
        public long OrderPayUserCount { get; set; }
        public long OrderPayCount { get; set; }
        public long SaleCounts { get; set; }
        public decimal SaleAmounts { get; set; }
        public long OrderRefundProductCount { get; set; }
        public decimal OrderRefundAmount { get; set; }
        public long OrderRefundCount { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal JointRate { get; set; }
        public short StatisticFlag { get; set; }
    }
}
