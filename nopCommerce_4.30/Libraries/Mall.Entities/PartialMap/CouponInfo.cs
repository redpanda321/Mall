using NPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class CouponInfo
    {
        /// <summary>
        /// 领取方式
        /// </summary>
        public enum CouponReceiveType
        {
            /// <summary>
            /// 店铺首页
            /// </summary>
            [Description("店铺首页")]
            ShopIndex = 0,
            /// <summary>
            /// 积分兑换
            /// </summary>
            [Description("积分兑换")]
            IntegralExchange = 1,
            /// <summary>
            /// 主动发放
            /// </summary>
            [Description("主动发放")]
            DirectHair = 2
        }
        /// <summary>
        /// 领取状态
        /// </summary>
        public enum CouponReceiveStatus
        {
            Success = 1,
            /// <summary>
            /// 已过期
            /// </summary>
            HasExpired = 2,
            /// <summary>
            /// 已超额
            /// <para>达个人领取上限</para>
            /// </summary>
            HasLimitOver = 3,
            /// <summary>
            /// 已领完
            /// <para>优惠券已领完</para>
            /// </summary>
            HasReceiveOver = 4,
            /// <summary>
            /// 积分不足
            /// </summary>
            IntegralLess = 5
        }

        /// <summary>
        /// 微信关联信息
        /// </summary>
        [ResultColumn]
        public WXCardLogInfo WXCardInfo { get; set; }

        #region 表单传参
        /// <summary>
        /// 是否同步微信
        /// <para>仅限表单使用</para>
        /// </summary>
        [ResultColumn]
        public bool FormIsSyncWeiXin { get; set; }
        [ResultColumn]
        public string FormWXColor { get; set; }
        [ResultColumn]
        public string FormWXCTit { get; set; }
        [ResultColumn]
        public string FormWXCSubTit { get; set; }
        /// <summary>
        /// 是否可以发布到微店首页
        /// </summary>
        [ResultColumn]
        public bool CanVshopIndex { get; set; }

        [ResultColumn]
        public string ProductIds { get; set; }
        #endregion



        /// <summary>
        /// 
        /// </summary>
        [ResultColumn]
        [Obsolete("关联属性移除遗留")]
        public List<CouponProductInfo> CouponProductInfo { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        [ResultColumn]
        [Obsolete("关联属性移除遗留")]
        public List<CouponSettingInfo> CouponSettingInfo { get; set; }
    }

    public class CouponRecord
    {
        public long CouponId { get; set; }

        public string CouponName { get; set; }

        public decimal Price { get; set; }

        public string StartTime { get; set; }

        public string ClosingTime { get; set; }

        /// <summary>
        /// 用户领取状态(0:可用，1:已领取，2:已领完)
        /// </summary>
        public int IsUse { get; set; }

        public string Remark { get; set; }
        public int UseArea { get; set; }
        public string OrderAmount { get; set; }
    }
}
