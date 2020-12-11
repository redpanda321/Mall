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
    public class DistributorBrokerageOrder
    {
        public long OrderId { get; set; }
        public OrderOperateStatus OrderStatus { get; set; }
        public string OrderStatusText { get { return OrderStatus.ToDescription(); } }
        public DateTime? SettlementTime { get; set; }
        public DistributionBrokerageStatus Status { get; set; }
        public string StatusText { get { return Status.ToDescription(); } }
        /// <summary>
        /// 分佣级别
        /// </summary>
        public int SuperiorLevel { get; set; }
        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal OrderAmount { get; set; }
        /// <summary>
        /// 佣金
        /// </summary>
        public decimal BrokerageAmount { get; set; }
        /// <summary>
        /// 是否售后导致订单关闭
        /// </summary>
        public bool IsRefundCloseOrder { get; set; }
        public DateTime OrderDate { get; set; }
        public long QuantitySum { get; set; }
        public string OrderDateText
        {
            get
            {
                return OrderDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        public List<DistributorBrokerageOrderItem> Items { get; set; }
    }
}
