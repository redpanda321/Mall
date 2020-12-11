using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mall.Core;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Mall.API
{
    /// <summary>
    /// APP端授权
    /// </summary>
    public class WebApiAuthAttribute : IAuthorizationFilter
    {
        public virtual void OnAuthorization(AuthorizationFilterContext actionContext)
        {
            var siteSettingsInfo = Application.SiteSettingApplication.SiteSettings;
            if (siteSettingsInfo != null && !siteSettingsInfo.IsOpenApp)
            {

                var actionDescriptor = actionContext.ActionDescriptor as ControllerActionDescriptor;
                string reurl = (actionDescriptor?.ControllerName + "/" + actionDescriptor?.ActionName).ToLower();
                bool ischeck = true;
                if (reurl == "productcontroller/getproductdetail" && siteSettingsInfo.IsOpenShopApp)
                {
                    ischeck = false;//如当前是产品详细接口，且开启了门店则表示验证app通过（详细页接口可以访问）
                }

                if (ischeck)
                {
                    HttpResponseMessage result = new HttpResponseMessage();
                    string jsonstr = "{\"IsOpenApp\":\"{0}\"}";
                    jsonstr = jsonstr.Replace("\"{0}\"", "false");
                    result.Content = new StringContent(jsonstr, Encoding.GetEncoding("UTF-8"), "application/json");
                    result.Content.CopyToAsync(actionContext.HttpContext.Response.Body).Wait();
                }
            }
        }
    }
}
