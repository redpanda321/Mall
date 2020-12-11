using Mall.Application;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.Entities;
using Mall.Web.Framework;
using Mall.Web.Models;
using System;
using System.Linq;

using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class OrderRefundController : BaseSellerController
    {
        private IRefundService _iRefundService;

        public OrderRefundController(IRefundService iRefundService)
        {
            _iRefundService = iRefundService;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="showtype">0 所有 1 订单退款 2 仅退款(包含订单退款) 3 退货 4 仅退款</param>
        /// <returns></returns>
        public ActionResult Management(int showtype = 0)
        {
            ViewBag.ShowType = showtype;
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
            #endregion
            ViewBag.ShopName = this.CurrentShop.ShopName;
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="orderId"></param>
        /// <param name="auditStatus"></param>
        /// <param name="userName"></param>
        /// <param name="ProductName"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="showtype">0 所有 1 订单退款 2 仅退款(包含订单退款) 3 退货 4 仅退款</param>
        /// <returns></returns>
		[HttpPost]
        public JsonResult List(DateTime? startDate, DateTime? endDate, long? orderId, int? auditStatus, long? shopBranchId, string userName, string ProductName, int page, int rows, int showtype = 0)
        {
            var queryModel = new RefundQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                OrderId = orderId,
                ProductName = ProductName,
                AuditStatus = (Entities.OrderRefundInfo.OrderRefundAuditStatus?)auditStatus,
                ShopId = CurrentSellerManager.ShopId,
                UserName = userName,
                PageSize = rows,
                PageNo = page,
                ShowRefundType = showtype
            };
            if (shopBranchId.HasValue && shopBranchId.Value >= 0) queryModel.ShopBranchId = shopBranchId.Value;

            var refunds = Application.RefundApplication.GetOrderRefunds(queryModel);
            var orders = Application.OrderApplication.GetOrders(refunds.Models.Select(p => p.OrderId));
            var orderItems = Application.OrderApplication.GetOrderItems(refunds.Models.Select(p => p.OrderItemId));
            var refundModels = refunds.Models.Select(item =>
            {
                //以下查询代码要做优化
                var order = orders.FirstOrDefault(p => p.Id == item.OrderId);
                var orderItem = orderItems.FirstOrDefault(p => p.Id == item.OrderItemId);
                string spec = ((string.IsNullOrWhiteSpace(orderItem.Color) ? "" : orderItem.Color + "，")
                                + (string.IsNullOrWhiteSpace(orderItem.Size) ? "" : orderItem.Size + "，")
                                + (string.IsNullOrWhiteSpace(orderItem.Version) ? "" : orderItem.Version + "，")).TrimEnd('，');
                if (!string.IsNullOrWhiteSpace(spec))
                {
                    spec = "  【" + spec + " 】";
                }
                string shopBranchName = order.ShopName;
                if ( order.ShopBranchId > 0)
                {
                    var shopBranchInfo = ShopBranchApplication.GetShopBranchById(order.ShopBranchId);
                    if (shopBranchInfo != null)
                    {
                        shopBranchName = shopBranchInfo.ShopBranchName;
                    }
                }
                var verificationCodeIds = new List<string>();
                if (!string.IsNullOrWhiteSpace(item.VerificationCodeIds))
                {
                    verificationCodeIds = item.VerificationCodeIds.Split(',').ToList();
                    verificationCodeIds = verificationCodeIds.Select(a => a = Regex.Replace(a, @"(\d{4})", "$1 ")).ToList();
                }
                return new OrderRefundModel()
                {
                    RefundId = item.Id,
                    OrderId = item.OrderId,
                    AuditStatus = (order.DeliveryType == CommonModel.DeliveryType.SelfTake ||  order.ShopBranchId > 0) ? ((CommonModel.Enum.OrderRefundShopAuditStatus)(int)item.SellerAuditStatus).ToDescription() : item.SellerAuditStatus.ToDescription(),
                    ConfirmStatus = item.ManagerConfirmStatus.ToDescription(),
                    ApplyDate = item.ApplyDate.ToShortDateString(),
                    ShopId = item.ShopId,
                    ShopName = item.ShopName.Replace("'", "‘").Replace("\"", "”"),
                    UserId = item.UserId,
                    UserName = item.Applicant,
                    ContactPerson = HTMLEncode(item.ContactPerson),
                    ContactCellPhone = HTMLEncode(item.ContactCellPhone),
                    RefundAccount = string.IsNullOrEmpty(item.RefundAccount) ? string.Empty : HTMLEncode(item.RefundAccount.Replace("'", "‘").Replace("\"", "”")),
                    Amount = item.Amount.ToString("F2"),
                    ReturnQuantity = ((item.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                                    ? 0
                                    : (item.ShowReturnQuantity == 0 ? orderItem.ReturnQuantity : item.ShowReturnQuantity)),
                    Quantity = orderItem.Quantity,
                    SalePrice = item.EnabledRefundAmount.ToString("F2"),
                    ProductName = ((item.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund && order.OrderType != OrderInfo.OrderTypes.Virtual)
                                    ? "订单所有商品"
                                    : (orderItem.ProductName + spec)),
                    Reason = string.IsNullOrEmpty(item.Reason) ? string.Empty : HTMLEncode(item.Reason.Replace("'", "‘").Replace("\"", "”")),
                    ReasonDetail = string.IsNullOrEmpty(item.ReasonDetail) ? string.Empty : item.ReasonDetail.Replace("'", "‘").Replace("\"", "”"),
                    ExpressCompanyName = HTMLEncode(item.ExpressCompanyName),
                    ShipOrderNumber = item.ShipOrderNumber,
                    Payee = string.IsNullOrEmpty(item.Payee) ? string.Empty : HTMLEncode(item.Payee),
                    PayeeAccount = string.IsNullOrEmpty(item.PayeeAccount) ? string.Empty : HTMLEncode(item.PayeeAccount.Replace("'", "‘").Replace("\"", "”")),
                    RefundMode = (int)item.RefundMode,
                    SellerRemark = string.IsNullOrEmpty(item.SellerRemark) ? string.Empty : HTMLEncode(item.SellerRemark.Replace("'", "‘").Replace("\"", "”")),
                    ManagerRemark = string.IsNullOrEmpty(item.ManagerRemark) ? string.Empty : HTMLEncode(item.ManagerRemark.Replace("'", "‘").Replace("\"", "”")),
                    RefundStatus = ((item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited)
                                    ? item.ManagerConfirmStatus.ToDescription()
                                    : ((order.DeliveryType == CommonModel.DeliveryType.SelfTake ||  order.ShopBranchId > 0) ? ((CommonModel.Enum.OrderRefundShopAuditStatus)item.SellerAuditStatus).ToDescription() : item.SellerAuditStatus.ToDescription())),
                    RefundPayType = item.RefundPayType == null ? "线下处理" : item.RefundPayType.ToDescription(),
                    ApplyNumber = (item.ApplyNumber.HasValue ? item.ApplyNumber.Value : 1),
                    nextSecond = GetNextSecond(item),
                    CertPic1 = Core.MallIO.GetImagePath(item.CertPic1),
                    CertPic2 = Core.MallIO.GetImagePath(item.CertPic2),
                    CertPic3 = Core.MallIO.GetImagePath(item.CertPic3),
                    ShopBranchId = order.ShopBranchId,
                    ShopBranchName = shopBranchName,
                    IsVirtual = item.IsVirtual,
                    VerificationCodeIds = string.Join(",", verificationCodeIds),
                    RefundBatchNo = item.RefundBatchNo,
                    RefundPayStatus = (int)item.RefundPayStatus
                };
            });

            var dataGrid = new DataGridModel<OrderRefundModel>()
            {
                rows = refundModels,
                total = refunds.Total
            };
            return Json(dataGrid);
        }

        private double GetNextSecond(DTO.OrderRefund data)
        {
            double result = -999;
            var siteSetting = SiteSettingApplication.SiteSettings;
            if (data != null)
            {
                if (data.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit ||
                    data.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery ||
                    data.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving
                    )
                {
                    int num = 0;
                    DateTime _time = DateTime.Now;
                    switch (data.SellerAuditStatus)
                    {
                        case OrderRefundInfo.OrderRefundAuditStatus.WaitAudit:
                            _time = data.ApplyDate;
                            num = siteSetting.AS_ShopConfirmTimeout;
                            break;
                        case OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery:
                            _time = data.SellerAuditDate;
                            num = siteSetting.AS_SendGoodsCloseTimeout;
                            break;
                        case OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving:
                            _time = data.BuyerDeliverDate.GetValueOrDefault();
                            num = siteSetting.AS_ShopNoReceivingTimeout;
                            break;
                    }
                    TimeSpan ts = (DateTime.Now - _time);
                    if (num > 0)
                    {
                        result = num * 24 * 60 * 60;
                        result = result - ts.TotalSeconds;
                        if (result < 0)
                        {
                            result = -1;
                        }
                    }
                }
            }
            return result;
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult DealRefund(long refundId, int auditStatus, string sellerRemark)
        {
            Result result = new Result();
            var refundauditstatus = (Entities.OrderRefundInfo.OrderRefundAuditStatus)auditStatus;
            switch (refundauditstatus)
            {
                case Entities.OrderRefundInfo.OrderRefundAuditStatus.UnAudit:
                    if (string.IsNullOrWhiteSpace(sellerRemark))
                    {
                        throw new MallException("请填写拒绝理由");
                    }
                    break;
            }
            var refund = RefundApplication.GetOrderRefund(refundId);
            if (refund != null && refund.IsReturn == true && auditStatus == 2)
            {
                var shopget = ShopShippersApplication.GetDefaultGetGoodsShipper(refund.ShopId);
                if (shopget == null)
                {
                    throw new MallException("需要先设置好退货地址再进行操作，立即前往设置！");
                }
            }
            _iRefundService.SellerDealRefund(refundId, refundauditstatus, sellerRemark, CurrentSellerManager.UserName);
            result.success = true;
            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult ConfirmRefundGood(long refundId)
        {
            Result result = new Result();
            try
            {
                _iRefundService.SellerConfirmRefundGood(refundId, CurrentSellerManager.UserName);
                result.success = true;
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
            }
            return Json(result);
        }

        public static string HTMLEncode(string txt)
        {
            if (string.IsNullOrEmpty(txt))
                return string.Empty;
            string Ntxt = txt;

            Ntxt = Ntxt.Replace(" ", "&nbsp;");

            Ntxt = Ntxt.Replace("<", "&lt;");

            Ntxt = Ntxt.Replace(">", "&gt;");

            Ntxt = Ntxt.Replace("\"", "&quot;");

            Ntxt = Ntxt.Replace("'", "&#39;");

            //Ntxt = Ntxt.Replace("\n", "<br>");

            return Ntxt;

        }

        /// <summary>
        /// 导出退款记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ActionResult ExportToExcel(RefundQuery query)
        {
            query.ShopId = CurrentSellerManager.ShopId;
            var orderResults = RefundApplication.GetAllFullOrderReFunds(query);

            string strTitle = "退款记录";
            #region 导出时标题名称
            if (query.ShowRefundType.HasValue)
            {
                switch (query.ShowRefundType)
                {
                    case 1:
                        strTitle = "订单退款";
                        break;
                    case 2:
                        strTitle = "退款记录";
                        break;
                    case 3:
                        strTitle = "退货记录";
                        break;
                    case 4:
                        strTitle = "货品退款";
                        break;
                }
            }
            #endregion

            return ExcelView("ExportOrderRefundinfo", "商家" + strTitle, orderResults);
        }
    }
}