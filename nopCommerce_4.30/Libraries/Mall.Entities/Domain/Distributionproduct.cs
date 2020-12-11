using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class DistributionProductInfo
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long ShopId { get; set; }
        public int ProductStatus { get; set; }
        public decimal BrokerageRate1 { get; set; }
        public decimal BrokerageRate2 { get; set; }
        public decimal BrokerageRate3 { get; set; }
        public int SaleCount { get; set; }
        public decimal SaleAmount { get; set; }
        public decimal SettlementAmount { get; set; }
        public DateTime? AddDate { get; set; }
    }
}
