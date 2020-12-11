using System;

namespace Mall.DTO.QueryModel
{
    /// <summary>
    /// 店铺结算历史查询
    /// </summary>
    public class ShopSettledHistoryQuery : QueryBase
    {
        /// <summary>
        /// 店铺Id
        /// </summary>
        public long ShopId { set; get; }

        /// <summary>
        /// 查询在这个时间之后的历史记录
        /// </summary>
        public DateTime MinSettleTime { set; get; }
    }
}
