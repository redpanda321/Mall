using Mall.CommonModel;
using Mall.DTO.QueryModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mall.Entities.OrderInfo;

namespace Mall.DTO
{
    public class BrokerageOrderQuery : QueryBase
    {
        public long OrderId { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set;}
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        /// <summary>
        /// 分销员ID
        /// </summary>
        public long DistributorId { get; set; }
        /// <summary>
        /// 分销员名称
        /// </summary>
        public string DistributorName { get; set; }
        public OrderOperateStatus? OrderStatus { get; set; }
        /// <summary>
        /// 结算开始
        /// </summary>
        public DateTime? SettlementBegin { get; set; }
        /// <summary>
        /// 结算末尾
        /// </summary>
        public DateTime? SettlementEnd { get; set; }
        /// <summary>
        /// 结算状态
        /// </summary>
        public DistributionBrokerageStatus? SettlementStatus { get; set; }
    }
}
