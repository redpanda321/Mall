using Mall.Application;
using Mall.CommonModel;
using Mall.Core.Helper;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.IServices;

using Mall.Web.Framework;
using Mall.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;


namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class StatisticsController : BaseSellerController
    {
        private IStatisticsService _iStatisticsService;
        public StatisticsController(IStatisticsService iStatisticsService)
        {
            _iStatisticsService = iStatisticsService;
        }
        protected string RenderPartialViewToString(Controller controller, string partialViewName)
        {
            var razorEngine = EngineContext.Current.Resolve<IRazorViewEngine>();

            IView view = razorEngine.FindView(controller.ControllerContext, partialViewName,false).View;
            using (StringWriter writer = new StringWriter())
            {
                ViewContext viewContext = new ViewContext(controller.ControllerContext, view, controller.ViewData, controller.TempData, writer,null);
                viewContext.View.RenderAsync(viewContext).Wait();
                return writer.GetStringBuilder().ToString();
            }
        }
        #region 店铺总流量
        public ActionResult ShopFlow()
        {
            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            return View();
        }

        private List<SelectListItem> GetYearDrop(int start, int end)
        {
            List<SelectListItem> yearDrop = new List<SelectListItem>();
            for (int i = start; i < end; i++)
            {
                yearDrop.Add(new SelectListItem { Selected = (DateTime.Now.Year == i), Text = i.ToString(), Value = i.ToString() });
            }

            return yearDrop;

        }

        private List<SelectListItem> GetMonthDrop()
        {
            List<SelectListItem> monthDrop = new List<SelectListItem>();
            for (int i = 1; i < 13; i++)
            {
                monthDrop.Add(new SelectListItem { Selected = (DateTime.Now.Month == i), Text = i.ToString(), Value = i.ToString() });
            }

            return monthDrop;

        }

        private List<SelectListItem> GetWeekDrop(int year, int month)
        {
            List<SelectListItem> weekDrop = new List<SelectListItem>();
            var start = DateTimeHelper.GetStartDayOfWeeks(year, month, 1);
            for (int i = 1; i <= 4; i++)
            {
                weekDrop.Add(new SelectListItem
                {
                    Selected = i == 1,
                    Text = string.Format("{0} -- {1}",
                        start.ToString("yyyy-MM-dd"), start.AddDays(6).ToString("yyyy-MM-dd")),
                    Value = i.ToString()
                });
                start = start.AddDays(7);
            }
            return weekDrop;
        }

        [UnAuthorize]
        [HttpGet]
        public JsonResult GetShopFlowChartByMonth(int year = 0, int month = 0, DateTime? begin = null, DateTime? end = null)
        {
            var shop = CurrentSellerManager.ShopId;
            if (begin.HasValue && end.HasValue)
            {
                var data = StatisticApplication.GetShopFlowChart(shop, begin.Value, end.Value);
                return Json(new { success = true, chart = data });
            }
            else
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                var data = StatisticApplication.GetShopFlowChart(shop, year, month);
                return Json(new { success = true, chart = data });
            }
        }



        #endregion

        #region 商品流量排行
        [UnAuthorize]
        [HttpGet]
        public ActionResult ProductVisitRanking()
        {
            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            ViewBag.WeekDrop = GetWeekDrop(DateTime.Now.Year, DateTime.Now.Month);
            return View();
        }
        [UnAuthorize]
        [HttpGet]
        public JsonResult ProductVisitRankingChart(string day = "", int year = 0, int month = 0, int weekIndex = 0, DateTime? begin = null, DateTime? end = null)
        {
            var model = new LineChartDataModel<int>();
            var shop = CurrentSellerManager.ShopId;
            if (begin.HasValue && end.HasValue)
            {
                model = StatisticApplication.GetProductSaleRankingChart(shop, begin.Value, end.Value, SaleDimension.Visti, 15);
            }
            else if (!string.IsNullOrWhiteSpace(day))
            {
                var date = DateTime.Parse(day);
                model = StatisticApplication.GetProductSaleRankingChart(shop, date, date, SaleDimension.Visti, 15);
            }
            else
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                model = StatisticApplication.GetProductSaleRankingChart(CurrentSellerManager.ShopId, year, month, weekIndex, SaleDimension.Visti, 15);
            }
            return Json(new { success = true, chart = model });
        }

        #endregion
        #region 商品销售排行

        [HttpGet]
        [UnAuthorize]
        public ActionResult ProductSaleRanking()
        {
            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            ViewBag.WeekDrop = GetWeekDrop(DateTime.Now.Year, DateTime.Now.Month);
            return View();
        }


        [HttpGet]
        [UnAuthorize]
        public JsonResult GetSaleRankingChart(string day = "", int year = 0, int month = 0, int weekIndex = 0, SaleDimension dimension =  SaleDimension.Count,DateTime? begin=null,DateTime? end=null)
        {
            var shop = CurrentSellerManager.ShopId;
            LineChartDataModel<int> model = new LineChartDataModel<int>();
            if (begin.HasValue && end.HasValue)
                model = StatisticApplication.GetProductSaleRankingChart(shop, begin.Value, end.Value, dimension,15);
            else if (!string.IsNullOrWhiteSpace(day))
            {
                var date = DateTime.Parse(day);//单日
                model = StatisticApplication.GetProductSaleRankingChart(shop, date, date, dimension, 15);
            }
            else
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                model = StatisticApplication.GetProductSaleRankingChart(shop, year, month, weekIndex, dimension, 15);
            }
            return Json(new { success = true, chart = model });
        }

        #endregion


        #region 店铺总销量

        public ActionResult ShopSale()
        {
            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            return View();
        }


        [UnAuthorize]
        [HttpGet]
        public JsonResult GetShopSaleChartByMonth(int year = 0, int month = 0, DateTime? begin = null, DateTime? end = null)
        {
            var shop = CurrentSellerManager.ShopId;
            if (begin.HasValue && end.HasValue)
            {
                var data = StatisticApplication.GetShopSaleCountChartByRange(shop, string.Format("{0}至{1}店铺总销量", begin.Value.ToString("yyyy-MM-dd"), end.Value.ToString("yyyy-MM-dd")), begin.Value, end.Value);
                return Json(new { success = true, chart = data });
            }
            else
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                var data = StatisticApplication.GetShopSaleCountChartByMonth(shop, year, month);
                return Json(new { success = true, chart = data });
            }
        }


        #endregion

        #region 成交转化率


        public ActionResult DealConversionRate()
        {
            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            return View();
        }

        [HttpGet]
        [UnAuthorize]
        public JsonResult GetDealConversionRateChartByMonth(int year = 0, int month = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;
            var data = StatisticApplication.GetDealConversionRateChart(CurrentSellerManager.ShopId, year, month);
            return Json(new { success = true, chart = data });
        }

        #endregion

        [UnAuthorize]
        public JsonResult GetWeekList(int year = 0, int month = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;
            var data = GetWeekDrop(year, month);
            return Json(new { success = true, week = data });
        }

        public ActionResult ProductSaleStatistic()
        {
            return View();
        }
        public JsonResult GetProductSaleStatisticList(ProductStatisticQuery query)
        {
            query.ShopId = CurrentSellerManager.ShopId;
            var model = StatisticApplication.GetProductSales(query);
            return Json(model);
        }

        public ActionResult ProductSaleCategoryStatistic()
        {
            var yesterday = DateTime.Now.Date.AddDays(-1);
            var mode = StatisticApplication.GetProductCategorySales(CurrentSellerManager.ShopId, yesterday, yesterday);
            return View(mode);
        }

        public JsonResult GetProductSaleCategoryStatistic(DateTime startDate, DateTime endDate)
        {
            var productCateSales = StatisticApplication.GetProductCategorySales(CurrentSellerManager.ShopId, startDate, endDate);
            return Json(new { success = true, model = productCateSales });
        }

        public ActionResult TradeStatistic()
        {
            var yesterday = DateTime.Now.Date.AddDays(-1);
            var platTradeStatistic = StatisticApplication.GetShopTradeStatistic(CurrentSellerManager.ShopId, null, yesterday, yesterday);
            #region 是否开启门店授权
            bool isOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            if (isOpenStore)
            {
                #region 商家下所有门店
                var data = ShopBranchApplication.GetShopBranchsAll(new ShopBranchQuery()
                {
                    ShopId = CurrentSellerManager.ShopId
                });
                ViewBag.StoreList = data.Models;
                #endregion
            }
            ViewBag.IsOpenStore = isOpenStore;
            var shop = ShopApplication.GetShop(CurrentSellerManager.ShopId);
            ViewBag.ShopName = shop.ShopName;
            #endregion
            return View(platTradeStatistic);
        }
        public JsonResult GetPlatTradeStatistic(DateTime startDate, DateTime endDate,int type, long? shopbranchId)
        {
            var platTradeStatistic = StatisticApplication.GetShopTradeStatistic(CurrentSellerManager.ShopId, shopbranchId, startDate, endDate, type);
            return Json(new { success = true, model = platTradeStatistic });
        }


        #region 导出
        public ActionResult ExportTradeStatistic(DateTime startDate, DateTime endDate, long? shopbranchId)
        {
            var tradeStatistic = StatisticApplication.GetShopTradeStatisticItem(CurrentSellerManager.ShopId, shopbranchId, startDate, endDate);
            ViewData.Model = tradeStatistic;
            string Title = startDate.ToString("yyyy-MM-dd") + "至" + endDate.ToString("yyyy-MM-dd") + "交易统计数据";
            ViewData.Add("Title", Title);
            string viewHtml = RenderPartialViewToString(this, "ExportTradeStatistic");

            return File(System.Text.Encoding.UTF8.GetBytes(viewHtml), "application/ms-excel", "交易数据导出.xls");
        }

        public ActionResult ExportProductStatistic(ProductStatisticQuery query)
        {
            DateTime end = query.EndDate;
            query.ShopId = CurrentSellerManager.ShopId;
            query.PageSize = int.MaxValue;
            var model = StatisticApplication.GetProductSales(query);
            ViewData.Model = model.Models;
            string Title = query.StartDate.ToString("yyyy-MM-dd") + "至" + end.ToString("yyyy-MM-dd") + "商品统计数据";
            ViewData.Add("Title", Title);
            string viewHtml = RenderPartialViewToString(this, "ExportProductStatistic");
            return File(System.Text.Encoding.UTF8.GetBytes(viewHtml), "application/ms-excel", "商品销售情况.xls");
        }
        #endregion 
    }
}