using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc.Filters;
using System;


namespace Mall.API
{
    /// <summary>
    /// Action过滤器-过滤基础参数与签名
    /// </summary>
    public class MallApiActionFilter: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (String.IsNullOrEmpty(ApiHelper.HostUrl))
            {
                //装载当前URI
                ApiHelper.HostUrl = actionContext.HttpContext.Request.Scheme + "://" + actionContext.HttpContext.Request.Host.ToString();
            }

            var data = ApiHelper.GetSortedParams(actionContext.HttpContext);
            ApiHelper.CheckBaseParamsAndSign(data);
            base.OnActionExecuting(actionContext);
        }
    }
}
