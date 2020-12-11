using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Web.Areas.Admin.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class AccountController : BaseAdminController
    {

        public ActionResult Management()
        {
            return View();
        }

        [UnAuthorize]
        public ActionResult Detail(long id)
        {
            Entities.AccountInfo account = AccountApplication.GetAccount(id);

            return View(account);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult List(int status, string shopName, int page, int rows)
        {
            var queryModel = new AccountQuery()
            {
                Status = (Entities.AccountInfo.AccountStatus?)status,
                ShopName = shopName,
                PageSize = rows,
                PageNo = page
            };

            var accounts = AccountApplication.GetAccounts(queryModel);
            var models = new List<AccountModel>();
            foreach (var item in accounts.Models)
            {
                AccountModel model = new AccountModel();
                model.Id = item.Id;
                model.ShopId = item.ShopId;
                model.ShopName = item.ShopName;
                model.AccountDate = item.AccountDate.ToString();
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


        public FileResult ExportExcel(int status, string shopName)
        {
            var queryModel = new AccountQuery()
            {
                Status = (Entities.AccountInfo.AccountStatus?)status,
                ShopName = shopName,
                PageSize = int.MaxValue,
                PageNo = 1
            };

            var accounts = AccountApplication.GetAccounts(queryModel);
            var models = new List<AccountModel>();
            foreach (var item in accounts.Models.ToArray())
            {
                AccountModel model = new AccountModel();
                model.Id = item.Id;
                model.ShopId = item.ShopId;
                model.ShopName = item.ShopName;
                model.AccountDate = item.AccountDate.ToString();
                model.StartDate = item.StartDate;
                model.EndDate = item.EndDate;
                model.Status = (int)item.Status;
                model.ProductActualPaidAmount = item.ProductActualPaidAmount;
                model.FreightAmount = item.FreightAmount;
                model.CommissionAmount = item.CommissionAmount;
                model.RefundAmount = item.RefundAmount;
                model.RefundCommissionAmount = item.RefundCommissionAmount;
                model.BrokerageAmount = item.Brokerage;
                model.ReturnBrokerageAmount = item.ReturnBrokerage;
                model.AdvancePaymentAmount = item.AdvancePaymentAmount;
                model.PeriodSettlement = item.PeriodSettlement;
                model.Remark = item.Remark;
                model.TimeSlot = string.Format("{0} 至 {1}", model.StartDate.Date.ToString("yyyy-MM-dd"), model.EndDate.Date.ToString("yyyy-MM-dd"));
                models.Add(model);

            }

            //创建Excel文件的对象
            NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            //添加一个sheet
            ISheet sheet1 = book.CreateSheet("Sheet1");
            //获取list数据

            //给sheet1添加第一行的头部标题

            var thead = sheet1.CreateRow(0);
            thead.CreateCell(0).SetCellValue("店铺名称");
            thead.CreateCell(1).SetCellValue("时间段");
            thead.CreateCell(2).SetCellValue("商品实付总额");
            thead.CreateCell(3).SetCellValue("运费");
            thead.CreateCell(4).SetCellValue("佣金");
            thead.CreateCell(5).SetCellValue("退款金额");
            thead.CreateCell(6).SetCellValue("退还佣金");
            thead.CreateCell(7).SetCellValue("分销佣金");
            thead.CreateCell(8).SetCellValue("退还分销佣金");
            thead.CreateCell(9).SetCellValue("营销费用总额");
            thead.CreateCell(10).SetCellValue("本期应结");
            thead.CreateCell(11).SetCellValue("出账日期");
            //将数据逐步写入sheet1各个行
            for (int i = 0; i < models.Count; i++)
            {
                var row = sheet1.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(models[i].ShopName);
                row.CreateCell(1).SetCellValue(models[i].TimeSlot);
                row.CreateCell(2).SetCellValue(models[i].ProductActualPaidAmount.ToString());
                row.CreateCell(3).SetCellValue(models[i].FreightAmount.ToString());
                row.CreateCell(4).SetCellValue(models[i].CommissionAmount.ToString());
                row.CreateCell(5).SetCellValue(models[i].RefundAmount.ToString());
                row.CreateCell(6).SetCellValue(models[i].RefundCommissionAmount.ToString());
                row.CreateCell(7).SetCellValue(models[i].BrokerageAmount.ToString());
                row.CreateCell(8).SetCellValue(models[i].ReturnBrokerageAmount.ToString());
                row.CreateCell(9).SetCellValue(models[i].AdvancePaymentAmount.ToString());
                row.CreateCell(10).SetCellValue(models[i].PeriodSettlement.ToString());
                row.CreateCell(11).SetCellValue(models[i].AccountDate.ToString());
            }

            // 写入到客户端 
            var ms = new System.IO.MemoryStream();
            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/vnd.ms-excel", "结算管理.xls");


        }

        [OperationLog(Message = "确认结算")]
        [UnAuthorize]
        [HttpPost]
        public JsonResult ConfirmAccount(long id, string remark)
        {
            Result result = new Result();
            try
            {
                AccountApplication.ConfirmAccount(id, remark);
                result.success = true;
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
            }
            return Json(result);
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
                PageNo = page
            };
            var accountDetails = AccountApplication.GetAccountDetails(queryModel);
            var result = (from p in accountDetails.Models
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
                                           p.BrokerageAmount,
                                           p.ReturnBrokerageAmount,
                                           p.RefundTotalAmount,
                                           Date = p.Date.ToString(),
                                           OrderDate = p.OrderDate.ToString(),
                                           p.OrderRefundsDates
                                       }).ToList();
            return Json(new { rows = result, total = accountDetails.Total });
        }



        public FileResult DetailListExportExcel(long accountId, int enumOrderTypeId, DateTime? startDate, DateTime? endDate)
        {
            var queryModel = new AccountQuery()
            {
                StartDate = startDate,
                EndDate = endDate.HasValue ? endDate.Value.AddDays(1) : endDate,
                AccountId = accountId,
                PageSize = int.MaxValue,
                EnumOrderType = (Entities.AccountDetailInfo.EnumOrderType)enumOrderTypeId,
                PageNo = 1
            };
            var accountDetails = AccountApplication.GetAccountDetails(queryModel);

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
                                       }).ToList();
            //创建Excel文件的对象
            NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            //添加一个sheet
            ISheet sheet1 = book.CreateSheet("Sheet1");
            //获取list数据

            //给sheet1添加第一行的头部标题
            string titleFlag = string.Empty;
           IRow row1 = sheet1.CreateRow(0);
            if (enumOrderTypeId == 1)
            {
                titleFlag = "订单列表";
                row1.CreateCell(0).SetCellValue("类型");
                row1.CreateCell(1).SetCellValue("订单编号");
                row1.CreateCell(2).SetCellValue("商品实付金额");
                row1.CreateCell(3).SetCellValue("运费");
                row1.CreateCell(4).SetCellValue("佣金");
                row1.CreateCell(5).SetCellValue("下单日期");
                row1.CreateCell(6).SetCellValue("成交日期");
                sheet1.SetColumnWidth(0, 550 * 5);
                sheet1.SetColumnWidth(1, 550 * 20);
                sheet1.SetColumnWidth(2, 550 * 8);
                sheet1.SetColumnWidth(3, 550 * 5);
                sheet1.SetColumnWidth(4, 550 * 5);
                sheet1.SetColumnWidth(5, 550 * 15);
                sheet1.SetColumnWidth(6, 550 * 15);
            }
            else if (enumOrderTypeId == 0)
            {
                titleFlag = "退单列表";
                row1.CreateCell(0).SetCellValue("类型");
                row1.CreateCell(1).SetCellValue("订单编号");
                row1.CreateCell(2).SetCellValue("商品实付金额");
                row1.CreateCell(3).SetCellValue("运费");
                row1.CreateCell(4).SetCellValue("退款金额");
                row1.CreateCell(5).SetCellValue("退还佣金");
                row1.CreateCell(6).SetCellValue("退单日期");
                sheet1.SetColumnWidth(0, 550 * 5);
                sheet1.SetColumnWidth(1, 550 * 20);
                sheet1.SetColumnWidth(2, 550 * 8);
                sheet1.SetColumnWidth(3, 550 * 5);
                sheet1.SetColumnWidth(4, 550 * 8);
                sheet1.SetColumnWidth(5, 550 * 5);
                sheet1.SetColumnWidth(6, 550 * 15);
            }

            //将数据逐步写入sheet1各个行
            for (int i = 0; i < accountDetailsModel.Count(); i++)
            {
               IRow rowtemp = sheet1.CreateRow(i + 1);
                if (enumOrderTypeId == 1)
                {
                    rowtemp.CreateCell(0).SetCellValue(titleFlag);
                    rowtemp.CreateCell(1).SetCellValue(accountDetailsModel[i].OrderId.ToString());
                    rowtemp.CreateCell(2).SetCellValue(accountDetailsModel[i].ProductActualPaidAmount.ToString());
                    rowtemp.CreateCell(3).SetCellValue(accountDetailsModel[i].FreightAmount.ToString());
                    rowtemp.CreateCell(4).SetCellValue(accountDetailsModel[i].CommissionAmount.ToString());
                    rowtemp.CreateCell(5).SetCellValue(accountDetailsModel[i].OrderDate.ToString());
                    rowtemp.CreateCell(6).SetCellValue(accountDetailsModel[i].Date.ToString());
                }
                else if (enumOrderTypeId == 0)
                {
                    rowtemp.CreateCell(0).SetCellValue(titleFlag);
                    rowtemp.CreateCell(1).SetCellValue(accountDetailsModel[i].OrderId.ToString());
                    rowtemp.CreateCell(2).SetCellValue(accountDetailsModel[i].ProductActualPaidAmount.ToString());
                    rowtemp.CreateCell(3).SetCellValue(accountDetailsModel[i].FreightAmount.ToString());
                    rowtemp.CreateCell(4).SetCellValue(accountDetailsModel[i].RefundTotalAmount.ToString());
                    rowtemp.CreateCell(5).SetCellValue(accountDetailsModel[i].RefundCommisAmount.ToString());
                    rowtemp.CreateCell(6).SetCellValue(accountDetailsModel[i].OrderRefundsDates.ToString());
                }

            }

            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/vnd.ms-excel", string.Format("结算详情-{0}.xls", titleFlag));
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


        [HttpPost]
        [UnAuthorize]
        public JsonResult ExecutSettlement()
        {
            //Mall.Service.Job.AccountJob job = new Service.Job.AccountJob();
            //job.Execute(null);
            //Mall.Service.Job.OrderCommentsJob commentjob = new Service.Job.OrderCommentsJob();
            //commentjob.Execute(null);
            return Json(new { success = true });
        }

        public FileResult AgreementDetailListExportExcel(long accountId, int enumOrderTypeId, DateTime? startDate, DateTime? endDate)
        {
            var queryModel = new AccountQuery()
            {
                StartDate = startDate,
                EndDate = endDate.HasValue ? endDate.Value.AddDays(1) : endDate,
                AccountId = accountId,
                PageSize = int.MaxValue,
                PageNo = 1
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
            }).ToList();
       
            //创建Excel文件的对象
            NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            //添加一个sheet
            ISheet sheet1 = book.CreateSheet("Sheet1");
            //获取list数据

            //给sheet1添加第一行的头部标题

           IRow row1 = sheet1.CreateRow(0);

            row1.CreateCell(0).SetCellValue("类型");
            row1.CreateCell(1).SetCellValue("营销类型");
            row1.CreateCell(2).SetCellValue("费用");
            row1.CreateCell(3).SetCellValue("服务周期");
            sheet1.SetColumnWidth(0, 550 * 5);
            sheet1.SetColumnWidth(1, 550 * 20);
            sheet1.SetColumnWidth(2, 550 * 8);
            sheet1.SetColumnWidth(3, 550 * 15);

            //将数据逐步写入sheet1各个行
            for (int i = 0; i < mode.Count(); i++)
            {
                IRow rowtemp = sheet1.CreateRow(i + 1);
                rowtemp.CreateCell(0).SetCellValue("营销服务费");
                rowtemp.CreateCell(1).SetCellValue(mode[i].MetaKey);
                rowtemp.CreateCell(2).SetCellValue(mode[i].MetaValue);
                rowtemp.CreateCell(3).SetCellValue(mode[i].DateRange);
            }

            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/vnd.ms-excel", "结算详情-营销服务费列表.xls");
        }

        public ActionResult SetSettlementWeek()
        {
            WeekSettlementModel model = new WeekSettlementModel();
            var settings = SiteSettingApplication.SiteSettings;
            model.CurrentWeekSettlement = settings.WeekSettlement;
            return View(model);
        }

        [HttpPost]
        public ActionResult UpdateSettlementWeek(WeekSettlementModel weekSettlementModel)
        {
            var setting = SiteSettingApplication.SiteSettings;
            setting.WeekSettlement = weekSettlementModel.NewWeekSettlement;
            SiteSettingApplication.SaveChanges();
            return RedirectToAction("SetSettlementWeek");
        }
    }
}