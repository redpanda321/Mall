using Mall.Application;
using Mall.IServices;
using Mall.Web.Areas.Admin.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Admin.Controllers
{
    public class ArticleCategoryController : BaseAdminController
    {
        private IArticleCategoryService _iArticleCategoryService;

        public ArticleCategoryController(IArticleCategoryService iArticleCategoryService)
        {
            _iArticleCategoryService = iArticleCategoryService;
        }

        public ActionResult Management()
        {
            var service = _iArticleCategoryService;
            var articleCategories = service.GetArticleCategoriesByParentId(0);
            var sitesetting = SiteSettingApplication.SiteSettings;
            if (sitesetting != null)
            {
                ViewBag.IsOpenPC = sitesetting.IsOpenPC;
                if (!sitesetting.IsOpenPC && articleCategories != null)//如果未开启PC端授权，则只保留平台公告分类
                {
                    articleCategories = articleCategories.Where(p => p.Name == "平台公告").ToList();
                }
            }

            var models = articleCategories.ToArray().Select(item => new ArticleCategoryModel()
            {
                ParentId = item.ParentCategoryId,
                Name = item.Name,
                DisplaySequence = item.DisplaySequence,
                Id = item.Id,
                IsDefault = item.IsDefault
            }).ToArray();
            foreach (var model in models)
            {
                model.HasChild = service.GetArticleCategoriesByParentId(model.Id, false).Count() > 0;
            }
            return View(models);
        }

        [HttpPost]
        public JsonResult GetArticleCategories(long parentId)
        {
            var articleCategories = _iArticleCategoryService.GetArticleCategoriesByParentId(parentId);
            var models = articleCategories.Select(item => new ArticleCategoryModel() { Id = item.Id, Name = item.Name, DisplaySequence = item.DisplaySequence, HasChild = false, Depth = 2 }).ToList();
            var service = _iArticleCategoryService;
            foreach (var model in models)
            {
                model.HasChild = service.GetArticleCategoriesByParentId(model.Id, false).Count() > 0;
            }
            return Json(models);

        }

        [HttpPost]
        public ActionResult Add(long? id, string name, long parentId)
        {
            Entities.ArticleCategoryInfo articleCategory = new Entities.ArticleCategoryInfo()
            {
                Id = id.GetValueOrDefault(),
                Name = name,
                ParentCategoryId = parentId
            };
            var service = _iArticleCategoryService;
            if (service.CheckHaveRename(articleCategory.Id, articleCategory.Name))
            {
                return Json(new Result { success = false, msg = "不可添加、修改为同名栏目！" });
            }
            if (id > 0)
                service.UpdateArticleCategory(articleCategory);
            else
                service.AddArticleCategory(articleCategory);
            return Json(new Result { success = true });
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult GetArticleCategory(long id)
        {
            var articleCategory = _iArticleCategoryService.GetArticleCategory(id);
            var model = new { id = id, name = articleCategory.Name, parentId = articleCategory.ParentCategoryId };
            return Json(model);

        }

        [HttpPost]
        public JsonResult Delete(long id)
        {

            _iArticleCategoryService.DeleteArticleCategory(id);
            return Json(new Result { success = true });
        }


        [HttpPost]
        public JsonResult BatchDelete(string ids)
        {
            long[] ids_long = ids.Split(',').Select(item => long.Parse(item)).ToArray();
            _iArticleCategoryService.DeleteArticleCategory(ids_long);
            return Json(new Result { success = true });
        }

        [HttpPost]
        public JsonResult UpdateOrder(long id, int displaySequence)
        {
            _iArticleCategoryService.UpdateArticleCategoryDisplaySequence(id, displaySequence);
            return Json(new Result { success = true });
        }


        [HttpPost]
        public JsonResult UpdateName(long id, string name)
        {
            var service = _iArticleCategoryService;
            if (service.CheckHaveRename(id, name))
            {
                return Json(new Result { success = false, msg = "不可添加、修改为同名栏目！" });
            }
            _iArticleCategoryService.UpdateArticleCategoryName(id, name);
            return Json(new Result { success = true });
        }


        [UnAuthorize]
        [HttpPost]
        public JsonResult GetCategories(long? key = null, int? level = -1)
        {
            if (level == -1)
                key = 0;

            if (key.HasValue)
            {
                var categories = _iArticleCategoryService.GetArticleCategoriesByParentId(key.Value).ToArray();
                var cateoriesPair = categories.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
                var sitesetting = SiteSettingApplication.SiteSettings;
                if (sitesetting != null)
                {
                    if (!sitesetting.IsOpenPC && cateoriesPair != null)//如果未开启PC端授权，则只保留平台公告分类
                    {
                        cateoriesPair = cateoriesPair.Where(p => p.Value == "平台公告");
                    }
                }
                return Json(cateoriesPair);
            }
            else
                return Json(new object[] { });
        }
    }
}