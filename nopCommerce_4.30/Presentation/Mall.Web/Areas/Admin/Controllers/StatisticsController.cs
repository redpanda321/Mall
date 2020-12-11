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


namespace Mall.Web.Areas.Admin.Controllers
{
    public class StatisticsController : BaseAdminController
    {
        IStatisticsService _iStatisticsService;
        IRegionService _iRegionService;
        public StatisticsController(IStatisticsService iStatisticsService, IRegionService iRegionService)
        {
            _iStatisticsService = iStatisticsService;
            _iRegionService = iRegionService;
        }
        #region 会员
        // GET: Admin/Chart
        [UnAuthorize]
        public ActionResult Member()
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
        public JsonResult GetMemberChartByMonth(int year = 0, int month = 0, DateTime? begin = null, DateTime? end = null)
        {
            var chart = new LineChartDataModel<int>();
            if (begin.HasValue && end.HasValue)
                chart = StatisticApplication.GetNewMemberChartByRange(begin.Value, end.Value);
            else
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                chart = StatisticApplication.GetNewMemberChartByMonth(year, month);
            }
            return Json(new { success = true, chart = chart });
        }


        public ActionResult ExportMemberByMonth(int year = 0, int month = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;
            var data = StatisticApplication.GetNewMemberChartByMonth(year, month);
            ViewData.Model = data;
            ViewData.Add("Title", year + "年" + month + "月");
            string viewHtml = RenderPartialViewToString(this, "ExportMemberByMonth");
            return File(System.Text.Encoding.UTF8.GetBytes(viewHtml), "application/ms-excel", string.Format("月份_{0}_{1}.xls", data.SeriesData[0].Name, DateTime.Now.ToString("yyyy-MM-dd")));
        }


        [NonAction]
        protected string RenderPartialViewToString(Controller controller, string partialViewName)
        {
            var razorEngine = EngineContext.Current.Resolve<IRazorViewEngine>();


            IView view = razorEngine.FindView(controller.ControllerContext, partialViewName, false).View;
            using (StringWriter writer = new StringWriter())
            {
                ViewContext viewContext = new ViewContext(controller.ControllerContext, view, controller.ViewData, controller.TempData, writer,null);
                viewContext.View.RenderAsync(viewContext).Wait();
                return writer.ToString();
            }
        }

        #endregion

