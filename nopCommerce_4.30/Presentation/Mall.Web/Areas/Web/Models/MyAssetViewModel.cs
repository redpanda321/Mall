using Mall.CommonModel;
using Mall.DTO;
using System.Collections.Generic;

namespace Mall.Web.Areas.Web.Models
{
    public class MyAssetViewModel
    {
        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool isLogin { get; set; }
        /// <summary>
        /// 我关注的商品
        /// </summary>
        public List<Entities.FavoriteInfo> MyConcernsProducts { get; set; }
        /// <summary>
        /// 我的积分
        /// </summary>
        public int MyMemberIntegral { get; set; }
        /// <summary>
        /// 我的优惠券
        /// </summary>
        public List<UserCouponInfo> MyCoupons { get; set; }
        /// <summary>
        /// 我的随机红包
        /// </summary>
        public List<ShopBonus> MyShopBonus { get; set; }
        /// <summary>
        /// 优惠券总数
        /// </summary>
        public int MyCouponCount { get; set; }
        /// <summary>
        /// 浏览商品记录
        /// </summary>
        public List<ProductBrowsedHistoryModel> MyBrowsingProducts { get; set; }
    }
}