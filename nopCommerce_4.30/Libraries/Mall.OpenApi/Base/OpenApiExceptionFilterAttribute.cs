
using System.Net.Http;
using System.Text;
using Hishop.Open.Api;
using Mall.Core;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mall.OpenApi
{
    /// <summary>
    /// OpenApi异常处理
    /// </summary>
    public class OpenApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            string jsonstr = OpenApiErrorMessage.ShowErrorMsg(OpenApiErrorCode.System_Error, "");
            if (context.Exception is MallApiException)
            {
                var curexp = context.Exception as MallApiException;
                jsonstr = OpenApiErrorMessage.ShowErrorMsg(curexp.ErrorCode, curexp.Message);
            }
            else
            {
                Log.Error(context.Exception.Message, context.Exception);
            }

            result.Content = new StringContent(jsonstr, Encoding.GetEncoding("UTF-8"), "application/json");

  
            result.Content.CopyToAsync(context.HttpContext.Response.Body).Wait();


        }
    }
}