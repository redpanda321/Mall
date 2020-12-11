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
using Mall.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    //[H5Authorization]
    /// <summary>
    /// 销售员管理
    /// </summary>
    public class DistributionController : BaseAdminController
    {
        #region 业绩排行
        public ActionResult Rankings()
        {
            var batch = DistributionApplication.GetLastRankingBatch();
            return View(batch);
        }
        [HttpPost]
        public ActionResult Rankings(DistributorRankingQuery query)
        {
            if (query.BatchId == 0)//必填参数
                return Json(new { rows = new List<object>(), total = 0 });

            var data = DistributionApplication.GetRankings(query);
            var list = data.Models.Select(item =>
            new
            {
                item.Rank,
                DistributorId=item.Distributor.MemberId,
                DistributorName = item.Member.UserName,
                item.Distributor.ShopName,
                item.Grade?.GradeName,
                item.Quantity,
                item.Amount,
                item.Settlement,
                item.NoSettlement
            });
            return Json(new { rows = list, total = data.Total }, true);
        }
        [HttpPost]
        public ActionResult GenerateRanking(DateTime begin, DateTime end)
        {
            DistributionApplication.GenerateRankingAsync(begin, end);
            return Json(new { success = true }, true);
        }
        public ActionResult CheckGenerate()
        {
            //是否完成生成任务
            var completed = !DistributionApplication.CheckGenerating();
            return Json(new { success = completed }, true);
        }

        /// <summary>
        /// 导出分销订单业绩
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ActionResult RankingsExportToExcel(DistributorRankingQuery query)
        {
            var orderResults = DistributionApplication.GetRankingsAll(query);

            return ExcelView("RankingsExport", "平台销售员排行", orderResults);
        }

        #endregion

        #region 分销订单明细
        [HttpGet]
        public ActionResult Orders(long distributor = 0, long? productId = null)
        {
            ViewBag.MaxLevel = SiteSettingApplication.SiteSettings.DistributionMaxLevel;
            ViewBag.Distributor = distributor;
            ViewBag.ProductId = productId;
            return View();
        }
        [HttpPost]
        public ActionResult Orders(BrokerageOrderQuery query)
        {
            if (query.SettlementEnd.HasValue)
                query.SettlementEnd = query.SettlementEnd.Value.AddDays(1);
            var data = DistributionApplication.GetBrokerageOrders(query);
            foreach(var item in data.Models)
            {
                item.Brokerage1 = decimal.Parse(item.Brokerage1.ToString("F2"));
                item.Brokerage2 = decimal.Parse(item.Brokerage2.ToString("F2"));
                item.Brokerage3 = decimal.Parse(item.Brokerage3.ToString("F2"));
            }
            return Json(data, true);
        }

        /// <summary>
        /// 导出分销订单业绩
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ActionResult OrdersExportToExcel(BrokerageOrderQuery query)
        {
            if (query.SettlementEnd.HasValue)
                query.SettlementEnd = query.SettlementEnd.Value.AddDays(1);
            var orderResults = DistributionApplication.GetBrokerageOrdersAll(query, true, true);

            return ExcelView("OrdersExport", "平台分销订单明细", orderResults);
        }
        #endregion
    }
}