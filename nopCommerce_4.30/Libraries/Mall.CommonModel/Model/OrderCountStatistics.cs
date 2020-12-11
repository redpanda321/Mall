using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    /// <summary>
    /// 订单数量统计
    /// </summary>
    public class OrderStatisticsItem
    {
        /// <summary>
        /// 实付合计
        /// </summary>
        public decimal TotalActualPayAmount { get; set; }
        public int OrderCount { get; set; }
    }

    public enum OrderCountStatisticsFields
    {
        /// <summary>
        /// 订单实付
        /// </summary>
        ActualPayAmount=1,
        /// <summary>
        /// 订单数
        /// </summary>
        OrderCount=2,
    }
}
