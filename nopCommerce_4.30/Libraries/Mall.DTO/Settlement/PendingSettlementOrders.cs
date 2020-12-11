using System;

namespace Mall.DTO
{
    /// <summary>
    /// 待结算订单
    /// </summary>
    public class PendingSettlementOrders
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public long OrderId { set; get; }

        /// <summary>
        /// 订单号字符串
        /// </summary>
        public string strOrderId { get { return OrderId.ToString(); } }

        /// <summary>
        /// 订单状态(目前只针对已完成订单)
        /// </summary>
        public string Status
        {
            set;
            get;
        }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { set; get; }

        /// <summary>
        /// 支付方式
        /// </summary>

        public string PaymentTypeName { set; get; }

        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal OrderAmount { get; set; }

        /// <summary>
        /// 平台佣金
        /// </summary>
        public decimal PlatCommission { set; get; }

        /// <summary>
        /// 分销佣金
        /// </summary>
        public decimal DistributorCommission { set; get; }


        /// <summary>
        /// 平台佣金退款
        /// </summary>
        public decimal PlatCommissionReturn { set; get; }


        /// <summary>
        /// 分销佣金退款
        /// </summary>
        public decimal DistributorCommissionReturn { set; get; }

        /// <summary>
        /// 运费
        /// </summary>
        public decimal FreightAmount { get; set; }

        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal RefundAmount { set; get; }

        /// <summary>
        /// 商家结算金额
        /// </summary>
        public decimal SettlementAmount { set; get; }

        /// <summary>
        /// 订单完成时间
        /// </summary>
        /// 
      
        public DateTime OrderFinshTime { set; get; }

        public string OrderFinshTimeStr { get { return OrderFinshTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        public decimal TaxAmount
        {
            get;
            set;
        }
        
        /// <summary>
        /// 积分抵扣金额
        /// </summary>
        
        public decimal IntegralDiscount
        {
            get;
            set;
        }
        /// <summary>
		/// 退款时间
		/// </summary>
        public DateTime? RefundDate
        {
            get;set;
        }
        /// <summary>
		/// 创建时间
		/// </summary>
        public DateTime CreateDate
        { get; set; }
        /// <summary>
		/// 退款时间
		/// </summary>
        public string RefundDateStr { get { return RefundDate.HasValue ? RefundDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty; } }
        /// <summary>
		/// 创建时间
		/// </summary>
        public string CreateDateStr { get { return CreateDate.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 结算周期范围(不在当前结算周期内的)
        /// </summary>
        public string SettlementCycle
        {
            set
            {
                if (value != null)  _SettlementCycle = value;
            }

            get { return _SettlementCycle; }
        }
        private string _SettlementCycle = "";
    }
}
