using System;
using System.Collections.Generic;
using System.Linq;

using Mall.Core;
using Mall.DTO;
using Mall.Web.Framework;
using Mall.CommonModel;
using Mall.Application;
using Mall.DTO.QueryModel;
using Mall.Web.Areas.Admin.Models.Distribution;
using Mall.DTO.Distribution;
using Mall.CommonModel;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    //[H5Authorization]
    /// <summary>
    /// 分销商品
    /// </summary>
    public class DistributionProductController : BaseAdminController
    {
        public ActionResult Management()
        {
            return View();
        }

        /// <summary>
        /// 获取分销商品
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetProductList(string productName, string shopname, int rows, int page, DistributionProductStatus? status = null, string Sort = "",bool IsAsc=false)
        {
            //查询条件
            DistributionProductQuery query = new DistributionProductQuery();
            query.ProductName = productName;
            query.ShopName = shopname;
            query.Status = status;
            query.PageSize = rows;
            query.PageNo = page;
            query.Sort = Sort;
            query.IsAsc = IsAsc;

            var data = DistributionApplication.GetProducts(query);
            var result = new { rows = data.Models, total = data.Total };
            return Json(result);
        }

        /// <summary>
        /// 导出分销商品
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ActionResult ExportToExcel(DistributionProductQuery query)
        {
            var results = DistributionApplication.GetProductsAll(query);

            return ExcelView("ExportDistributionProduct", "平台分销商品", results);
        }
    }
}