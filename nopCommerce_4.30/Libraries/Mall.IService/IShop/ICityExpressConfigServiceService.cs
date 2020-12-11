using Mall.Entities;

namespace Mall.IServices
{
    public interface ICityExpressConfigServiceService : IService
    {
        /// <summary>
        /// 获得达达配置信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        CityExpressConfigInfo GetDaDaCityExpressConfig(long shopId);
        /// <summary>
        /// 修改达达配置信息
        /// </summary>
        /// <param name="data"></param>
        void Update(long shopId, CityExpressConfigInfo data);
        /// <summary>
        /// 设置达达物流开启关闭状态
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        void SetStatus(long shopId, bool status);
        /// <summary>
        /// 获取达达物流开启状态
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        bool GetStatus(long shopId);
    }
}
