using Mall.Core;
using Mall.IServices;
using Mall.Entities;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    public class ShopOpenApiApplication
    {
        static IShopOpenApiService _iShopOpenApiService =  EngineContext.Current.Resolve<IShopOpenApiService>();

        /// <summary>
        /// 获取店铺的OpenApi配置
        /// </summary>
        /// <param name="appkey"></param>
        /// <returns></returns>
        public static ShopOpenApiSettingInfo Get(string appkey)
        {
            return _iShopOpenApiService.Get(appkey);
        }
    }
}
