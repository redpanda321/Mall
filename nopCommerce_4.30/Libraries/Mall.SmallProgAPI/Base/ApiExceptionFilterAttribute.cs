using Mall.Core;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text;


namespace Mall.SmallProgAPI
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public  void OnException(ActionExecutedContext context)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            string jsonstr = ShowErrorMsg(ApiErrorCode.System_Error, context.Exception.Message);
            if (context.Exception is MallApiException)
            {
                var curexp = context.Exception as MallApiException;
                jsonstr = ShowErrorMsg(curexp.ErrorCode, curexp.Message);
            }
            else
            {
                Log.Error(context.Exception.Message, context.Exception);
            }

            result.Content = new StringContent(jsonstr, Encoding.GetEncoding("UTF-8"), "application/json");

            result.Content.CopyToAsync(context.HttpContext.Response.Body).Wait();
        }
        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="enumSubitem"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        private string ShowErrorMsg(Enum enumSubitem, string fields)
        {
            string str = GetEnumDescription(enumSubitem).Replace("_", " ");
            string format = "{{\"success\":false,\"code\":{0},\"msg\":\"{1}\",\"sub_msg\":\"{2}\",\"data\":[]}}";
            return string.Format(format, enumSubitem.GetHashCode(), fields, str);
        }
        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <param name="enumSubitem"></param>
        /// <returns></returns>
        private string GetEnumDescription(Enum enumSubitem)
        {
            string name = enumSubitem.ToString();
            object[] customAttributes = enumSubitem.GetType().GetField(name).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if ((customAttributes == null) || (customAttributes.Length == 0))
            {
                return name;
            }
            DescriptionAttribute attribute = (DescriptionAttribute)customAttributes[0];
            return attribute.Description;
        }


    }
}
