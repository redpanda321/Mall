
namespace Mall.Web.Framework
{
    /// <summary>
    /// Cookie集合
    /// </summary>
    public class CookieKeysCollection
    {
        /// <summary>
        /// 平台管理员登录标识
        /// </summary>
        public const string PLATFORM_MANAGER = "Mall-PlatformManager";

        /// <summary>
        /// 商家管理员登录标识
        /// </summary>
        public const string SELLER_MANAGER = "Mall-SellerManager";

        /// <summary>
        /// 会员登录标识
        /// </summary>
        public const string Mall_USER = "Mall-User";
        /// <summary>
        /// 会员登录标识
        /// </summary>
        public const string Mall_ACTIVELOGOUT = "d783ea20966909ff";  //Mall_ACTIVELOGOUT做MD5后的16位字符
        /// <summary>
        /// 分销销售员编号
        /// </summary>
        public const string Mall_DISTRIBUTION_SPREAD_ID_COOKIE_NAME = "Mall-dspreadid";
        /// <summary>
        /// 需要清理分销销售员编号
        /// </summary>
        public const string Mall_NEED_CLEAR_DISTRIBUTION_SPREAD_ID_COOKIE_NAME = "Mall-needclearspreadid";
        /// <summary>
        /// 不同平台用户key
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static string Mall_USER_KEY(string platform)
        {
            return string.Format("Mall-{0}User", platform);
        }
        /// <summary>
        /// 
        /// </summary>
        public const string Mall_USER_OpenID = "Mall-User_OpenId";
        /// <summary>
        /// 购物车
        /// </summary>
        public const string Mall_CART = "Mall-CART";
        /// <summary>
        /// 门店购物车
        /// </summary>
        public const string Mall_CART_BRANCH = "Mall-CART-BRANCH";

        /// <summary>
        /// 商品浏览记录
        /// </summary>
        public const string Mall_PRODUCT_BROWSING_HISTORY = "Mall_ProductBrowsingHistory";
        
        /// <summary>
        /// 最后产生访问时间
        /// </summary>
        public const string Mall_LASTOPERATETIME = "Mall_LastOpTime";

        /// <summary>
        /// 标识是平台还是商家公众号
        /// </summary>
        public const string MobileAppType = "Mall-Mobile-AppType";
        /// <summary>
        /// 访问的微店标识
        /// </summary>
        public const string Mall_VSHOPID = "Mall-VShopId";

		/// <summary>
		/// 用户角色(Admin)
		/// </summary>
		public const string USERROLE_ADMIN = "0";
		/// <summary>
		/// 用户角色(SellerAdmin)
		/// </summary>
		public const string USERROLE_SELLERADMIN = "1";
		/// <summary>
		/// 用户角色(User)
		/// </summary>
		public const string USERROLE_USER = "2";
    }
}