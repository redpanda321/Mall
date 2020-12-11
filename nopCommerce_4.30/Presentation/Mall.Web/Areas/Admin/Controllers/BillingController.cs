using Mall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

using Mall.CommonModel;
using Mall.Core.Plugins.Payment;
using Mall.Application;
using Mall.Web.Areas.Admin.Models;
using Mall.DTO.QueryModel;
using Mall.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class BillingController : BaseAdminController
    {
        public ActionResult Index()
        {
            var model = BillingApplication.GetPlatBillingIndex();
            return View(model);
        }
        public ActionResult GetSevenDaysTradeChart()
        {
            var enddate = DateTime.Now;
            var startdate = DateTime.Now.AddDays(-6);
            var data = BillingApplication.GetTradeChart(startdate, enddate, 0);
            return Json(new { success = true, chart = data });
        }

        public ActionResult GetThirdtyDaysTradeChart()
        {
            var enddate = DateTime.Now;
            var startdate = DateTime.Now.AddDays(-29);
            var data = BillingApplication.GetTradeChart(startdate, enddate, 0);
            return Json(new { success = true, chart = data });
        }
        public ActionResult GetTradeChartMonthChart()
        {
            var enddate = DateTime.Now;
            var startdate = new DateTime(enddate.Year, enddate.Month, 1);
            var data = BillingApplication.GetTradeChartMonth(startdate, enddate, 0);
            return Json(new { success = true, chart = data });
        }
        public ActionResult PendingSettlementOrders()
        {
            ViewBag.Payments = PaymentApplication.GetPaymentTypeDesc();
            var model = BillingApplication.GetPlatSettlementCycle();
            return View(model);
        }

        /// <summary>
        /// 待结算列表
        /// </summary>
        /// <returns></returns>
        public ActionResult PendingSettlementList()
        {
            var model = BillingApplication.GetCurrentBilingTime();
            return View(model);
        }

        private List<string> GetPayment()
        {
            List<string> payNames = new List<string>();
            var payments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).OrderByDescending(d => d.PluginInfo.PluginId).ToList();
            foreach (var pay in payments)
            {
                payNames.Add(pay.PluginInfo.DisplayName);
            }
            return payNames;
        }


        public ActionResult PlatSettlementOrders()
        {
            ViewBag.Payments = PaymentApplication.GetPaymentTypeDesc();
            var model = BillingApplication.GetPlatSettlementStatistics();
            return View("SettlementOrders", model);
        }


        public ActionResult SettlementList()
        {
            return View();
        }

        [HttpPost]
        public JsonResult PendingSettlementOrderList(DateTime? startDate, DateTime? endDate, string shopName, string paymentName, long? orderId, long? shopId, int page, int rows)
        {
            var query = new PendingSettlementOrderQuery();
            query.CreateDateStart = startDate;
            query.CreateDateEnd = endDate;
            query.PageNo = page;
            query.PaymentName = paymentName;
            query.ShopName = shopName;
            query.PageSize = rows;
            query.OrderId = orderId;
            query.ShopId = shopId;
            var model = BillingApplication.GetPendingSettlementOrders(query);
            var result = new { rows = model.Models, total = model.Total };
            return Json(result);
        }

        [HttpPost]
        public JsonResult PendingSettlementList(StatisticsPendingSettlementQuery query)
        {
            var result = BillingApplication.StatisticsPendingSettlementOrders(query);
            if (result.Models != null)
            {
                foreach (var item in result.Models)
                {
                    item.ShopName = ShopApplication.GetShop(item.ShopId) == null ? item.ShopName : ShopApplication.GetShop(item.ShopId).ShopName;
                }
            }
            return Json(new { rows = result.Models, result.Total }, true);
        }

        public ActionResult ExportPendingSettlementList(string shopName)
        {
            var query = new StatisticsPendingSettlementQuery();
            query.ShopName = shopName;
            var result = BillingApplication.AllStatisticsPendingSettlementOrders(query);
            var shopNames = ShopApplication.GetShopNames(result.Select(p => p.ShopId).ToList());

            var model = new ExportPendingSettlementListModel();
            model.Data = result;
            model.ShopNames = shopNames;
            model.SettmentCycle = BillingApplication.GetCurrentBilingTime();

            return ExcelView("待结算列表", model);
        }

        public ActionResult ExportPendingSettlementOrders(DateTime? startDate, DateTime? endDate, string shopName, string paymentName, long? orderId, long? shopId)
        {
            var query = new PendingSettlementOrderQuery();
            query.OrderStart = startDate;
            query.OrderEnd = endDate;
            query.PaymentName = paymentName;
            query.ShopName = shopName;
            query.OrderId = orderId;
            query.ShopId = shopId;
            var models = BillingApplication.GetAllPendingSettlementOrders(query);
            return ExcelView("待结算订单", models);
        }

        /// <summary>
        /// 已结算数据
        /// </summary>
        /// <param name="timeStart"></param>
        /// <param name="timeEnd"></param>
        /// <param name="shopName"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SettlementList(ShopAccountItemQuery query)
        {
            if (!query.TimeStart.HasValue)
                query.TimeStart = query.TimeEnd.HasValue ? query.TimeEnd.Value.AddYears(-1).Date : DateTime.Now.AddYears(-1).Date;
            query.ShopAccountType = ShopAccountType.SettlementIncome;
            var data = BillingApplication.GetShopAccountItem(query);

            var models = data.Models.Select(item => new SettlementListModel
            {
                Id = item.Id,
                ShopId = item.ShopId,
                ShopName = item.ShopName,
                Amount = item.Amount,
                DetailId = item.DetailId,
                CreateTime = item.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                Cycle = item.Cycle
            });

            return Json(new { rows = models, data.Total }, true);
        }


        /// <summary>
        /// 已结算数据
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="shopName"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public ViewResult ExportSettlementList(DateTime? startDate, DateTime? endDate, string shopName)
        {
            if (startDate == null)
                startDate = endDate.HasValue ? endDate.Value.AddYears(-1).Date : DateTime.Now.AddYears(-1).Date;

            var query = new ShopAccountItemQuery();
            query.ShopName = shopName;
            query.TimeStart = startDate;
            query.TimeEnd = endDate;
            query.ShopAccountType = ShopAccountType.SettlementIncome;

            var data = BillingApplication.GetAllShopAccountItem(query);
            return ExcelView("已结算列表", data);
        }

        /// <summary>
        /// 已结算订单
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="BillingstartDate"></param>
        /// <param name="BillingendDate"></param>
        /// <param name="shopName"></param>
        /// <param name="paymentName"></param>
        /// <param name="orderId"></param>
        /// <param name="detailId"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SettlementOrderList(DateTime? startDate, DateTime? endDate, DateTime? BillingstartDate, DateTime? BillingendDate, long? shopId, string paymentName, long? orderId, long? detailId, int page, int rows)
        {
            var query = new SettlementOrderQuery();
            query.OrderStart = startDate;
            query.OrderEnd = endDate;
            query.WeekSettlementId = detailId;
            query.SettleStart = BillingstartDate;
            query.SettleEnd = BillingendDate;
            query.PaymentName = paymentName;
            query.PageNo = page;
            query.PageSize = rows;
            query.OrderId = orderId;
            query.ShopId = shopId;
            var model = BillingApplication.GetSettlementOrders(query);
            var result = new { rows = model.Models, total = model.Total };
            return Json(result);
        }

        /// <summary>
        /// 导出已结算订单
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="BillingstartDate"></param>
        /// <param name="BillingendDate"></param>
        /// <param name="shopName"></param>
        /// <param name="paymentName"></param>
        /// <param name="orderId"></param>
        /// <param name="detailId"></param>
        /// <returns></returns>
        public ActionResult ExportSettlementOrders(DateTime? startDate, DateTime? endDate, DateTime? BillingstartDate, DateTime? BillingendDate, long? shopId, string paymentName, long? orderId, long? detailId)
        {
            var query = new SettlementOrderQuery();
            query.OrderStart = startDate;
            query.OrderEnd = endDate;
            query.WeekSettlementId = detailId;
            query.SettleStart = BillingstartDate;
            query.SettleEnd = BillingendDate;
            query.PaymentName = paymentName;
            query.OrderId = orderId;
            query.ShopId = shopId;
            var models = BillingApplication.GetAllSettlementOrders(query);
            return ExcelView("已结算订单", models);
        }

        public ActionResult MarketServiceRecordInfo(long Id)
        {
            var model = BillingApplication.GetMarketServiceRecord(Id);
            return View(model);
        }

        //平台帐户流水页面
        public ActionResult PlatAccountItem()
        {
            var model = BillingApplication.GetPlatAccount();
            return View(model);
        }

        /// <summary>
        /// 获取平台帐户流水
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="type"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetPlatAccountItemlist(DateTime? startDate, DateTime? endDate, PlatAccountType? type, int page, int rows)
        {
            PlatAccountItemQuery query = new PlatAccountItemQuery
            {
                PageNo = page,
                PageSize = rows,
                TimeStart = startDate,
                TimeEnd = endDate,
                PlatAccountType = type
            };
            var model = BillingApplication.GetPlatAccountItem(query);
            var result = new { rows = model.Models, total = model.Total };
            return Json(result);
        }

        /// <summary>
        /// 获取平台帐户流水
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="type"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public ActionResult ExportPlatAccountItems(DateTime? startDate, DateTime? endDate, PlatAccountType? type)
        {
            PlatAccountItemQuery query = new PlatAccountItemQuery();
            query.TimeStart = startDate;
            query.TimeEnd = endDate;
            query.PlatAccountType = type;
            var models = BillingApplication.GetAllPlatAccountItem(query);
            return ExcelView("结余明细", models);
        }

        public ActionResult PendingSettlementDetail(long Id)
        {
            var model = BillingApplication.GetPendingOrderSettlementDetail(Id);
            return View(model);
        }


        public ActionResult SettlementDetail(long Id)
        {
            var model = BillingApplication.GetOrderSettlementDetail(Id);
            return View(model);
        }


        public ActionResult SettledPaymentDetail(long id)
        {
            var model = ShopApplication.GetSettledPaymentRecord(id);

            return View(model);
        }

        public ActionResult SettlementOrders(long? shopId, long? detailId = null)
        {
            ViewBag.Payments = PaymentApplication.GetPaymentTypeDesc();
            var model = BillingApplication.GetShopSettlementStatistics(shopId, detailId);
            return View(model);
        }
    }
}