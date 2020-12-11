using Mall.Core;
using Mall.IServices;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    public class AppBaseApplication
    {
    
        static IAppBaseService _iAppBaseService =  EngineContext.Current.Resolve<IAppBaseService>();


        /// <summary>
        /// 通过appkey获取AppSecret
        /// </summary>
        /// <param name="appkey"></param>
        /// <returns></returns>
        public static string GetAppSecret(string appkey)
        {
            return _iAppBaseService.GetAppSecret(appkey);
        }
    }
}
