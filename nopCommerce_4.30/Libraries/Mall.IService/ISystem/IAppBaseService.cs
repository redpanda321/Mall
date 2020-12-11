using Mall.Entities;

namespace Mall.IServices
{
    /// <summary>
    /// APP基础信息服务
    /// </summary>
    public interface IAppBaseService : IService
    {
        /// <summary>
        /// 使用appkey获取app基础信息
        /// </summary>
        /// <param name="appkey"></param>
        /// <returns></returns>
        AppBaseSafeSettingInfo GetSetting(string appkey);
        /// <summary>
        /// 通过appkey获取AppSecreate
        /// </summary>
        /// <param name="appkey"></param>
        /// <returns></returns>
        string GetAppSecret(string appkey);
        /// <summary>
        /// 取第一个app基础信息
        /// </summary>
        /// <returns></returns>
        AppBaseSafeSettingInfo GetFirstSetting();
    }
}