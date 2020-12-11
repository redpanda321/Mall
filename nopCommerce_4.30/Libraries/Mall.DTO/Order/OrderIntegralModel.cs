using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.DTO
{
    /// <summary>
    /// 订单积分
    /// </summary>
    public class OrderIntegralModel
    {
        public OrderIntegralModel()
        {
            IntegralPerMoney = 0;
            UserIntegrals = 0;
        }

        /// <summary>
        /// 多少积分换一无
        /// <para>被别人用坏了</para>
        /// </summary>
        public decimal IntegralPerMoney { get; set; }
        /// <summary>
        /// 最多可抵
        /// </summary>
        public decimal userIntegralMaxDeductible { get; set; }
        /// <summary>
        /// 多少积分换一元
        /// </summary>
        public int integralPerMoneyRate { get; set; }
        /// <summary>
        /// 订单需要使用积分
        /// </summary>
        public decimal UserIntegrals { get; set; }
    }
}