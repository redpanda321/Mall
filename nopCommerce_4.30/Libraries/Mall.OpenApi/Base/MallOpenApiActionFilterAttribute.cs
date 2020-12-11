using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc.Filters;
using System;


namespace Mall.OpenApi
{
    /// <summary>
    /// Action过滤器-过滤基础参数与签名
    /// </summary>
    public class MallOpenApiActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (String.IsNullOrEmpty(OpenAPIHelper.HostUrl))
            {
                //装载当前URI
                OpenAPIHelper.HostUrl = actionContext.HttpContext.Request.Scheme + "://" + actionContext.HttpContext.Request.Host.ToString();
            }

            var data = OpenAPIHelper.GetSortedParams(actionContext.HttpContext.Request);
            OpenAPIHelper.CheckBaseParamsAndSign(data);
            base.OnActionExecuting(actionContext);
        }
    }
}
