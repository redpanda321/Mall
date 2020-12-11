using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    /// <summary>
    /// 会员中心数据汇总
    /// </summary>
    public class OrderCountStatistics
    {
        /// <summary>
        /// 总订单数
        /// </summary>
        public int OrderCount { get; set; }
        /// <summary>
        /// 待支付
        /// </summary>
        public int WaitingForPay { get; set; }
        /// <summary>
        /// 待发货
        /// </summary>
        public int WaitingForDelivery { get; set; }
        /// <summary>
        /// 待评论
        /// </summary>
        public int WaitingForComments { get; set; }
        /// <summary>
        /// 待收货
        /// </summary>
        public int WaitingForRecieve { get; set; }
        /// <summary>
        /// 待自提
        /// </summary>
        public int WaitingForSelfPickUp { get; set; }
        /// <summary>
        /// 待售后数量
        /// </summary>
        public int RefundCount { get; set; }
    }
}
