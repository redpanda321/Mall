using Mall.IServices;

using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class CategoryController : BaseSellerController
    {
        private ICategoryService _iCategoryService;
        private IShopCategoryService _iShopCategoryService;
        public CategoryController(ICategoryService iCategoryService, IShopCategoryService iShopCategoryService)
        {
            _iCategoryService = iCategoryService;
            _iShopCategoryService = iShopCategoryService;
        }

        // GET: SellerAdmin/Category
        public ActionResult Management(int? isAll)
        {
            var ICategory = _iShopCategoryService;
            IEnumerable<Entities.ShopCategoryInfo> category = null;
            if (isAll.HasValue && isAll.Value == 1)
            {
                category = ICategory.GetShopCategory(CurrentSellerManager.ShopId);
            }
            else
            {
                category = ICategory.GetMainCategory(CurrentSellerManager.ShopId);
            }
            List<ShopCategoryModel> list = new List<ShopCategoryModel>();
            foreach (var item in category)
            {
                list.Add(new ShopCategoryModel(item));
            }
            return View(list);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult GetEffectCategory(long categoryId)
        {
            var cate = _iCategoryService.GetCategory(categoryId);
            string names = _iCategoryService.GetEffectCategoryName(CurrentSellerManager.ShopId, cate.TypeId);
            return Json(new { json = names });
        }

        [UnAuthorize]
        public JsonResult GetCategoryDrop(long id = 0)
        {
            List<SelectListItem> cateList = new List<SelectListItem>{ new SelectListItem
                {
                    Selected = id==0,
                    Text ="请选择...",
                    Value = "0"
                }
            };
            var cateMain = _iShopCategoryService.GetMainCategory(CurrentSellerManager.ShopId);
            foreach (var item in cateMain)
            {
                cateList.Add(new SelectListItem
                {
                    Selected = id == item.Id,
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }
            return Json(new { success = true, category = cateList });
        }

        [HttpPost]
        [ShopOperationLog("创建店铺分类", "pid,name")]
        public JsonResult CreateCategory(string name, long pId)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 12)
                throw new Exception("分类名称长度不能多于12位");



            var cate = new Entities.ShopCategoryInfo
            {
                Name = name,
                ParentCategoryId = pId,
                IsShow = true,
                DisplaySequence = _iShopCategoryService.GetCategoryByParentId(pId).Count() + 1,
                ShopId = CurrentSellerManager.ShopId
            };
            _iShopCategoryService.AddCategory(cate);
           // ServiceApplication.Create<IOperationLogService>().AddSellerOperationLog(
           //new LogInfo
           //{
           //    Date = DateTime.Now,
           //    Description = "创建店铺分类，父Id=" + pId,
           //    IPAddress =  base.Request.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress.ToString(),
           //    PageUrl = "/Category/CreateCategory",
           //    UserName = CurrentSellerManager.UserName,
           //    ShopId = CurrentSellerManager.ShopId
           //});
            return Json(new { success = true });
        }

        [ShopOperationLog("修改店铺分类名称", "id,name")]
        public JsonResult UpdateName(string name, long id)
        {
            _iShopCategoryService.UpdateCategoryName(id, name);

            return Json(new { success = true });
        }

        [UnAuthorize]
        public JsonResult UpdateOrder(long order, long id)
        {
            _iShopCategoryService.UpdateCategoryDisplaySequence(id, order);
            return Json(new { success = true });
        }
        [UnAuthorize]
        public JsonResult UpdateCategoryShow(bool isShow, long id)
        {
            //_iShopCategoryService.UpdateCategoryShow(id, !isShow);
            var ids = _iShopCategoryService.GetSecondAndThirdLevelCategories(new long[] { id}).Select(a=>a.Id).ToList();
            var pids = _iShopCategoryService.GetParentCategoryById(id, isShow).Select(a => a.Id).ToList();
            if (ids != null)
            {
                ids.Add(id);
                _iShopCategoryService.UpdateCategorysShow(!isShow,ids);
                _iShopCategoryService.UpdateCategorysShow(!isShow, pids);
            }
            //HttpResponse.RemoveOutputCacheItem(string.Format("/Shop/Home/{0}", this.CurrentShop.Id)); //移除页面缓存
           // HttpResponse.RemoveOutputCacheItem(string.Format("/shop/home/{0}", this.CurrentShop.Id));
            return Json(new { success = true });
        }

        [UnAuthorize]
        public ActionResult GetCategoryByParentId(int id)
        {
            List<ShopCategoryModel> list = new List<ShopCategoryModel>();
            var categoryList = _iShopCategoryService.GetCategoryByParentId(id);
            foreach (var item in categoryList)
            {
                list.Add(new ShopCategoryModel(item));
            }
            return Json(new { success = true, Category = list });
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult GetAllShopCategory()
        {
            var categories = _iShopCategoryService.GetShopCategory(CurrentSellerManager.ShopId);
            return SuccessResult<dynamic>(data: categories);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult GetCategory(long? key = null, int? level = -1)
        {
            if (level == -1)
                key = 0;

            if (key.HasValue)
            {
                var categories = _iShopCategoryService.GetCategoryByParentId(key.Value, CurrentSellerManager.ShopId);
                var cateoriesPair = categories.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
                return Json(cateoriesPair);
            }
            else
                return Json(new object[] { });
        }


        [UnAuthorize]
        [HttpPost]
        public JsonResult GetSystemCategory(long? key = null, int? level = -1)
        {
            if (level == -1)
                key = 0;

            if (key.HasValue)
            {
                var categories = _iCategoryService.GetCategoryByParentId(key.Value);
                var cateoriesPair = categories.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
                return Json(cateoriesPair);
            }
            else
                return Json(new object[] { });
        }

        [HttpPost]
        [ShopOperationLog("删除店铺分类", "id")]
        public JsonResult DeleteCategoryById(long id)
        {
            _iShopCategoryService.DeleteCategory(id, CurrentSellerManager.ShopId);
           // ServiceApplication.Create<IOperationLogService>().AddSellerOperationLog(
           //new LogInfo
           //{
           //    Date = DateTime.Now,
           //    Description = "删除店铺分类，Id=" + id ,
           //    IPAddress =  base.Request.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress.ToString(),
           //    PageUrl = "/Category/DeleteCategoryById",
           //    UserName = CurrentSellerManager.UserName,
           //    ShopId = CurrentSellerManager.ShopId
           //});
            return Json(new { success = true });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult BatchDeleteCategory(string Ids)
        {
            int id;
            foreach (var idStr in Ids.Split('|'))
            {
                if (string.IsNullOrWhiteSpace(idStr)) continue;
                if (int.TryParse(idStr, out id))
                {
                    _iShopCategoryService.DeleteCategory(id, CurrentSellerManager.ShopId);
                }
            }
            return Json(new { success = true });
        }
    }
}