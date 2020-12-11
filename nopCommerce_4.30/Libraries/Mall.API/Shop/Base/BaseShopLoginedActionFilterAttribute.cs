using Mall.Core.Helper;
using Mall.Web.Framework;
using System;

using Mall.Application;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mall.API
{
    /// <summary>
    /// Action过滤器-过滤冻结
    /// </summary>
    public class BaseShopLoginedActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            string userkey = "";
            userkey = WebHelper.GetQueryString("userkey");
            if (string.IsNullOrWhiteSpace(userkey))
            {
                userkey = WebHelper.GetFormString("userkey");
            }
            long shopuid= UserCookieEncryptHelper.Decrypt(userkey, CookieKeysCollection.USERROLE_SELLERADMIN);
            var shopm = ManagerApplication.GetSellerManager(shopuid);
            if (shopm == null)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "商家信息错误");
            }
            var shop = ShopApplication.GetShop(shopm.ShopId);
            if (shop == null)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "商家信息错误");
            }
            if (shop.ShopStatus== Entities.ShopInfo.ShopAuditStatus.Freeze)
            {
                throw new MallApiException(ApiErrorCode.User_Freeze, "商家已冻结");
            }
            base.OnActionExecuting(actionContext);
        }
    }
}
