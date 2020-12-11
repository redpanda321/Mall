using System;

using System.Linq;
using Mall.Web.Framework;
using Mall.DTO.QueryModel;
using Mall.Application;
using Mall.CommonModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class ShopWithDrawController : BaseAdminController
    {
        public ActionResult Management(int status = 1)
        {
            ViewBag.Status = status;
            return View();
        }

        /// <summary>
        /// 提现信息列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult List(WithdrawQuery query)
        {
            var model = BillingApplication.GetShopWithDraw(query);
            return Json(new { rows = model.Models, total = model.Total });
        }

        /// <summary>
        /// 审核操作
        /// </summary>
        /// <param name="id">申请ID</param>
        /// <param name="status">状态</param>
        /// <param name="remark">备注</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ConfirmPay(long id, int status, string remark)
        {
            var result = BillingApplication.ShopApplyWithDraw(id, (Mall.CommonModel.WithdrawStaus)status, remark, Request.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress.ToString(), CurrentManager.UserName, webRoot: CurrentUrlHelper.CurrentUrlNoPort());
            if (result.Success)
            {
                return Json(new { success = true, msg = "成功！", jumpurl = result.JumpUrl, status = !string.IsNullOrWhiteSpace(result.JumpUrl) ? 2 : 0 });
            }
            else
            {
                return Json(new { success = false, msg = "操作失败，可能微信证书设置错误或者扣款账号余额不足，检查完稍后再试。" });
            }
        }
        public JsonResult BatchConfirmApply(string ids, int status, string remark)
        {
            var result = BillingApplication.ShopApplyWithDrawBatch(ids, (Mall.CommonModel.WithdrawStaus)status, remark, Request.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress.ToString(), CurrentManager.UserName, webRoot: CurrentUrlHelper.CurrentUrlNoPort());
            var isHaveError = result.Where(e => e.Success == false).Count() > 0;
            if (isHaveError)
            {
                return Json(new { success = true, msg = "审核完成，因批量操作存在异常，请检查！", status =  0 });
            }
            else
            {
                return Json(new { success = true, msg = "审核完成！", status = 0 });
                //return Json(new { success = false, msg = "操作失败，可能微信证书设置错误或者扣款账号余额不足，检查完稍后再试。" });
            }
        }
        /// <summary>
        /// 取消提现
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult CancelPay(long Id)
        {
            string Msg = "ok";
            var b = true;
            if (Id > 0)
            {
                b = BillingApplication.CancelShopApplyWithDraw(Id);
                if (!b)
                    Msg = "取消失败";
            }
            else
            {
                b = false;
                Msg = "数据错误";
            }
            return Json(new Result() { success = b, msg = Msg });
        }

        /// <summary>
        /// 商家提现设置
        /// </summary>
        /// <returns></returns>
        public ActionResult Setting()
        {
            return View(SiteSettingApplication.SiteSettings);
        }

        /// <summary>
        /// 保存商家体现设置
        /// </summary>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <param name="alipayEnable"></param>
        /// <returns></returns>
        public JsonResult SaveWithDrawSetting(int minimum, int maximum)
        {
            if (minimum < 1 && minimum > maximum && maximum > 1000000)
                return Json(new Result() { success = false, msg = "金额范围只能是(1-1000000)" });
            SiteSettingApplication.SiteSettings.ShopWithDrawMaximum = maximum;
            SiteSettingApplication.SiteSettings.ShopWithDrawMinimum = minimum;
            SiteSettingApplication.SaveChanges();
            return Json(new Result() { success = true, msg = "保存成功" });
        }

        /// <summary>
        /// 导出分销订单业绩
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ActionResult ExportToExcelShopWithDraw(WithdrawQuery query)
        {
            var orderResults = BillingApplication.GetAllShopWithDraw(query);

            return ExcelView("ExportShopWithDraw", "平台商家提现成功记录", orderResults);
        }
    }
}