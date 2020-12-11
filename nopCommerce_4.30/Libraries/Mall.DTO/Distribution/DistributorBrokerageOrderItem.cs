using Mall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mall.Entities.OrderInfo;
using Mall.Core;
namespace Mall.DTO
{
    public class DistributorBrokerageOrderItem
    {
        public long OrderItemId { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDefaultImage { get; set; }
        public string Sku { get; set; }
        public long Quantity { get; set; }
        public decimal RealPayAmount { get; set; }
        public string StatusText { get { return Status.ToDescription(); } }
        public long SuperiorId1 { get; set; }
        public decimal BrokerageRate1 { get; set; }
        public long SuperiorId2 { get; set; }
        public decimal BrokerageRate2 { get; set; }
        public long SuperiorId3 { get; set; }
        public decimal BrokerageRate3 { get; set; }
        public DateTime? SettlementTime { get; set; }
        /// <summary>
        /// 是否售后
        /// </summary>
        public bool IsHasRefund { get; set; }
        public DistributionBrokerageStatus Status { get; set; }
    }
}
