using System;
using Mall.CommonModel;

namespace Mall.DTO
{
    /// <summary>
    /// 购买营销服务
    /// </summary>
    public class MarketServiceModel
    {
        /// <summary>
        /// 营销服务
        /// </summary>
        public MarketType MarketType { get; set; }
        /// <summary>
        /// 店铺编号
        /// </summary>
        public long ShopId { get; set; }
        /// <summary>
        /// 到期时间
        /// <para>未购买返回null</para>
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 每月服务费用
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 是否已购买
        /// </summary>
        public bool IsBuy { get
            {
                bool result = false;
                if(EndTime.HasValue)
                {
                    result = true;
                }
                return result;
            }
        }
        /// <summary>
        /// 是否已过期
        /// </summary>
        public bool IsExpired
        {
            get
            {
                bool result = true;
                if(EndTime>DateTime.Now)
                {
                    result = false;
                }
                return result;
            }
        }

        /// <summary>
        /// 最后一次购买的价格
        /// </summary>
        public decimal LastBuyPrice { get; set; }
    }
}
