using Mall.Application;
using Mall.CommonModel;
using Mall.Core.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mall.Web.Framework
{
    [Area("Web")]
    public abstract class BaseWebController : BaseController
    {
        ISellerManager sellerManager = null;

        public long UserId
        {
            get
            {
                if (CurrentUser != null)
                    return CurrentUser.Id;
                return 0;
            }
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (  filterContext.HttpContext.Request.Headers["x-requested-with"].ToString() != "XMLHttpRequest")
            {
                //统计代码
                StatisticApplication.StatisticPlatVisitUserCount();
            }
            base.OnActionExecuting(filterContext);
        }
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
           // if (filterContext.IsChildAction)
           // {
            //    return;
            //}
            base.OnActionExecuted(filterContext);
        }

        /// <summary>
        /// 当前管理员
        /// </summary>
        public ISellerManager CurrentSellerManager
        {
            get
            {
                if (sellerManager != null)
                {
                    return sellerManager;
                }
                else
                {
                    long userId = UserCookieEncryptHelper.Decrypt(WebHelper.GetCookie(CookieKeysCollection.SELLER_MANAGER), CookieKeysCollection.USERROLE_SELLERADMIN);
                    if (userId != 0)
                    {
                        sellerManager = ManagerApplication.GetSellerManager(userId);
                    }
                }
                return sellerManager;
            }
        }
    }
}
