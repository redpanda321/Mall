using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Http;
using System.Text;



namespace Mall.Web.Framework
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            if (context.Exception is Mall.Core.MallException)
            {
                result.Content = new StringContent("success = false, code = 101, msg =" + context.Exception.Message, Encoding.GetEncoding("UTF-8"), "application/json");
            }
            else
            {
                result.Content = new StringContent("success = false, code = 102, msg =" + context.Exception.Message, Encoding.GetEncoding("UTF-8"), "application/json");
            }
            //context.Response.StatusCode = System.Net.HttpStatusCode.Accepted;
            //context.Response = result;

        }
    }
}


