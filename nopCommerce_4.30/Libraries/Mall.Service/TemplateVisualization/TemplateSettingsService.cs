using Mall.CommonModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System.Linq;

namespace Mall.Service
{
    public class TemplateSettingsService : ServiceBase, ITemplateSettingsService
    {
        public void SetCurrentTemplate(string tName, long shopId = 0)
        {
            TemplateVisualizationSettingInfo curr = DbFactory.Default.Get<TemplateVisualizationSettingInfo>().Where(t => t.ShopId.Equals(shopId)).FirstOrDefault();
            if (null == curr)
            {
                curr = new TemplateVisualizationSettingInfo()
                {
                    CurrentTemplateName = tName,
                    ShopId = shopId
                };
                DbFactory.Default.Add(curr);
            }
            else
            {
                curr.CurrentTemplateName = tName;
                DbFactory.Default.Update(curr);
            }
        }

        public TemplateVisualizationSettingInfo GetCurrentTemplate(long shopId = 0)
        {
            var curr = DbFactory.Default.Get<TemplateVisualizationSettingInfo>().Where(t => t.ShopId.Equals(shopId)).FirstOrDefault();
            return curr;
        }

        public string GetGoodTagFromCache(long page, string tName = "")
        {
            string crrentTemplateName = "t1";
            string html = string.Empty;
            if (string.IsNullOrWhiteSpace(tName))
            {//从缓存获取当前模板数据
                var curr = new TemplateVisualizationSettingInfo();
                if (Core.Cache.Exists(CacheKeyCollection.MobileHomeTemplate("0")))//如果存在缓存，则从缓存中读取
                {
                    curr = Core.Cache.Get<TemplateVisualizationSettingInfo>(CacheKeyCollection.MobileHomeTemplate("0"));
                }
                else
                {
                    curr = DbFactory.Default.Get<TemplateVisualizationSettingInfo>().Where(t => t.ShopId.Equals(0)).FirstOrDefault();
                    Core.Cache.Insert<TemplateVisualizationSettingInfo>(CacheKeyCollection.MobileHomeTemplate("0"), curr);
                }
                if (curr != null)
                    crrentTemplateName = curr.CurrentTemplateName;
            }
            else
            {
                crrentTemplateName = tName;
            }
            if (Core.Cache.Exists(CacheKeyCollection.MobileHomeProductInfo(crrentTemplateName, page)))//如果存在缓存，则从缓存中读取
            {
                html = Core.Cache.Get<string>(CacheKeyCollection.MobileHomeProductInfo(crrentTemplateName, page));
            }
            return html;
        }

        public string GetShopGoodTagFromCache(long shopId, long page, string tName = "")
        {
            string crrentTemplateName = "t1";
            string html = string.Empty;
            var curr = new TemplateVisualizationSettingInfo();
            if (Core.Cache.Exists(CacheKeyCollection.MobileHomeTemplate(shopId.ToString())))//如果存在缓存，则从缓存中读取
            {
                curr = Core.Cache.Get<TemplateVisualizationSettingInfo>(CacheKeyCollection.MobileHomeTemplate(shopId.ToString()));
            }
            else
            {
                curr = DbFactory.Default.Get<TemplateVisualizationSettingInfo>().Where(t => t.ShopId.Equals(shopId)).FirstOrDefault();
                Core.Cache.Insert<TemplateVisualizationSettingInfo>(CacheKeyCollection.MobileHomeTemplate(shopId.ToString()), curr);
            }
            if (curr != null)
            {
                crrentTemplateName = curr.CurrentTemplateName;
            }
            if (!string.IsNullOrWhiteSpace(tName))
            {
                crrentTemplateName = tName;
            }

            if (Core.Cache.Exists(CacheKeyCollection.MobileShopHomeProductInfo(shopId.ToString(), crrentTemplateName, page)))//如果存在缓存，则从缓存中读取
            {
                html = Core.Cache.Get<string>(CacheKeyCollection.MobileShopHomeProductInfo(shopId.ToString(), crrentTemplateName, page));
            }
            return html;
        }

    }
}
