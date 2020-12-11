using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class DistributionBrokerageInfo
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long OrderItemId { get; set; }
        public long ProductId { get; set; }
        public long MemberId { get; set; }
        public long ShopId { get; set; }
        public long Quantity { get; set; }
        public decimal RealPayAmount { get; set; }
        public int BrokerageStatus { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? SettlementTime { get; set; }
        public long SuperiorId1 { get; set; }
        public decimal BrokerageRate1 { get; set; }
        public long SuperiorId2 { get; set; }
        public decimal BrokerageRate2 { get; set; }
        public long SuperiorId3 { get; set; }
        public decimal BrokerageRate3 { get; set; }
    }
}
