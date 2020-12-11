using Mall.Entities;
using System;

namespace Mall.Web.Areas.Web.Models
{
    /// <summary>
    /// 首页优惠券动态化
    /// </summary>
    public class Coupon
    {
        public string BaseShopName { get; set; }
        public long BaseShopId { get; set; }
        public decimal BasePrice { get; set; }
        public CouponType BaseType { get; set; }
        DateTime BaseEndTime { get; set; }
        public decimal OrderAmount { get; set; }
        public ShopBonusInfo.UseStateType UseState { get; set; }
        public decimal UsrStatePrice { get; set; }
    }
}