using Mall.Web.Framework;
using System;
using System.Linq;

using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Core;
using Mall.CommonModel;
using Mall.Application;
using Mall.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class CashDepositController : BaseAdminController
    {
        ICashDepositsService _iCashDepositsService;
        public CashDepositController(ICashDepositsService iCashDepositsService)
        {
            this._iCashDepositsService = iCashDepositsService;

        }
        public ActionResult Management()
        {
            return View();
        }

        public ActionResult CashDepositDetail(long id)
        {
            ViewBag.Id = id;
            return View();
        }

        public ActionResult CashDepositRule()
        {
            var data = _iCashDepositsService.GetCategoryCashDeposits();
            var categories = CategoryApplication.GetCategories();
            ViewBag.Categories = categories;

            #region 老套餐可能漏掉了一级分类保证金，这里同步下
            var cIds = categories.Where(t => t.ParentCategoryId == 0).Select(t => t.Id);
            if(cIds!=null && cIds.Count() > 0)
            {
                var addIds = cIds.Except(data.Select(a => a.CategoryId));
                if(addIds != null && addIds.Count()>0)
                {
                    foreach (var value in addIds)
                    {
                        CategoryCashDepositInfo addm = new CategoryCashDepositInfo();
                        addm.CategoryId = value;
                        CashDepositsApplication.AddCategoryCashDeposits(addm);
                    }
                    Log.Error("保证金少了一级分类Id：" + string.Join(",", addIds));
                    data = _iCashDepositsService.GetCategoryCashDeposits();//说明之前漏掉了重新读取最新的
                }
            }
            #endregion

            ViewBag.Categories = categories;
            return View(data);
        }

        [HttpPost]
        public JsonResult List(CashDepositQuery query)
        {
            var data = CashDepositsApplication.GetCashDeposits(query);
            return Json(data, true);
        }

        [HttpPost]
        public JsonResult CashDepositDetailList(long id, string name, DateTime? startDate, DateTime? endDate, int page, int rows)
        {
            var queryModel = new CashDepositDetailQuery()
            {
                CashDepositId = id,
                Operator = name,
                StartDate = startDate,
                EndDate = endDate,
                PageNo = page,
                PageSize = rows
            };
            QueryPageModel<Entities.CashDepositDetailInfo> cashDepositDetail = _iCashDepositsService.GetCashDepositDetails(queryModel);
            var cashDepositDetailModel = cashDepositDetail.Models.ToArray().Select(item => new
            {
                Id = item.Id,
                Date = item.AddDate.ToString("yyyy-MM-dd HH:mm"),
                Balance = item.Balance,
                Operator = item.Operator,
                Description = item.Description
            });
            return Json(new { rows = cashDepositDetailModel, total = cashDepositDetail.Total });
        }

        public JsonResult Deduction(long id, string balance, string description)
        {
            if (Convert.ToDecimal(balance) < 0)
                throw new MallException("扣除保证金不能为负值");
            Entities.CashDepositDetailInfo model = new Entities.CashDepositDetailInfo()
            {
                AddDate = DateTime.Now,
                Balance = -Convert.ToDecimal(balance),
                CashDepositId = id,
                Description = description,
                Operator = CurrentManager.UserName
            };
            _iCashDepositsService.AddCashDepositDetails(model);
            return Json(new Result { success = true });
        }

        public JsonResult UpdateEnableLabels(long id, bool enableLabels)
        {
            _iCashDepositsService.UpdateEnableLabels(id, enableLabels);
            return Json(new Result { success = true });
        }

        public JsonResult OpenNoReasonReturn(long categoryId)
        {
            _iCashDepositsService.OpenNoReasonReturn(categoryId);
            return Json(new Result { success = true });
        }
        public JsonResult CloseNoReasonReturn(long categoryId)
        {
            _iCashDepositsService.CloseNoReasonReturn(categoryId);
            return Json(new Result { success = true });
        }

        public JsonResult UpdateNeedPayCashDeposit(long categoryId, decimal cashDeposit)
        {
            if (cashDeposit < 0)
            {
                return Json(new Result { success = false, msg="不可为负数！" });
            }
            _iCashDepositsService.UpdateNeedPayCashDeposit(categoryId, cashDeposit);
            return Json(new Result { success = true });
        }
    }
}