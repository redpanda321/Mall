using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Plugins;
using Mall.DTO;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Areas.Admin.Models;
using Mall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class AccountController : BaseSellerController
    {
        
        public ActionResult Management()
        {
            return View();
        }

        public ActionResult AccountDetail(long id)
        {
            Entities.AccountInfo account = AccountApplication.GetAccount(id);
            if (account.ShopId != CurrentSellerManager.ShopId)
            {
                throw new MallException("不存在该结算信息" + id);
            }
            return View(account);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult List(int status, int page, int rows)
        {
            var queryModel = new AccountQuery()
            {
                Status = (Entities.AccountInfo.AccountStatus?)status,
                PageSize = rows,
                PageNo = page,
                ShopId = CurrentSellerManager.ShopId
            };

            QueryPageModel<Entities.AccountInfo> accounts = AccountApplication.GetAccounts(queryModel);
            IList<AccountModel> models = new List<AccountModel>();
            foreach (var item in accounts.Models.ToArray())
            {
                AccountModel model = new AccountModel();
                model.Id = item.Id;
                model.ShopId = item.ShopId;
                model.ShopName = item.ShopName;
                model.AccountDate = item.AccountDate.ToLocalTime().ToString();
                model.StartDate = item.StartDate;
                model.EndDate = item.EndDate;
                model.Status = (int)item.Status;
                model.ProductActualPaidAmount = item.ProductActualPaidAmount;
                model.FreightAmount = item.FreightAmount;
                model.CommissionAmount = item.CommissionAmount;
                model.RefundAmount = item.RefundAmount;
                model.RefundCommissionAmount = item.RefundCommissionAmount;
                model.AdvancePaymentAmount = item.AdvancePaymentAmount;
                model.PeriodSettlement = item.PeriodSettlement;
                model.Remark = item.Remark;
                model.BrokerageAmount = item.Brokerage;
                model.ReturnBrokerageAmount = item.ReturnBrokerage;
                model.TimeSlot = string.Format("{0} 至 {1}", model.StartDate.Date.ToString("yyyy-MM-dd"), model.EndDate.Date.ToString("yyyy-MM-dd"));
                models.Add(model);

            }
            return Json(new { rows = models, total = accounts.Total });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult DetailList(long accountId, int enumOrderTypeId, DateTime? startDate, DateTime? endDate, int page, int rows)
        {
            var queryModel = new AccountQuery()
            {
                StartDate = startDate,
                EndDate = endDate.HasValue ? endDate.Value.AddDays(1) : endDate,
                AccountId = accountId,
                PageSize = rows,
                EnumOrderType = (Entities.AccountDetailInfo.EnumOrderType)enumOrderTypeId,
                PageNo = page,
                ShopId = CurrentSellerManager.ShopId
            };
            QueryPageModel<Entities.AccountDetailInfo> accountDetails = AccountApplication.GetAccountDetails(queryModel);

            var accountDetailsModel = (from p in accountDetails.Models.ToList()
                                       select new
                                       {
                                           p.Id,
                                           p.OrderType,
                                           OrderTypeDescription = p.OrderType.ToDescription(),
                                           p.OrderId,
                                           p.ProductActualPaidAmount,
                                           p.FreightAmount,
                                           p.CommissionAmount,
                                           p.RefundCommisAmount,
                                           p.RefundTotalAmount,
                                           Date = p.Date.ToString(),
                                           OrderDate = p.OrderDate.ToString(),
                                           p.OrderRefundsDates
                                       });
            return Json(new { rows = accountDetailsModel, total = accountDetails.Total });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult MetaDetailList(long accountId, int enumOrderTypeId, DateTime? startDate, DateTime? endDate, int page, int rows)
        {
            var queryModel = new AccountQuery()
            {
                StartDate = startDate,
                EndDate = endDate.HasValue ? endDate.Value.AddDays(1) : endDate,
                AccountId = accountId,
                PageSize = rows,
                PageNo = page
            };
            QueryPageModel<DTO.AccountMetaModel> pageModelMetaInfo = AccountApplication.GetAccountMeta(queryModel);
            var mode = pageModelMetaInfo.Models.ToList().Select(e => new DTO.AccountMetaModel
            {
                AccountId = e.Id,
                Id = e.Id,
                EndDate = e.EndDate,
                StartDate = e.StartDate,
                MetaKey = e.MetaKey,
                MetaValue = e.MetaValue,
                DateRange = e.StartDate.ToString("yyyy-MM-dd") + " 至 " + e.EndDate.ToString("yyyy-MM-dd")
            });
            return Json(new { rows = mode, total = pageModelMetaInfo.Total });
        }
        
    }
}