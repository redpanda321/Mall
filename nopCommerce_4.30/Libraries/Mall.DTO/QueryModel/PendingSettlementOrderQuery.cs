using System;

namespace Mall.DTO.QueryModel
{
    /// <summary>
    /// 待结算订单查询实体
    /// </summary>
    public class PendingSettlementOrderQuery : QueryBase
    {
       /// <summary>
       /// 订单ID
       /// </summary>
        public long? OrderId { set; get; }

        /// <summary>
        /// 店铺ID
        /// </summary>
        public long? ShopId { set; get; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { set; get; }

        /// <summary>
        /// 支付方式
        /// </summary>
        public string PaymentName { set; get; }

        /// <summary>
        /// 订单完成开始时间
        /// </summary>
        public DateTime? OrderStart { set; get; }
        /// <summary>
        /// 订单完成结束时间
        /// </summary>
        public DateTime? OrderEnd { set; get; }

        /// <summary>
        /// 待结算创建开始时间
        /// </summary>
        public DateTime? CreateDateStart { set; get; }
        /// <summary>
        /// 待结算创建结束时间
        /// </summary>
        public DateTime? CreateDateEnd { set; get; }

    }
}
