using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    /// <summary>
    /// 订单结算详情
    //结算金额=商品实付+运费+税费-平台佣金-分销佣金-退款金额
    /// </summary>
    public class OrderSettlementDetail
    {
        /// <summary>
        /// 订单支付时间
        /// </summary>
        public string OrderPayTime { set; get; }

        /// <summary>
        /// 商品实付
        /// </summary>
        public decimal ProductsTotal { set; get; }
        /// <summary>
        /// 订单实付
        /// </summary>
        public decimal OrderAmount { set; get; }
        /// <summary>
        /// 订单运费
        /// </summary>
        public decimal Freight { set; get; }

        /// <summary>
        /// 平台佣金
        /// </summary>
        public decimal PlatCommission { set; get; }

        /// <summary>
        /// 分销佣金
        /// </summary>
        public decimal DistributorCommission { set; get; }

        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal RefundAmount { set; get; }
         /// <summary>
        /// 平台佣金退还
        /// </summary>
        public decimal PlatCommissionReturn { set; get; }
         
        /// <summary>
        /// 分销佣金退还
        /// </summary>
        public decimal DistributorCommissionReturn { set; get; }
        /// <summary>
        /// 退款确认时间
        /// </summary>
        public string OrderRefundTime { set; get; }

        /// <summary>
		/// 积分抵扣金额
		/// </summary>
        public decimal IntegralDiscount
        {
            get;set;
        }

    }
}