        #region 区域地图
        [UnAuthorize]
        [HttpGet]
        public JsonResult GetAreaMapBySearch(SaleDimension dimension, int year = 0, int month = 0, DateTime? begin = null, DateTime? end = null)
        {
            if (begin.HasValue && end.HasValue)
            {
                end = end.Value.AddDays(1);
                var data = StatisticApplication.GetAreaOrderChart(dimension, begin.Value, end.Value);
                return Json(new { success = true, chart = data }, true);
            }
            else
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                var data = StatisticApplication.GetAreaOrderChart(dimension, year, month);
                return Json(new { success = true, chart = data }, true);
            }
        }

        public ActionResult ExportAreaMap(int year = 0, int month = 0, DateTime? begin = null, DateTime? end = null)
        {
            MapChartDataModel<decimal> orderCounts = new MapChartDataModel<decimal>();
            MapChartDataModel<decimal> orderAmounts = new MapChartDataModel<decimal>();
            var title = string.Empty;
            if (begin.HasValue && end.HasValue)
            {
                orderCounts = StatisticApplication.GetAreaOrderChart(SaleDimension.Count, begin.Value, end.Value);
                orderAmounts = StatisticApplication.GetAreaOrderChart(SaleDimension.Amount, begin.Value, end.Value);
                title = string.Format("区域统计_{0}至{1}", begin.Value.ToString("yyyy-MM-dd"), end.Value.ToString("yyyy-MM-dd"));
            }
            else
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                orderCounts = StatisticApplication.GetAreaOrderChart(SaleDimension.Count, year, month);
                orderAmounts = StatisticApplication.GetAreaOrderChart(SaleDimension.Amount, year, month);
                title = string.Format("区域统计_{0}年{1}月", year, month);
            }
            var list = new List<AreaMapExportSubModel>();
            for (int i = 0; i < orderCounts.Data.Count; i++)
            {
                list.Add(new AreaMapExportSubModel
                {
                    RegionName = orderCounts.Data[i].Name,
                    OrderCount = (int)orderCounts.Data[i].Value,
                    OrderAmount = orderAmounts.Data[i].Value
                });
            }
            ViewData.Model = list;
            ViewData.Add("Title", title);
            string viewHtml = RenderPartialViewToString(this, "ExportAreaMap");
            return File(System.Text.Encoding.UTF8.GetBytes(viewHtml), "application/ms-excel", title + ".xls");
        }


        [UnAuthorize]
        public ActionResult OrderAreaMap()
        {
            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            ViewBag.Year = DateTime.Now.Year;
            ViewBag.Month = DateTime.Now.Month;
            return View();
        }

        #endregion

        #region 店铺
        [UnAuthorize]
        public ActionResult NewShop()
        {
            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            return View();
        }

        [UnAuthorize]
        [HttpGet]
        public JsonResult GetNewShopChartByMonth(int year = 0, int month = 0, DateTime? begin = null, DateTime? end = null)
        {
            if (begin.HasValue && end.HasValue)
            {
                var chart = StatisticApplication.GetNewsShopChartByDate(begin.Value, end.Value);
                return Json(new { success = true, chart = chart });
            }
            else
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                var chart = StatisticApplication.GetNewsShopChartByMonth(year, month);
                return Json(new { success = true, chart = chart });
            }
        }

        [UnAuthorize]
        [HttpGet]
        public ActionResult ShopRanking()
        {
            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            ViewBag.WeekDrop = GetWeekDrop(DateTime.Now.Year, DateTime.Now.Month);
            return View();
        }
        [UnAuthorize]
        public JsonResult GetWeekList(int year = 0, int month = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;
            var data = GetWeekDrop(year, month);
            return Json(new { success = true, week = data });
        }

        [UnAuthorize]
        [HttpGet]
        public JsonResult GetShopRankingChart(string day = "", int year = 0, int month = 0, int weekIndex = 0, SaleDimension dimension = SaleDimension.Count, DateTime? begin = null, DateTime? end = null)
        {
            LineChartDataModel<int> model = new LineChartDataModel<int>();
            var rank = 15;
            if (begin.HasValue && end.HasValue)
                model = StatisticApplication.GetShopRankingChart(begin.Value, end.Value, dimension, rank);
            else if (!string.IsNullOrWhiteSpace(day))
            {
                var date = DateTime.Parse(day);
                model = StatisticApplication.GetShopRankingChart(date, date, dimension, rank);
            }
            else
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                model = StatisticApplication.GetShopRankingChart(year, month, weekIndex, dimension, rank);
            }
            return Json(new { success = true, chart = model });
        }

        #endregion

        #region 销售排行

        [HttpGet]
        [UnAuthorize]
        public ActionResult ProductSaleRanking()
        {
            ViewBag.YearDrop = GetYearDrop(2014, 2024);
            ViewBag.MonthDrop = GetMonthDrop();
            ViewBag.WeekDrop = GetWeekDrop(DateTime.Now.Year, DateTime.Now.Month);
            return View();
        }

        [UnAuthorize]
        [HttpGet]
        public JsonResult GetSaleRankingChart(string day = "", int year = 0, int month = 0, int weekIndex = 0, SaleDimension dimension =  SaleDimension.Count)
        {
            var model = new LineChartDataModel<int>();
            if (!string.IsNullOrWhiteSpace(day))
            {
                DateTime date = DateTime.Parse(day);
                StatisticApplication.GetProductSaleRankingChart(0, date, date, dimension, 15);
            }
            else
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                StatisticApplication.GetProductSaleRankingChart(0, year, month, weekIndex, dimension, 15);
            }
            return Json(new { success = true, chart = model });
        }
        public ActionResult ExportSaleRanking(string day = "", int year = 0, int month = 0, int weekIndex = 0)
        {
            #region 获取查询结果
            var result = new List<SalesExportModel>();
            var SaleCountmodel = new LineChartDataModel<int>();
            var Salesmodel = new LineChartDataModel<int>();
            var Title = string.Empty;
            if (!string.IsNullOrWhiteSpace(day))
            {
                DateTime date = DateTime.Parse(day);
                SaleCountmodel = StatisticApplication.GetProductSaleRankingChart(0, date, date, SaleDimension.Count, 15);
                Salesmodel = StatisticApplication.GetProductSaleRankingChart(0, date, date, SaleDimension.Amount, 15);
                Title = "按天统计：" + day;
            }
            else
            {
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                if (weekIndex == 0) Title = "按月统计：" + year + "年" + month + "月";
                else Title = "按周统计：" + year + "年" + month + "月 第" + weekIndex + "周";
                SaleCountmodel = StatisticApplication.GetProductSaleRankingChart(0, year, month, weekIndex, SaleDimension.Count, 15);
                Salesmodel = StatisticApplication.GetProductSaleRankingChart(0, year, month, weekIndex, SaleDimension.Amount, 15);
            }

            for (int i = 0; i < SaleCountmodel.ExpandProp.Length; i++)
            {
                SalesExportModel model = new SalesExportModel();
                if (!string.IsNullOrEmpty(SaleCountmodel.ExpandProp[i]))
                {
                    model.ProductName = SaleCountmodel.ExpandProp[i];
                    model.SaleCount = SaleCountmodel.SeriesData[0].Data[i];
                    model.SaleAmount = Salesmodel.SeriesData[0].Data[i];
                    result.Add(model);
                }
            }
            #endregion

            #region 构建EXCEL
            ViewData.Model = result;
            ViewData.Add("Title", Title);
            string viewHtml = RenderPartialViewToString(this, "ExportSaleRanking");

            return File(System.Text.Encoding.UTF8.GetBytes(viewHtml), "application/ms-excel", string.Format("销量分析_{0}.xls", DateTime.Now.ToString("yyyy-MM-dd")));
            #endregion
        }

        #endregion

        #region 商品统计
        public ActionResult ProductSaleStatistic()
        {
            return View();
        }
        public JsonResult GetProductSaleStatisticList(ProductStatisticQuery query)
        {
            var model = StatisticApplication.GetProductSales(query);
            return Json(model);
        }

        public ActionResult ProductSaleCategoryStatistic()
        {
            var startDate = DateTime.Now.Date.AddDays(-1);
            var endDate = startDate;//默认为一天
            var productCateSales = StatisticApplication.GetProductCategorySales(0,startDate, endDate);
            return View(productCateSales);
        }

        public JsonResult GetProductSaleCategoryStatistic(DateTime startDate, DateTime endDate)
        {
            var productCateSales = StatisticApplication.GetProductCategorySales(0,startDate, endDate);

            return Json(new { success = true, model = productCateSales });
        }
        #endregion 商品统计

        #region 交易统计
        public ActionResult TradeStatistic()
        {
            var startDate = DateTime.Now.Date.AddDays(-1);
            var endDate = startDate;//默认为一天
            var platTradeStatistic = StatisticApplication.GetPlatTradeStatistic(startDate, endDate);
            return View(platTradeStatistic);
        }
        public JsonResult GetPlatTradeStatistic(DateTime startDate, DateTime endDate)
        {
            var platTradeStatistic = StatisticApplication.GetPlatTradeStatistic(startDate, endDate);
            return Json(new { success = true, model = platTradeStatistic });
        }
        #endregion 交易统计

        #region 导出
        public ActionResult ExportTradeStatistic(DateTime startDate, DateTime endDate)
        {
            var platTradeStatistic = StatisticApplication.GetPlatTradeStatisticItems(startDate, endDate);
            ViewData.Model = platTradeStatistic;
            string Title = startDate.ToString("yyyy-MM-dd") + "至" + endDate.ToString("yyyy-MM-dd") + "交易统计数据";
            ViewData.Add("Title", Title);
            string viewHtml = RenderPartialViewToString(this, "ExportTradeStatistic");
            return File(System.Text.Encoding.UTF8.GetBytes(viewHtml), "application/ms-excel", "交易数据导出.xls");
        }

        public ActionResult ExportProductStatistic(DateTime startDate, DateTime endDate)
        {
            ProductStatisticQuery query = new ProductStatisticQuery
            {
                PageSize = int.MaxValue,
                PageNo = 1,
                StartDate = startDate,
                EndDate = endDate
            };
            var model = StatisticApplication.GetProductSales(query);

            ViewData.Model = model.Models;
            string Title = startDate.ToString("yyyy-MM-dd") + "至" + endDate.ToString("yyyy-MM-dd") + "商品统计数据";
            ViewData.Add("Title", Title);
            string viewHtml = RenderPartialViewToString(this, "ExportProductStatistic");
            return File(System.Text.Encoding.UTF8.GetBytes(viewHtml), "application/ms-excel", "商品销售情况.xls");

        }
        #endregion 导出
    }
}