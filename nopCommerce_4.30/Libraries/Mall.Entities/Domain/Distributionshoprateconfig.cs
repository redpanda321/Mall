using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class DistributionShopRateConfigInfo
    {
        public int Id { get; set; }
        public long ShopId { get; set; }
        public decimal BrokerageRate1 { get; set; }
        public decimal BrokerageRate2 { get; set; }
        public decimal BrokerageRate3 { get; set; }
    }
}
