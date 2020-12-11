using Mall.Application;
using Mall.CommonModel;
using Mall.Core.Helper;
using System.Configuration;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace Mall.Web.Framework
{
    /// <summary>
    /// 主平台基础控制器类
    /// </summary>

    [Area("Admin")]
    public abstract class BaseAdminController : BaseController
    {
        /// <summary>
        /// 当前管理员
        /// </summary>
        ///  
        /// 
        IPaltManager platManager = null;

        public IPaltManager CurrentManager
        {
            get
            {
                if (platManager != null)
                {
                    return platManager;
                }

                var userId = UserCookieEncryptHelper.Decrypt(WebHelper.GetCookie(CookieKeysCollection.PLATFORM_MANAGER), CookieKeysCollection.USERROLE_ADMIN, true);
                if (userId > 0)
                    platManager = ManagerApplication.GetPlatformManager(userId);
                if (null == platManager)
                {
                    WebHelper.DeleteCookie(CookieKeysCollection.PLATFORM_MANAGER);
                    RedirectToAction("","Login", new { area = "admin" });
                    return null;
                }
                return platManager;
            }
        }

       

        public override void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            //TODO:DZY[150731] 与父级方法冲突，改为数据直接补充
            InitVisitorTerminal();

            //    var t = ConfigurationManager.AppSettings["IsInstalled"];
            //   if (!(null == t || bool.Parse(t)))

            bool t = configuration.GetValue<bool>("Mall:IsInstalled");

            if (!t)

            {
                return;
            }
            //不能应用在子方法上
           //   if (filterContext.IsChildAction)
           //     return;
            
            if (CurrentManager == null)
            {
                if (Core.Helper.WebHelper.IsAjax())
                {
                    Result result = new Result();
                    result.msg = "登录超时,请重新登录！";
                    result.success = false;
                    filterContext.Result = Json(result);
                    return;
                }
                else
                {
                    var result = RedirectToAction("", "Login", new { area = "admin" });
                    filterContext.Result = result;
                    return;
                    //跳转到登录页
                }

            }

            var ad = filterContext.ActionDescriptor as ControllerActionDescriptor;

            object[] actionFilter = ad.MethodInfo.GetCustomAttributes(typeof(UnAuthorize), false);
            if (actionFilter.Length == 1)
                return;
            var controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();
            var actionName = filterContext.RouteData.Values["action"].ToString().ToLower();
            if (CurrentManager.AdminPrivileges == null || CurrentManager.AdminPrivileges.Count == 0 || !AdminPermission.CheckPermissions(CurrentManager.AdminPrivileges, controllerName, actionName))
            {
                if (Core.Helper.WebHelper.IsAjax())
                {
                    Result result = new Result();
                    result.msg = "你没有访问的权限！";
                    result.success = false;
                    filterContext.Result = Json(result);
                    return;
                }
                else
                {
                    //跳转到错误页
                    var result = new ViewResult() { ViewName = "NoAccess" };
                    result.TempData.Add("Message", "你没有权限访问此页面");
                    result.TempData.Add("Title", "你没有权限访问此页面！");
                    filterContext.Result = result;
                    return;
                }
            }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            /*
            //System.Threading.Thread.Sleep(2000);
            //不能应用在子方法上
         //   if (filterContext.IsChildAction)
         //       return;

            // AdminPermission.GetAllActionByAssembly();
            //  var desc=  ((DescriptionAttribute)(filterContext.ActionDescriptor.GetCustomAttributes(typeof(DescriptionAttribute), true)[0])).Description;
            var controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();
            var actionName = filterContext.RouteData.Values["action"].ToString().ToLower();
            // var areaName=RouteData.DataTokens["area"].ToString().ToLower(); 

            */



            //TODO:DZY[150731] 与父级方法冲突，改为数据直接补充
            InitVisitorTerminal();

            //   bool t = DataSettingsManager.DatabaseIsInstalled;

            bool t = configuration.GetValue<bool>("Mall:IsInstalled");
            if (!t)
            {
                return;
            }

            //不能应用在子方法上
            // if (filterContext.IsChildAction)
            //     return;

            if (CurrentManager == null)
            {
                if (Core.Helper.WebHelper.IsAjax())
                {
                    Result result = new Result();
                    result.msg = "登录超时,请重新登录！";
                    result.success = false;
                    filterContext.Result = Json(result);
                    return;
                }
                else
                {
                    var result = RedirectToAction("", "Login", new { area = "admin" });
                    filterContext.Result = result;
                    return;
                    //跳转到登录页
                }

            }
            var ad = filterContext.ActionDescriptor as ControllerActionDescriptor;

            object[] actionFilter = ad.MethodInfo.GetCustomAttributes(typeof(UnAuthorize), false);
            if (actionFilter.Length == 1)
                return;
            var controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();
            var actionName = filterContext.RouteData.Values["action"].ToString().ToLower();
            if (CurrentManager.AdminPrivileges == null || CurrentManager.AdminPrivileges.Count == 0 || !AdminPermission.CheckPermissions(CurrentManager.AdminPrivileges, controllerName, actionName))
            {
                if (Core.Helper.WebHelper.IsAjax())
                {
                    Result result = new Result();
                    result.msg = "你没有访问的权限！";
                    result.success = false;
                    filterContext.Result = Json(result);
                    return;
                }
                else
                {
                    //跳转到错误页
                    var result = new ViewResult() { ViewName = "NoAccess" };
                    TempData["Message"] = "你没有权限访问此页面";
                    TempData["Title"] = "你没有权限访问此页面！";
                    filterContext.Result = result;
                    return;
                }
            }


            //Before OnActionExecuting


            base.OnActionExecuting(filterContext);


            //System.Threading.Thread.Sleep(2000);


            // AdminPermission.GetAllActionByAssembly();
            //  var desc=  ((DescriptionAttribute)(filterContext.ActionDescriptor.GetCustomAttributes(typeof(DescriptionAttribute), true)[0])).Description;
            ///  var controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();
            ///  var actionName = filterContext.RouteData.Values["action"].ToString().ToLower();
            // var areaName=RouteData.DataTokens["area"].ToString().ToLower(); 

        }

        protected ActionResult SuccessfulRedirectView(string viewName, object routeData = null)
        {
            return RedirectToAction(viewName, routeData);
        }
    }
}
