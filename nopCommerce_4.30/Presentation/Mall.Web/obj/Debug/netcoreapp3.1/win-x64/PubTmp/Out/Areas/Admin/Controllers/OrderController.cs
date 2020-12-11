using Mall.Core;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Framework;
using Mall.Web.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using Mall.Application;
using Mall.Entities;
using System;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class OrderController : BaseAdminController
    {
        private IOrderService _iOrderService;
        private IExpressService _iExpressService;
        private IPaymentConfigService _iPaymentConfigService;
        private IFightGroupService _iFightGroupService;
        public OrderController(IOrderService iOrderService, IExpressService iExpressService, IPaymentConfigService iPaymentConfigService
            , IFightGroupService iFightGroupService)
        {
            _iOrderService = iOrderService;
            _iExpressService = iExpressService;
            _iPaymentConfigService = iPaymentConfigService;
            _iFightGroupService = iFightGroupService;
        }

        public ActionResult Management(long? shopBranchId)
        {
            ViewBag.hasHistory = SiteSettingApplication.SiteSettings.IsOpenHistoryOrder;
            var model = PaymentApplication.GetPaymentTypeDesc();
            var shopbranchName = "";
            if (shopBranchId.HasValue)
            {
                var shopbranch = ShopBranchApplication.GetShopBranchById(shopBranchId.Value);
                if (shopbranch != null)
                    shopbranchName = shopbranch.ShopBranchName;
            }
            ViewBag.ShopBranchName = shopbranchName;
            return View(model);
        }


        public ActionResult Detail(long id)
        {
            var order = _iOrderService.GetOrder(id);
            if (order == null)
            {
                throw new MallException("错误的订单信息");
            }
            if (order.OrderType == Entities.OrderInfo.OrderTypes.FightGroup)
            {
                var fgord = _iFightGroupService.GetFightGroupOrderStatusByOrderId(order.Id);
                order.FightGroupOrderJoinStatus = fgord.GetJoinStatus;
                order.FightGroupCanRefund = fgord.CanRefund;
            }
            var orderItems = _iOrderService.GetOrderItemsByOrderId(order.Id);
            //处理平台佣金
            var orderRefunds = RefundApplication.GetOrderRefundList(id);
            foreach (var item in orderItems)
            {
                var refund = orderRefunds.Where(e => e.OrderItemId == item.Id).Sum(e => e.ReturnPlatCommission);
                item.PlatCommission = Math.Round(item.CommisRate * (item.RealTotalPrice - item.FullDiscount - item.CouponDiscount), 2);
                if (refund > 0)
                {
                    item.PlatCommission = item.PlatCommission - refund;
                }
                item.PlatCommission = (item.PlatCommission < 0) ? 0 : item.PlatCommission;

            }
            ViewBag.OrderItems = orderItems;
            ViewBag.Logs = _iOrderService.GetOrderLogs(order.Id);
            ViewBag.Coupon = 0;
            string shipperAddress = string.Empty, shipperTelPhone = string.Empty;
            #region 门店信息
            if (order.ShopBranchId > 0)
            {
                var shopBranchInfo = ShopBranchApplication.GetShopBranchById(order.ShopBranchId);
                if (shopBranchInfo != null)
                {
                    ViewBag.ShopBranchInfo = shopBranchInfo;
                    if (order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.Finish) ViewBag.ShopBranchContactUser = shopBranchInfo.UserName;
                    if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                    {
                        shipperAddress = RegionApplication.GetFullName(shopBranchInfo.AddressId) + " " + shopBranchInfo.AddressDetail;
                        shipperTelPhone = shopBranchInfo.ContactPhone;
                    }
                }
            }
            #endregion
            if (order.OrderType == OrderInfo.OrderTypes.Virtual)
            {
                ViewBag.VirtualOrderItemInfos = OrderApplication.GetVirtualOrderItemInfosByOrderId(order.Id);
                ViewBag.OrderVerificationCodeInfos = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(new List<long>() { order.Id });
                if (order.ShopBranchId == 0)
                {
                    var verificationShipper = ShopShippersApplication.GetDefaultVerificationShipper(order.ShopId);
                    if (verificationShipper != null)
                    {
                        shipperAddress = RegionApplication.GetFullName(verificationShipper.RegionId) + " " + verificationShipper.Address;
                        shipperTelPhone = verificationShipper.TelPhone;
                    }
                }
            }
            ViewBag.ShipperAddress = shipperAddress;
            ViewBag.ShipperTelPhone = shipperTelPhone;
            //发票信息
            ViewBag.OrderInvoiceInfo = OrderApplication.GetOrderInvoiceInfo(order.Id);
            //统一显示支付方式名称
            order.PaymentTypeName = PaymentApplication.GetPaymentTypeDescById(order.PaymentTypeGateway) ?? order.PaymentTypeName;
            return View(order);
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult List(OrderQuery query, int page, int rows)
        {
            query.PageNo = page;
            query.PageSize = rows;
            query.Operator = Operator.Admin;
            query.PaymentTypeGateways = PaymentApplication.GetPaymentIdByDesc(query.PaymentTypeGateway);
            var fullOrders = OrderApplication.GetFullOrders(query);
            var models = fullOrders.Models.ToList();

            var shops = ShopApplication.GetShops(fullOrders.Models.Select(p => p.ShopId).ToList());
            var shopBranchs = ShopBranchApplication.GetShopBranchs(models.Where(p => p.DeliveryType == CommonModel.DeliveryType.SelfTake && p.ShopBranchId != 0).Select(p => p.ShopBranchId).ToList());

            IEnumerable<OrderModel> orderModels = models.Select(item =>
            {
                var shop = shops.FirstOrDefault(sp => sp.Id == item.ShopId);

                return new OrderModel()
                {
                    OrderId = item.Id,
                    OrderStatus = item.OrderStatus.ToDescription(),
                    OrderState = (int)item.OrderStatus,
                    OrderDate = item.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    ShopId = item.ShopId,
                    ShopName = item.ShopBranchId > 0 ? item.ShopBranchName : item.ShopName,
                    ShopBranchName = item.DeliveryType == CommonModel.DeliveryType.SelfTake && item.ShopBranchId > 0 && shopBranchs.FirstOrDefault(sb => sb.Id == item.ShopBranchId) != null ? shopBranchs.FirstOrDefault(sb => sb.Id == item.ShopBranchId).ShopBranchName : "",
                    UserId = item.UserId,
                    UserName = item.UserName,
                    TotalPrice = item.OrderTotalAmount,
                    PaymentTypeName = item.PaymentTypeName,
                    PlatForm = (int)item.Platform,
                    IconSrc = GetIconSrc(item.Platform),
                    PlatformText = item.Platform.ToDescription(),
                    PaymentTypeGateway = item.PaymentTypeGateway,
                    PayDate = item.PayDate,
                    PaymentTypeStr = item.PaymentTypeDesc,
                    PaymentType = item.PaymentType,
                    OrderType = item.OrderType,
                    GatewayOrderId = item.GatewayOrderId,
                    Payee = shop.ContactsName,
                    CellPhone = item.CellPhone,
                    RegionFullName = item.RegionFullName,
                    Address = item.Address,
                    SellerRemark = item.SellerRemark,
                    UserRemark = item.UserRemark,
                    OrderItems = item.OrderItems,
                    SellerRemarkFlag = item.SellerRemarkFlag,
                    IsVirtual = item.OrderType == OrderInfo.OrderTypes.Virtual
                };
            });

            DataGridModel<OrderModel> dataGrid = new DataGridModel<OrderModel>()
            {
                rows = orderModels,
                total = fullOrders.Total
            };
            return Json(dataGrid);
        }

        public ActionResult ExportToExcel(OrderQuery query)
        {
            var orders = OrderApplication.GetAllFullOrders(query);

            return ExcelView("ExportOrderinfo", "平台订单信息", orders);
        }


        /// <summary>
        /// 获取订单来源图标地址
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        string GetIconSrc(PlatformType platform)
        {
            if (platform == PlatformType.IOS || platform == PlatformType.Android)
                return "/images/app.png";
            return string.Format("/images/{0}.png", platform.ToString());
        }

        /// <summary>
        /// 付款
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="payRemark">收款备注</param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult ConfirmPay(long orderId, string payRemark)
        {
            Result result = new Result();
            OrderApplication.PlatformConfirmOrderPay(orderId, payRemark, CurrentManager.UserName);
            //PaymentHelper.IncreaseSaleCount(new List<long> { orderId });
            result.success = true;
            return Json(result);

        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="payRemark">收款备注</param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult CloseOrder(long orderId)
        {
            Result result = new Result();
            _iOrderService.PlatformCloseOrder(orderId, CurrentManager.UserName);
            result.success = true;

            return Json(result);

        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult GetExpressData(string expressCompanyName, string shipOrderNumber)
        {
            string content = "暂时没有此快递单号的信息";
            if (string.IsNullOrEmpty(expressCompanyName) || string.IsNullOrEmpty(shipOrderNumber))
                return Json(content);
            string kuaidi100Code = _iExpressService.GetExpress(expressCompanyName).Kuaidi100Code;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format("https://www.kuaidi100.com/query?type={0}&postid={1}", kuaidi100Code, shipOrderNumber));
            request.Timeout = 8000;


            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream stream = response.GetResponseStream();
                System.IO.StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.GetEncoding("UTF-8"));

                // 读取流字符串内容
                content = streamReader.ReadToEnd();
                content = content.Replace("&amp;", "");
                content = content.Replace("&nbsp;", "");
                content = content.Replace("&", "");
            }

            return Json(content);
        }

        public ActionResult InvoiceContext()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetInvoiceContexts(int page = 1, int rows = 20)
        {
            var model = _iOrderService.GetInvoiceContexts(page, rows);
            return Json(new
            {
                rows = model.Models,
                total = model.Total
            });
        }

        [HttpPost]
        public ActionResult SaveInvoiceContext(string name, long id = -1)
        {
            Entities.InvoiceContextInfo info = new Entities.InvoiceContextInfo()
            {
                Id = id,
                Name = name
            };
            _iOrderService.SaveInvoiceContext(info);
            return Json(true);
        }

        [HttpPost]
        public ActionResult DeleteInvoiceContexts(long id)
        {
            _iOrderService.DeleteInvoiceContext(id);
            return Json(true);
        }

        public JsonResult GetShopAndShopBranch(string keyWords, sbyte type)
        {
            var result = OrderApplication.GetShopOrShopBranch(keyWords, type);
            var values = result.Select(item => new { type = item.Type, value = item.Name, id = item.SearchId });
            return Json(values);
        }

    }
}