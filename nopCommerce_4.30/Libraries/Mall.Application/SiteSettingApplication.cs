using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.IServices;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Application
{
    //TODO:FG 转移Servic对Application的引用
    public class SiteSettingApplication:BaseApplicaion<ISiteSettingService>
    {

        private static ISiteSettingService _iSiteSettingService = EngineContext.Current.Resolve<ISiteSettingService>();


        public static SiteSettings SiteSettings
        {
            get
            {
                var settings = (SiteSettings)CallContext.GetData(CacheKeyCollection.SiteSettings);
                if (settings == null)
                {
                    settings = Cache.Get<SiteSettings>(CacheKeyCollection.SiteSettings);//缓存中获取
                    if (settings == null)
                    {
                        settings = InitSettings();//数据库中加载
                        Cache.Insert(CacheKeyCollection.SiteSettings, settings);
                    }
                    CallContext.SetData(CacheKeyCollection.SiteSettings, settings);
                }
                return settings;
            }
        }

        private static SiteSettings InitSettings()
        {
            var settings = new SiteSettings();
            var properties = typeof(SiteSettings).GetProperties();

            var data = Service.GetSiteSettings();
            foreach (var property in properties)
            {
                var temp = data.FirstOrDefault(item => item.Key == property.Name);
                if (temp != null)
                    property.SetValue(settings, Convert.ChangeType(temp.Value, property.PropertyType));
            }
            return settings;
        }

        /// <summary>
        /// 保存对配置的修改
        /// </summary>
        public static void SaveChanges()
        {
            var current = SiteSettings;
            var data = Service.GetSiteSettings();

            var changes = new Dictionary<string, string>();
            var properties = typeof(SiteSettings).GetProperties();

            foreach (var property in properties)
            {
                var key = property.Name;
                var oldValue = data.FirstOrDefault(p => p.Key == key)?.Value ?? string.Empty;
                var newValue = property.GetValue(current)?.ToString() ?? string.Empty;

                if (oldValue != newValue) changes.Add(key, newValue);
            }

            if (changes.Count > 0)
            {
                Service.SaveSettings(changes);
                Cache.Remove(CacheKeyCollection.SiteSettings);//清空配置缓存
            }
        }

    
    }
}
