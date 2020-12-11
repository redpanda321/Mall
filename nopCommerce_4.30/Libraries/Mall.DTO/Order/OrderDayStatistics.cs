using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    /// <summary>
    /// 订单单日统计
    /// </summary>
    public class OrderDayStatistics
    {
        /// <summary>
        /// 下单数
        /// </summary>
        public int OrdersNum { get; set; }
        /// <summary>
        /// 支付订单数
        /// </summary>
        public int PayOrdersNum { get; set; }
        /// <summary>
        /// 销售额
        /// </summary>
        public decimal SaleAmount { get; set; }
        
        
    }
}
