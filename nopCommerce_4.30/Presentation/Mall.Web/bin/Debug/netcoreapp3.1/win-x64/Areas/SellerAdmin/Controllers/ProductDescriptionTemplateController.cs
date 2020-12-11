using Mall.Core;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Framework;
using Mall.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


namespace Mall.Web.Areas.SellerAdmin.Models
{
    public class ProductDescriptionTemplateController : BaseSellerController
    {
        private IProductDescriptionTemplateService _iProductDescriptionTemplateService;
        public ProductDescriptionTemplateController(IProductDescriptionTemplateService iProductDescriptionTemplateService)
        {
            _iProductDescriptionTemplateService = iProductDescriptionTemplateService;
        }
        // GET: SellerAdmin/ProductDescriptionTemplate
        public ActionResult Management()
        {
            return View();
        }



        [UnAuthorize]
        [HttpPost]
        public JsonResult List(int page, int rows, string name, int? position)
        {
            var templatePosition = (ProductDescriptionTemplateInfo.TemplatePosition?)position;
            var pageModel = _iProductDescriptionTemplateService.GetTemplates(CurrentSellerManager.ShopId, page, rows, name, templatePosition);

            var dataGrid = new DataGridModel<ProductDescriptionTemplateModel>()
            {
                rows = pageModel.Models.ToArray().Select(item => new ProductDescriptionTemplateModel()
                {
                    HtmlContent = item.Content,
                    Id = item.Id,
                    Name = item.Name,
                    Position = (int)item.Position,
                    PositionText = item.Position.ToDescription()
                }),
                total = pageModel.Total
            };
            return Json(dataGrid);
        }


        [HttpPost]
        public JsonResult GetAll()
        {
            var templates = _iProductDescriptionTemplateService.GetTemplates(CurrentSellerManager.ShopId).ToArray();

            var models = templates.Select(item => new ProductDescriptionTemplateModel()
            {
                HtmlContent = item.Content,
                Id = item.Id,
                Name = item.Name,
                Position = (int)item.Position,
                PositionText = item.Position.ToDescription()
            });

            return Json(models);
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult GetTemplateForSelect()
        {
            var templates = _iProductDescriptionTemplateService.GetTemplates(CurrentSellerManager.ShopId).ToArray();

            var models = templates.Select(item => new
            {
                Id = item.Id,
                Name = item.Name,
                Position = (int)item.Position
            });

            return Json(models);
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult Delete(long id)
        {
            _iProductDescriptionTemplateService.DeleteTemplate(CurrentSellerManager.ShopId, id);
            return Json(new { success = true });
        }


        public ActionResult Add(long id)
        {
            ProductDescriptionTemplateInfo template = new ProductDescriptionTemplateInfo();
            if (id > 0)
                template = _iProductDescriptionTemplateService.GetTemplate(id, CurrentSellerManager.ShopId);

            var model = new ProductDescriptionTemplateModel()
            {
                Id = template.Id,
                Position = (int)template.Position,
                Name = template.Name,
                HtmlContent = template.Content,
                MobileHtmlContent = template.MobileContent
            };

            return View(model);
        }

       
        [HttpPost]
        public JsonResult Add(ProductDescriptionTemplateModel model)
        {
            var template = new ProductDescriptionTemplateInfo()
            {
                Id = model.Id,
                Content = model.HtmlContent,
                MobileContent = model.MobileHtmlContent,
                Name = model.Name,
                Position = (ProductDescriptionTemplateInfo.TemplatePosition)model.Position,
                ShopId = CurrentSellerManager.ShopId
            };
            var service = _iProductDescriptionTemplateService;
            if (model.Id > 0)
                service.UpdateTemplate(template);
            else
                service.AddTemplate(template);
            return Json(new { success = true });
        }
    }
}