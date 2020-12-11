using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ProductVistiInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public long ProductId { get; set; }
        public DateTime Date { get; set; }
        public long VistiCounts { get; set; }
        public long VisitUserCounts { get; set; }
        public long PayUserCounts { get; set; }
        public long SaleCounts { get; set; }
        public decimal SaleAmounts { get; set; }
        public long OrderCounts { get; set; }
        public short StatisticFlag { get; set; }
    }
}
