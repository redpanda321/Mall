using Mall.Application;
using Mall.CommonModel;
using Mall.Entities;
using Mall.IServices;

namespace Mall.Web.App_Code.Common
{
    /// <summary>
    /// 加载缓存处理类
    /// </summary>
    public class InitCacheHelper
    {
        /// <summary>
        /// 初始化缓存
        /// </summary>
        public static void InitCache()
        {
            //加载移动端当前首页模版
            var curr = ServiceApplication.Create<ITemplateSettingsService>().GetCurrentTemplate(0);
            Core.Cache.Insert<TemplateVisualizationSettingInfo>(CacheKeyCollection.MobileHomeTemplate("0"), curr);
        }
    }
}