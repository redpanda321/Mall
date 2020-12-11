using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Web.Areas.Admin.Models;
using Mall.Web.Framework;
using Mall.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Mall.Web.Areas.Admin.Controllers
{
    public class VerificationCodeController : BaseAdminController
    {
        // GET: SellerAdmin/VerificationCode
        public ActionResult Management()
        {
            bool isOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            ViewBag.IsOpenStore = isOpenStore;
            return View();
        }

        [HttpPost]
        public JsonResult List(VerificationRecordQuery query, int page, int rows)
        {
            query.PageNo = page;
            query.PageSize = rows;
            var orderVerificationCode = OrderApplication.GetOrderVerificationCodeInfos(query);
            DataGridModel<OrderVerificationCodeModel> dataGrid = new DataGridModel<OrderVerificationCodeModel>() { rows = orderVerificationCode.Models, total = orderVerificationCode.Total };
            return Json(dataGrid);
        }

        public JsonResult GetShopAndShopBranch(string keyWords)
        {
            var result = OrderApplication.GetShopOrShopBranch(keyWords);
            var values = result.Select(item => new { type = item.Type, value = item.Name, id = item.SearchId });
            return Json(values);
        }
    }
}