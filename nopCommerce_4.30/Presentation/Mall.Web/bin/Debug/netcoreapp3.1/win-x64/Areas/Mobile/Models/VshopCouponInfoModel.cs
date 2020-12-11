using Mall.Entities;

namespace Mall.Web.Areas.Mobile.Models
{
    public class VshopCouponInfoModel
    {
        public long CouponId { get; set; }
        public long CouponRecordId { get; set; }
        public long ShopId { get; set; }
        public long VShopid { get; set; }
        public bool IsFavoriteShop { get; set; }
        public CouponInfo.CouponReceiveStatus CouponStatus { get; set; }
        public bool IsShowSyncWeiXin { get; set; }
        public string WeiXinReceiveUrl { get; set; }
        public string FollowUrl { get; set; }
        public Entities.CouponInfo CouponData { get; set; }
        public int? AcceptId { get; set; }

        #region 微信JSSDK参数
        public Entities.WXSyncJSInfoByCard WXJSInfo { get; set; }
        public Entities.WXJSCardModel WXJSCardInfo { get; set; }
        #endregion
    }
}