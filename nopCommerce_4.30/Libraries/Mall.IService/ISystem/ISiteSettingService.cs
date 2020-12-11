using Mall.Entities;
using System.Collections.Generic;

namespace Mall.IServices
{
    /// <summary>
    /// 站点设置服务
    /// </summary>
    public interface ISiteSettingService : IService
    {
        List<SiteSettingInfo> GetSiteSettings();

        void SaveSettings(Dictionary<string, string> settings);
    }
}
