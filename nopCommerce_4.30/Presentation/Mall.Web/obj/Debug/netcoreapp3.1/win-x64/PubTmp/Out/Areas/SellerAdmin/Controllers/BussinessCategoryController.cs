using Mall.Core;
using Mall.Core.Plugins;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.DTO;
using Mall.Web.Areas.Admin.Models;
using Mall.Web.Areas.Admin.Models.Product;
using Mall.Web.Framework;
using Mall.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using Mall.Application;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class BussinessCategoryController : BaseSellerController
    {
        private IShopService _iShopService;
        private ICategoryService _iCategoryService;
        public BussinessCategoryController(IShopService iShopService, ICategoryService iCategoryService)
        {
            _iShopService = iShopService;
            _iCategoryService = iCategoryService;
        }
        public ActionResult Management()
        {
            return View();
        }

        public ActionResult ApplyList()
        {
            return View();
        }

        public JsonResult GetApplyList(int page, int rows)
        {
            BussinessCateApplyQuery query = new BussinessCateApplyQuery();
            query.PageNo = page;
            query.PageSize = rows;
            query.shopId = CurrentSellerManager.ShopId;
            var model = _iShopService.GetBusinessCateApplyList(query);
            var cate = model.Models.ToList().Select(a => new { Id = a.Id, ShopName = a.ShopName, ApplyDate = a.ApplyDate.ToString("yyyy-MM-dd HH:mm"), AuditedStatus = a.AuditedStatus.ToDescription() });
            var p = new { rows = cate.ToList(), total = model.Total };
            return Json(p);
        }

        public ActionResult ApplyDetail(long id)
        {
            var model = _iShopService.GetBusinessCategoriesApplyInfo(id);
            var details = _iShopService.GetBusinessCategoriesApplyDetails(id);
            var categories = CategoryApplication.GetCategories();
            foreach (var item in details)
            {
                var path = CategoryApplication.GetCategoryPath(categories, item.CategoryId);
                item.CatePath = string.Join(">", path.Select(p => p.Name));
            }
            ViewBag.Details = details;
            return View(model);
        }


        public ActionResult List(int page, int rows)
        {
            var data = _iShopService.GetBusinessCategory(CurrentSellerManager.ShopId, page, rows);
            var list = data.Models.Select(a => new { a.Id, a.CommisRate, a.CategoryName }).ToList();
            return Json(new { rows = list, total = data.Total });
        }

        public ActionResult Apply()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetCategories(long? key = null, int? level = -1)
        {
            var categories = _iCategoryService.GetValidBusinessCategoryByParentId(key.GetValueOrDefault());
            var models = categories.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
            return Json(models);
        }

        public JsonResult GetBussinessCate(long id)
        {
            var categories = _iShopService.GetThirdBusinessCategory(id, CurrentSellerManager.ShopId);
            var t = categories.Select(item => new { id = item.Id, rate = item.Rate, path = item.Path });
            return Json(t);
        }

        public JsonResult ApplyBussinessCate(string categoryIds)
        {
            List<long> arr = new List<long>();
            var ids= Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(categoryIds, arr);
            _iShopService.ApplyShopBusinessCate(CurrentSellerManager.ShopId, ids);
            return Json(new Result() { success = true, msg = "申请成功" });
        }
    }
}