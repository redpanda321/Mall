using Mall.Core;
using Mall.IServices;
using Mall.Entities;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    public class CityExpressConfigApplication
    {
      //  private static ICityExpressConfigServiceService _iCityExpressConfigServiceService =  EngineContext.Current.Resolve<ICityExpressConfigServiceService>();


        private static ICityExpressConfigServiceService _iCityExpressConfigServiceService =  EngineContext.Current.Resolve<ICityExpressConfigServiceService>();


        /// <summary>
        /// 获得达达配置信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static CityExpressConfigInfo GetDaDaCityExpressConfig(long shopId)
        {
            return _iCityExpressConfigServiceService.GetDaDaCityExpressConfig(shopId);
        }
        /// <summary>
        /// 修改达达配置信息
        /// </summary>
        /// <param name="data"></param>
        public static void Update(long shopId, CityExpressConfigInfo data)
        {
            _iCityExpressConfigServiceService.Update(shopId, data);
        }
        /// <summary>
        /// 设置达达物流开启关闭状态
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static void SetStatus(long shopId, bool status)
        {
            _iCityExpressConfigServiceService.SetStatus(shopId, status);
        }
        /// <summary>
        /// 获取达达物流开启状态
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static bool GetStatus(long shopId)
        {
            return _iCityExpressConfigServiceService.GetStatus(shopId);
        }
    }
}
