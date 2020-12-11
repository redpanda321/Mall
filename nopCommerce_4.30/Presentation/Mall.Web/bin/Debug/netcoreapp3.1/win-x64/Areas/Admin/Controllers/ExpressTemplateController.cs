using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.IServices;
using Mall.Web.Areas.Admin.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Admin.Controllers
{
    public class ExpressTemplateController : BaseAdminController
    {
        private IExpressService _iExpressService;

        public ExpressTemplateController(IExpressService iExpressService)
        {
            _iExpressService = iExpressService;
        }

        public ActionResult Management()
        {
            var result = ExpressApplication.GetAllExpress();
            return View(result);
        }
        [HttpPost]
        public JsonResult Express(ExpressCompany model)
        {
            if (model.Id > 0)
            {
                ExpressApplication.UpdateExpressCode(model);
            }
            else
            {
                ExpressApplication.AddExpress(model);
            }
            return Json(new Result { success = true });
        }

        public JsonResult DeleteExpress(long id)
        {
            ExpressApplication.DeleteExpress(id);
            return Json(new Result { success = true, msg = "删除成功" });
        }
        public JsonResult ClearData(long id)
        {
            ExpressApplication.ClearData(id);
            return Json(new Result { success = true, msg = "清除成功" });
        }
        public ActionResult Setting()
        {
            return View(SiteSettingApplication.SiteSettings);
        }
        public JsonResult SaveExpressSetting(string Kuaidi100Key, int KuaidiType, string KuaidiApp_key, string KuaidiAppSecret)
        {
            var setting = SiteSettingApplication.SiteSettings;
            setting.Kuaidi100Key = Kuaidi100Key;
            setting.KuaidiType = KuaidiType;
            setting.KuaidiApp_key = KuaidiApp_key;
            setting.KuaidiAppSecret = KuaidiAppSecret;
            SiteSettingApplication.SaveChanges();
            return Json(new Result() { success = true, msg = "保存成功" });
        }

        public ActionResult Edit(string name)
        {
            var template = ExpressApplication.GetExpress(name);
            return View(template);
        }

        public JsonResult ChangeStatus(long id, ExpressStatus status)
        {
            ExpressApplication.ChangeExpressStatus(id, status);
            return Json(new Result { success = true, msg = "操作成功" });
        }
        [HttpPost]
        [UnAuthorize]
        public JsonResult GetConfig(string name)
        {
            var template = ExpressApplication.GetExpress(name);
            var elementTypes = Enum.GetValues(typeof(ExpressElementType));
            var allElements = new List<Element>();
            foreach(var item in elementTypes)
            {
                Element el = new Element() {
                     key=((int)item).ToString(),
                     value=((ExpressElementType)item).ToDescription()
                };
                allElements.Add(el);
            }
            ExpressTemplateConfig config = new ExpressTemplateConfig()
            {
                width = template.Width,
                height = template.Height,
                data = allElements.ToArray(),
                
            };
            if (template.Elements != null)
            {
                int i = 0;
                foreach (var element in template.Elements)
                {
                    var item = config.data.FirstOrDefault(t => t.key == ((int)element.ElementType).ToString());
                    item.a =element.a;
                    item.b =element.b;
                    item.selected = true;
                    i++;
                }
                config.selectedCount = i;
            }
            return Json(config);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult Save(string elements, string name,int width,int height,string backimage)//前台返回的的元素点的X、Y与宽、高的比例
        {
            elements = elements.Replace("\"[", "[").Replace("]\"", "]");
            var expressElements = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<ExpressElement>>(elements);

            ExpressCompany express = new ExpressCompany();
            express.Name = name;
            express.Height = height;
            express.Width = width;
            express.BackGroundImage = backimage;
            express.Elements = expressElements.Select(e => new ExpressElement
            {
                a = e.a,
                b = e.b,
                ElementType = (ExpressElementType)e.name,
            }).ToList();
            ExpressApplication.UpdateExpressAndElement(express);
            return Json(new Result { success = true });
        }
    }
}