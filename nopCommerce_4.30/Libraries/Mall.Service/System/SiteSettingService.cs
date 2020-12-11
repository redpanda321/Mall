using Mall.CommonModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Linq;
using System.Reflection;

using System.Collections.Generic;
namespace Mall.Service
{
    public class SiteSettingService : ServiceBase, ISiteSettingService
    {
        public List<SiteSettingInfo> GetSiteSettings()
        {
            return DbFactory.Default.Get<SiteSettingInfo>().ToList();
        }

        public void SaveSettings(Dictionary<string, string> settings)
        {
            var keys = settings.Keys.ToList();
            var models = DbFactory.Default.Get<SiteSettingInfo>().Where(p => p.Key.ExIn(keys)).ToList();
            DbFactory.Default.InTransaction(() =>
            {
                foreach (var item in settings)
                {
                    var model = models.FirstOrDefault(p => p.Key == item.Key);
                    if (model != null)
                    {
                        model.Value = item.Value;
                        DbFactory.Default.Update(model);
                    }
                    else
                    {
                        DbFactory.Default.Add(new SiteSettingInfo
                        {
                            Key = item.Key,
                            Value = item.Value
                        });
                    }
                }
            });
        }
    }
}
