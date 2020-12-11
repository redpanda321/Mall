using Mall.Application;
using Mall.CommonModel;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    [MarketingAuthorization]
    public class DistributionController : BaseSellerController
    {
        public ActionResult Orders(long product = 0)
        {
            ViewBag.MaxLevel = SiteSettingApplication.SiteSettings.DistributionMaxLevel;
            ViewBag.Product = product;
            return View();
        }

        [HttpPost]
        public ActionResult Orders(BrokerageOrderQuery query)
        {
            query.ShopId = CurrentShop.Id;
            if (query.SettlementEnd.HasValue)
                query.SettlementEnd = query.SettlementEnd.Value.AddDays(1);
            var data = DistributionApplication.GetBrokerageOrders(query);
            return Json(data, true);
        }

        /// <summary>
        /// 导出分销订单业绩
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ActionResult OrdersExportToExcel(BrokerageOrderQuery query)
        {
            query.ShopId = CurrentSellerManager.ShopId;
            if (query.SettlementEnd.HasValue)
                query.SettlementEnd = query.SettlementEnd.Value.AddDays(1);
            var orderResults = DistributionApplication.GetBrokerageOrdersAll(query);

            return ExcelView("OrdersExport", "商家分销订单业绩", orderResults);
        }

        public ActionResult Products()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Products(DistributionProductQuery query)
        {
            query.ShopId = CurrentShop.Id;
            var data = DistributionApplication.GetProducts(query);
            var list = data.Models.Select(item => new
            {
                item.ProductId,
                item.ProductName,
                Status = item.ProductStatus.ToDescription(),
                Quantity=item.SaleCount,
                Amount=item.SaleAmount.ToString("F2"),
                Settlement=item.SettlementAmount.ToString("F2"),
                NoSettlement=item.NoSettlementAmount.ToString("F2")
            });
            return Json(new { rows = list, total = data.Total }, true);
        }

        /// <summary>
        /// 导出商品分销明细
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ActionResult ProductsExportToExcel(DistributionProductQuery query)
        {
            query.ShopId = CurrentSellerManager.ShopId;
            var results = DistributionApplication.GetProductsAll(query);

            return ExcelView("ProductsExport", "商家商品分销明细", results);
        }
    }
}