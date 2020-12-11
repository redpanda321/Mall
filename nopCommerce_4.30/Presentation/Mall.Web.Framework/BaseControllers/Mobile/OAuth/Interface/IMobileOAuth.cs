
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mall.Web.Framework
{
    /// <summary>
    /// 移动端信任登录
    /// </summary>
    interface IMobileOAuth
    {
        /// <summary>
        /// 获取移动端信任登录OpenId
        /// </summary>
        /// <returns></returns>
        MobileOAuthUserInfo GetUserInfo(ActionExecutingContext filterContext, out string redirectUrl);

        MobileOAuthUserInfo GetUserInfo(ActionExecutingContext filterContext, out string redirectUrl, Entities.WXshopInfo settings);

        MobileOAuthUserInfo GetUserInfo_bequiet(ActionExecutingContext filterContext, out string redirectUrl, Entities.WXshopInfo settings);
    }
}
