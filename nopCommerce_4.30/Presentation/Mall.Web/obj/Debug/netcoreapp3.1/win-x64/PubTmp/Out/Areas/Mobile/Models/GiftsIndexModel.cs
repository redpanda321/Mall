using Mall.DTO;
using Mall.Entities;
using System.Collections.Generic;

namespace Mall.Web.Areas.Mobile.Models
{
    public class GiftsIndexModel
    {
        public GiftsIndexModel()
        {
            SlideAds = new List<SlideAdInfo>();
            WeiActives = new List<IntegralMallAdInfo>();
            HomeGiftses = new List<GiftModel>();
            IntegralCoupons = new List<CouponGetIntegralCouponModel>();
        }
        /// <summary>
        /// 轮播图
        /// </summary>
        public List<SlideAdInfo> SlideAds { get; set; }
        /// <summary>
        /// 大转盘刮刮卡
        /// </summary>
        public List<IntegralMallAdInfo> WeiActives { get; set; }
        /// <summary>
        /// 首页礼品
        /// </summary>
        public List<GiftModel> HomeGiftses { get; set; }
        /// <summary>
        /// 显示更多
        /// </summary>
        public bool HasMoreGifts { get; set; }
        /// <summary>
        /// 积分优惠券
        /// </summary>
        public List<CouponGetIntegralCouponModel> IntegralCoupons { get; set; }
        /// <summary>
        /// 显示更多
        /// </summary>
        public bool HasMoreIntegralCoupons { get; set; }
        /// <summary>
        /// 当前用户可用积分
        /// </summary>
        public int MemberAvailableIntegrals { get; set; }
        /// <summary>
        /// 用户等级
        /// </summary>
        public string MemberGradeName { get; set; }
        public bool HasLogined { get; set; }
    }
}