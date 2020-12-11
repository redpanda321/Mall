using System;
using System.Net.Http;
using System.Text;

using Mall.Core;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mall.API
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            string jsonstr = ShowErrorMsg(ApiErrorCode.System_Error, "");
            if (context.Exception is MallApiException)
            {
                var curexp = context.Exception as MallApiException;
                jsonstr = ShowErrorMsg(curexp.ErrorCode, "", curexp.Message);
            }
            else
            {
                jsonstr = ShowErrorMsg(ApiErrorCode.System_Error, "no params", context.Exception.Message);
                Log.Error(context.Exception.Message, context.Exception);
            }

            result.Content = new StringContent(jsonstr, Encoding.GetEncoding("UTF-8"), "application/json");
            // context.Response.StatusCode = System.Net.HttpStatusCode.Accepted;
            //context.Response = result;
            result.Content.CopyToAsync(context.HttpContext.Response.Body).Wait();
        }
        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="enumSubitem"></param>
        /// <param name="fields"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private string ShowErrorMsg(Enum enumSubitem, string fields, string errorMessage = "")
        {
            string str = enumSubitem.ToDescription().Replace("_", " ");
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                str = errorMessage;
            }
            string format = "{{\"success\":false,\"msg\":\"{0}\",\"code\":{1}}}";
            return string.Format(format, str, enumSubitem.GetHashCode());
        }
    }
}
