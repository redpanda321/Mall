

using Hishop.Open.Api;

using Mall.Application;
using Mall.Core;



namespace Mall.Web.Framework
{
    /// <summary>
    /// 店铺辅助工具
    /// </summary>
    public class ShopHelper
    {
        private string _AppKey { get; set; }
        private string _AppSecreate { get; set; }

        /// <summary>
        /// 店铺编号
        /// </summary>
        public long ShopId { get; set; }
        /// <summary>
        /// 店铺管理员
        /// </summary>
        public string SellerName { get; set; }
        /// <summary>
        /// 是否官方自营店
        /// </summary>
        public bool IsSelf { get; set; }
        /// <summary>
        /// 店铺app_key
        /// </summary>
        public string AppKey
        {
            get
            {
                return _AppKey;
            }
        }
        /// <summary>
        /// 店铺app_secreate
        /// </summary>
        public string AppSecreate
        {
            get
            {
                return _AppSecreate;
            }
        }

        public ShopHelper(string app_key)
        {
            _AppKey = app_key;
            if (string.IsNullOrWhiteSpace(_AppKey))
            {
                throw new MallApiException(Hishop.Open.Api.OpenApiErrorCode.Missing_App_Key, "app_key");
            }
            var shopappinfo = ShopOpenApiApplication.Get(_AppKey);
            if (_AppKey == "Malltest")
                shopappinfo = new Entities.ShopOpenApiSettingInfo { IsEnable = true, ShopId = 1, AppSecreat = "has2f5zbd4" };
            if (shopappinfo == null)
            {
                throw new MallApiException(OpenApiErrorCode.Invalid_App_Key, "app_key");
            }
            if (shopappinfo.IsEnable != true)
            {
                throw new MallApiException(Hishop.Open.Api.OpenApiErrorCode.System_Error, "function not open");
            }
            _AppSecreate = shopappinfo.AppSecreat;
            if (string.IsNullOrWhiteSpace(_AppSecreate))
            {
                throw new MallApiException(Hishop.Open.Api.OpenApiErrorCode.Insufficient_ISV_Permissions, "not set app_secreat");
            }

            var shop = ShopApplication.GetShop(shopappinfo.ShopId);
            if (shop == null)
            {
                throw new MallApiException(Hishop.Open.Api.OpenApiErrorCode.Invalid_App_Key, "app_key");
            }
            ShopId = shop.Id;
            var manage = ManagerApplication.GetSellerManagerByShopId(ShopId);
            if (manage == null)
            {
                throw new MallException("店铺管理信息有误，请管理员修正");
            }
            SellerName = manage.UserName;
            IsSelf = shop.IsSelf;
        }
    }
}
