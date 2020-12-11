
using Mall.Web.Framework;
using System;

using Mall.DTO;
using Mall.Application;
using Mall.DTO.QueryModel;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class ShopAccountController : BaseSellerController
    {

        /// <summary>
        /// 充值页面
        /// </summary>
        /// <returns></returns>
        public ActionResult CapitalCharge()
        {
            Mall.DTO.ShopAccount model = BillingApplication.GetShopAccount(CurrentSellerManager.ShopId);
            return View(model);
        }

        /// <summary>
        /// 充值请求提交
        /// </summary>
        /// <param name="amount">充值金额</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ChargeSubmit(decimal amount)
        {
            string webRoot = CurrentUrlHelper.CurrentUrlNoPort();
            var model = BillingApplication.PaymentList(CurrentSellerManager.ShopId, amount, webRoot);
            return Json(model.models);
        }

        /// <summary>
        /// 提现申请页面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult ApplyWithDraw()
        {
            Entities.ShopInfo.ShopStage Stage = Entities.ShopInfo.ShopStage.Agreement;
            var shop = ShopApplication.GetShopProfileStep2(CurrentSellerManager.ShopId, out Stage);
            Mall.DTO.MemberAccountSafety mMemberAccountSafety = MemberApplication.GetMemberAccountSafety(MemberApplication.GetMemberUserIdOrShop(CurrentUser, CurrentSellerManager));

            ViewBag.MemberEmail = mMemberAccountSafety.Email;
            ViewBag.MemberPhone = mMemberAccountSafety.Phone;
            var siteSetting = SiteSettingApplication.SiteSettings;
            ViewBag.CanAlipay = siteSetting.Withdraw_AlipayEnable;
            ViewBag.CanWXpay = siteSetting.IsOpenH5;//微信提现需要绑定了微信支付才可以
            ViewBag.WithDrawMinimum = siteSetting.ShopWithDrawMinimum;
            ViewBag.WithDrawMaximum = siteSetting.ShopWithDrawMaximum;
            return View(shop);
        }

        /// <summary>
        /// 提现申请数据提交
        /// </summary>
        /// <returns></returns>
        public JsonResult ApplyWithDrawSubmit(string pluginId, string destination, string code, decimal amount, int WithdrawType, string Account, string AccountName)
        {
            long userId = MemberApplication.GetMemberUserIdOrShop(CurrentUser, CurrentSellerManager);
            int result = MemberApplication.CheckCode(pluginId, code, destination, userId);
            if (result > 0)
            {
                var sitesetting = SiteSettingApplication.SiteSettings;
                if (amount <= sitesetting.ShopWithDrawMinimum && amount >= sitesetting.ShopWithDrawMaximum)
                {
                    return Json(new { success = false, msg = "提现金额不能小于：" + sitesetting.ShopWithDrawMinimum + " 元,不能大于：" + sitesetting.ShopWithDrawMaximum + " 元" });
                }
                Mall.DTO.ShopWithDraw model = new ShopWithDraw()
                {
                    SellerId = userId,
                    SellerName = CurrentSellerManager.UserName,
                    ShopId = CurrentSellerManager.ShopId,
                    WithdrawalAmount = amount,
                    WithdrawType = (Mall.CommonModel.WithdrawType)WithdrawType,
                    Account = Account,
                    AccountName = AccountName
                };
                if (model.WithdrawType == CommonModel.WithdrawType.ALipay)
                {
                    if (string.IsNullOrEmpty(model.Account) || string.IsNullOrEmpty(model.AccountName))
                    {
                        return Json(new { success = false, msg = "支付宝提现请填写支付账号与真实姓名！" });
                    }
                }

                bool isbool = BillingApplication.ShopApplyWithDraw(model);

                if (isbool)
                    return Json(new { success = true, msg = "成功！" });
                else
                    return Json(new { success = false, msg = "余额不足，无法提现！" });
            }
            else
            {
                return Json(new { success = false, msg = "验证码错误！" });
            }
        }

        /// <summary>
        /// 提现记录列表
        /// </summary>
        /// <returns></returns>
        public ActionResult Management(long Id = 0)
        {
            ViewBag.Id = Id;
            return View();
        }

        /// <summary>
        /// 提现信息列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult List(DateTime? startDate, DateTime? endDate, DateTime? startDates, DateTime? endDates, long? WithdrawStaus, long? Id, int Status, int page, int rows)
        {
            WithdrawQuery query = new WithdrawQuery()
            {
                ApplyStartTime = startDate,
                ApplyEndTime = endDate,
                AuditedStartTime = startDates,
                AuditedEndTime = endDates,
                Id = Id,
                Status = (Mall.CommonModel.WithdrawStaus?)WithdrawStaus,
                ShopId = CurrentSellerManager.ShopId,
                PageSize = rows,
                PageNo = page
            };
            var model = BillingApplication.GetShopWithDraw(query);
            return Json(new { rows = model.Models, total = model.Total });
        }

        /// <summary>
        /// 提现信息列表
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportShopWithDraw(DateTime? applyStartTime, DateTime? applyEndTime, DateTime? auditedStartTime, DateTime? auditedEndTime, long? staus)
        {
            var query = new WithdrawQuery()
            {
                ApplyStartTime = applyStartTime,
                ApplyEndTime = applyEndTime,
                AuditedStartTime = auditedStartTime,
                AuditedEndTime = auditedEndTime,
                Status = (Mall.CommonModel.WithdrawStaus?)staus,
                ShopId = CurrentSellerManager.ShopId,
            };
            var models = BillingApplication.GetAllShopWithDraw(query);
            return ExcelView("提现明细", models);
        }
    }
}