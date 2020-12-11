using Mall.CommonModel;
using Mall.Core;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Service
{
    public class ProductDescriptionTemplateService : ServiceBase, IProductDescriptionTemplateService
    {
        /// <summary>
        /// 模板所在目录
        /// </summary>
        const string TEMPLATE_DIRECTORY = "/Storage/Shop/{0}/templates/{1}";

        public QueryPageModel<ProductDescriptionTemplateInfo> GetTemplates(long shopId, int pageNumber, int pageSize, string name = null, ProductDescriptionTemplateInfo.TemplatePosition? position = null)
        {
            var templates = DbFactory.Default.Get<ProductDescriptionTemplateInfo>().Where(item => item.ShopId == shopId);
            if (position.HasValue)
                templates.Where(item => item.Position == position.Value);

            if (!string.IsNullOrWhiteSpace(name))
                templates.Where(item => item.Name.Contains(name));

            var rets = templates.OrderByDescending(item => item.Id).ToPagedList(pageNumber, pageSize);
            var pageMode = new QueryPageModel<ProductDescriptionTemplateInfo>()
            {
                Total = rets.TotalRecordCount,
                Models = rets
            };

            return pageMode;
        }

        public List<ProductDescriptionTemplateInfo> GetTemplates(long shopId)
        {
            return DbFactory.Default.Get<ProductDescriptionTemplateInfo>().Where(t => t.ShopId.Equals(shopId)).ToList();
        }

        public ProductDescriptionTemplateInfo GetTemplate(long id, long shopId)
        {
            return DbFactory.Default.Get<ProductDescriptionTemplateInfo>().Where(item => item.Id == id && item.ShopId == shopId).FirstOrDefault();
        }

        public void AddTemplate(ProductDescriptionTemplateInfo template)
        {
            CheckProperty(template);//检查属性合法性
            template.Content = HTMLProcess(template.Content, template.Id, template.ShopId);//获取外站图片及去除script脚本,防止注入
            template.MobileContent = HTMLProcess(template.MobileContent, template.Id, template.ShopId);
            DbFactory.Default.Add(template);
        }

        public void UpdateTemplate(ProductDescriptionTemplateInfo template)
        {
            CheckProperty(template);//检查属性合法性

            var oldTemplate = DbFactory.Default.Get<ProductDescriptionTemplateInfo>().Where(p => p.Id == template.Id).FirstOrDefault();
            oldTemplate.Name = template.Name;
            oldTemplate.Position = template.Position;
            oldTemplate.Content = HTMLProcess(template.Content, template.Id, template.ShopId);//获取外站图片及去除script脚本,防止注入
            oldTemplate.MobileContent = HTMLProcess(template.MobileContent, template.Id, template.ShopId);
            DbFactory.Default.Update(oldTemplate);
        }

        public void DeleteTemplate(long shopId, params long[] ids)
        {
            var templates = DbFactory.Default.Get<ProductDescriptionTemplateInfo>().Where(item => item.Id.ExIn(ids)).ToList();
            if (templates.Count(item => item.ShopId != shopId) > 0)
                throw new MallException("不能删除非本店铺的商品描述模板");

            var templateDirs = templates.Select(item => item.Id.ToString());

            templateDirs = templateDirs.Select(item => string.Format(TEMPLATE_DIRECTORY, shopId, item));

            //删除图片
            foreach (var dir in templateDirs)
            {
                if (Core.MallIO.ExistDir(dir))
                {
                    Core.MallIO.DeleteDir(dir, true);
                }
            }

            DbFactory.Default.Del<ProductDescriptionTemplateInfo>().Where(item => item.Id.ExIn(ids)).Succeed();
        }

        void CheckProperty(ProductDescriptionTemplateInfo template)
        {
            if (string.IsNullOrWhiteSpace(template.Content))
                throw new InvalidPropertyException("模板内容不可空");
            if (string.IsNullOrWhiteSpace(template.Name))
                throw new InvalidPropertyException("模板名称不可空");
            if (template.ShopId == 0)
                throw new InvalidPropertyException("店铺id不可空");
        }

        /// <summary>
        /// 转移外站图片，去除script脚本
        /// </summary>
        /// <param name="content">html内容</param>
        /// <param name="id"></param>
        /// <returns></returns>
        string HTMLProcess(string content, long id, long shopId)
        {
            string imageRealtivePath = string.Format(TEMPLATE_DIRECTORY, shopId, id);
            content = Core.Helper.HtmlContentHelper.TransferToLocalImage(content, "/", imageRealtivePath, Core.MallIO.GetImagePath(imageRealtivePath) + "/");
            content = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(content);
            return content;
        }
    }
}
