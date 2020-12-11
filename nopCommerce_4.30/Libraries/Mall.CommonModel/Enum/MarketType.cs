using System.ComponentModel;

namespace Mall.CommonModel
{
    public enum MarketType : int
    {
        /// <summary>
        /// 限时购
        /// </summary>
        [Description("限时购")]
        LimitTimeBuy = 1,

        /// <summary>
        /// 优惠券
        /// </summary>
        [Description("优惠券")]
        Coupon=2,

          /// <summary>
        /// 组合购
        /// </summary>
        [Description("组合购")]
        Collocation=3,

        /// <summary>
        /// 随机红包
        /// </summary>
        [Description( "代金红包" )]
        RandomlyBonus = 4,

        /// <summary>
        /// 拼团
        /// </summary>
        [Description("拼团")]
        FightGroup = 5,
        /// <summary>
        /// 满额减
        /// </summary>
        [Description("满额减")]
        FullDiscount = 6,
    }
}
