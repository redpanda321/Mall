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
    public class DistributionWithdrawController : BaseAdminController
    {
        public ActionResult Management()
        {
            return View();
        }

        public ActionResult List(DistributorSubQuery query)
        {
            query.IncludeNoSettlementAmount = true;
            var data = DistributionApplication.GetDistributors(query);
            var list = data.Models.Select(item =>
            new
            {
                item.MemberId,
                MemberName = item.Member == null ? "" : item.Member.UserName,
                item.ShopName,
                SettlementAmount = item.SettlementAmount.ToString("n2"),
                NoSettlementAmount = item.NoSettlementAmount.ToString("n2"),
                Balance = item.Balance.ToString("n2"),
                FreezeAmount = item.FreezeAmount.ToString("n2"),
                WithdrawalsAmount = item.WithdrawalsAmount.ToString("n2")
            });
            return Json(new { rows = list, total = data.Total }, true);
        }

        public ActionResult Details(long id)
        {
            ViewBag.MemberId = id;
            return View();
        }

        public ActionResult WithdrawList(DistributionWithdrawQuery query)
        {
            var data = DistributionApplication.GetWithdraws(query);
            var list = data.Models.Select(p => new
            {
                p.Amount,
                Id = p.Id.ToString(),
                Time = p.ApplyTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ConfirmTime = p.ConfirmTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty,
                PayTime = p.PayTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty,
                p.PayNo,
                MemberName = p.Member.UserName,
                WithdrawType = p.WithdrawType.ToDescription(),
                Status = p.WithdrawStatus,
                p.WithdrawAccount,
                p.WithdrawName,
                WithdrawStatus = p.WithdrawStatus.ToDescription(),
                p.Remark
            });
            return Json(new { rows = list, total = data.Total }, true);
        }

        public ActionResult WeChat() {
            return View();
        }
        public ActionResult Alipay() {
            return View();
        }

        public ActionResult Confirm(long id=0, List<long> batchs= null, string remark="")
        {
            if (id > 0)
                DistributionApplication.AuditingWithdraw(id, CurrentManager.UserName, remark);
            else if (batchs != null)
            {
                var error = 0;
                foreach (var item in batchs)
                {
                    try
                    {
                        DistributionApplication.AuditingWithdraw(item, CurrentManager.UserName, remark);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("审核佣金提现失败:", ex);
                        error++;
                    }
                }
                if (error == 0)
                {

                }
                else
                {
                    return Json(new { success = false, msg = "审核操作完成，但部分提现失败，请检查！" });
                }
            }
            else
                return Json(new { success = false, msg = "无效参数" });
            return Json(new { success = true }, true);
        }
        public ActionResult Refused(long id = 0, List<long> batchs = null, string remark = "")
        {
            if(id>0)
                DistributionApplication.RefusedWithdraw(id, CurrentManager.UserName, remark);
            else if(batchs!=null)
                foreach (var item in batchs)
                    DistributionApplication.RefusedWithdraw(item, CurrentManager.UserName, remark);
            else
                return Json(new { success = false, msg = "无效参数" });
            return Json(new { success = true }, true);
        }
        public ActionResult Settings()
        {
            var settings = new DistributionWithdrawSettings
            {
                MinLimit = SiteSettings.DistributorWithdrawMinLimit,
                MaxLimit = SiteSettings.DistributorWithdrawMaxLimit,
                Types = SiteSettings.DistributorWithdrawTypes,
            };
            return View(settings);
        }
        [HttpPost]
        public ActionResult Settings(DistributionWithdrawSettings settings)
        {
            SiteSettingApplication.SiteSettings.DistributorWithdrawMinLimit = settings.MinLimit;
            SiteSettingApplication.SiteSettings.DistributorWithdrawMaxLimit = settings.MaxLimit;
            SiteSettingApplication.SiteSettings.DistributorWithdrawTypes = settings.Types;
            SiteSettingApplication.SaveChanges();
            return Json(new { success = true }, true);
        }

        /// <summary>
        /// 导出佣金管理
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ActionResult ManagementExportToExcel(DistributorSubQuery query)
        {
            query.IncludeNoSettlementAmount = true;
            var result = DistributionApplication.GetDistributorsAll(query);

            return ExcelView("ManagementExport", "平台佣金管理", result);
        }

        /// <summary>
        /// 导出佣金提现记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ActionResult DetailsExportToExcel(DistributionWithdrawQuery query)
        {
            var result = DistributionApplication.GetWithdrawsAll(query);

            return ExcelView("DetailsExport", "平台佣金提现记录", result);
        }
    }
}
