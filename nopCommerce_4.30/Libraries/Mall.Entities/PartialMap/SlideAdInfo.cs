using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class SlideAdInfo
    {
        /// <summary>
        /// 幻灯片类型
        /// </summary>
        public enum SlideAdType
        {
            /// <summary>
            /// 原数据
            /// </summary>
            [Description("原数据")]
            Initial = 0,
            /// <summary>
            /// 平台首页轮播图
            /// </summary>
            [Description("平台首页轮播图")]
            PlatformHome = 1,

            /// <summary>
            /// 平台限时购轮播图
            /// </summary>
            [Description("平台限时购轮播图")]
            PlatformLimitTime = 2,

            /// <summary>
            /// 店铺首页轮播图
            /// </summary>
            [Description("店铺首页轮播图")]
            ShopHome = 3,

            /// <summary>
            /// 微店轮播图
            /// </summary>
            [Description("微店轮播图")]
            VShopHome = 4,
            /// <summary>
            /// 微信首页轮播图
            /// </summary>
            [Description("微信首页轮播图")]
            WeixinHome = 5,

            /// <summary>
            /// 触屏版首页轮播图
            /// </summary>
            [Description("触屏版首页轮播图")]
            WapHome = 6,

            /// <summary>
            /// 触屏版微店首页轮播图
            /// </summary>
            [Description("触屏版微店首页轮播图")]
            WapShopHome = 7,

            /// <summary>
            /// IOS首页轮播图
            /// </summary>
            [Description("APP首页轮播图")]
            IOSShopHome = 8,

            /// <summary>
            ///APP首页图标图
            /// </summary>
            [Description("APP首页轮播图")]
            APPIcon = 10,

            /// <summary>
            /// APP积分商城轮播图
            /// </summary>
            [Description("APP积分商城轮播图")]
            AppGifts = 11,

            /// <summary>
            /// 引导页图
            /// </summary>
            [Description("APP积分商城轮播图")]
            AppGuide = 12,

            /// <summary>
            /// 周边门店轮播图
            /// </summary>
            [Description("周边门店轮播图")]
            NearShopBranchHome = 13,

            /// <summary>
            ///周边门店图标图
            /// </summary>
            [Description("周边门店图标图")]
            NearShopBranchIcon = 14,

            /// <summary>
            /// 周边门店广告位
            /// </summary>
            [Description("周边门店广告位")]
            NearShopBranchSpecial = 15,

            /// <summary>
            /// 周边门店轮播图2
            /// </summary>
            [Description("周边门店轮播图2")]
            NearShopBranchHome2 = 16
        }
    }
}
