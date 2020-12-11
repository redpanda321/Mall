using Mall.Application;
using Mall.Core.Helper;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc.Filters;
using System;


namespace Mall.API
{
    /// <summary>
    /// Action过滤器-过滤冻结
    /// </summary>
    public class BaseShopBranchLoginedActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            string userkey = "";
            userkey = WebHelper.GetQueryString("userkey");
            if (string.IsNullOrWhiteSpace(userkey))
            {
                userkey = WebHelper.GetFormString("userkey");
            }
            #region 特殊控制器方法不验证(因为app已打包发布了，则没改app代码，在接口这里判断处理下这个控制器)
            var route = actionContext.RouteData;
            if (route != null)
            {
                var controllerName = (route.Values["controller"] == null ? "" : route.Values["controller"].ToString().ToLower());
                var actionName = (route.Values["action"] == null ? "" : route.Values["action"].ToString().ToLower());
                if (controllerName == "shopbranchorder" && actionName == "getlogisticsdata")
                {
                    base.OnActionExecuting(actionContext);
                    return;
                }
            }
            #endregion

            long sbuid = UserCookieEncryptHelper.Decrypt(userkey, CookieKeysCollection.USERROLE_USER);
            var sbm = ShopBranchApplication.GetShopBranchManager(sbuid);
            if (sbm == null)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error,"门店信息错误");
            }
            var sb = ShopBranchApplication.GetShopBranchById(sbm.ShopBranchId);
            if (sb == null)
            {
                throw new MallApiException(ApiErrorCode.Parameter_Error, "门店信息错误");
            }
            if (sb.Status == CommonModel.ShopBranchStatus.Freeze)
            {
                throw new MallApiException(ApiErrorCode.User_Freeze, "门店已冻结，您无权进行此操作");
            }
            base.OnActionExecuting(actionContext);
        }
    }
}
