using Mall.Application;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.SmallProgAPI.Model;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace Mall.SmallProgAPI
{
    /// <summary>
    /// 售后
    /// </summary>
    public class OrderRefundController : BaseApiController
    {
        #region 获取信息
        /// <summary>
        /// 获取售后列表
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetList")]
        public object GetList(string openId, int pageIndex = 1, int pageSize = 10)
        {
            CheckUserLogin();
            DateTime? startDate = null;
            DateTime? endDate = null;
            var queryModel = new RefundQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                UserId = CurrentUser.Id,
                PageSize = pageSize,
                PageNo = pageIndex,
                ShowRefundType = 0
            };
            var refunds = RefundApplication.GetOrderRefunds(queryModel);
            var refundorderItems = Application.OrderApplication.GetOrderItems(refunds.Models.Select(p => p.OrderItemId));
            dynamic result = new System.Dynamic.ExpandoObject();
            result.RecordCount = refunds.Total;
            result.Data = refunds.Models.Select(refund =>
            {
                var orderItem = OrderApplication.GetOrderItem(refund.OrderItemId);
                var vshop = VshopApplication.GetVShopByShopId(refund.ShopId) ?? new Entities.VShopInfo() { Id = 0 };
                var order = OrderApplication.GetOrder(refund.OrderId, CurrentUser.Id);
                var status = refund.RefundStatus;
                if (order != null && order.ShopBranchId > 0)
                    status = status.Replace("商家", "门店");

                var orderItems = new List<Entities.OrderItemInfo>();
                if (refund.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    orderItems = OrderApplication.GetOrderItems(refund.OrderId);
                }
                else
                {
                    orderItems.Add(orderItem);
                }
                return new OrderRefundItem
                {
                    OrderId = refund.OrderId.ToString(),
                    OrderTotal = order.OrderTotalAmount.ToString("f2"),
                    Status = refund.RefundStatusValue.Value,
                    StatusText = status,
                    ShopBranchId = order.ShopBranchId,
                    AdminRemark = (refund.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed ? refund.ManagerRemark : refund.SellerRemark),
                    AfterSaleId = refund.Id,
                    AfterSaleType = refund.RefundMode.GetHashCode(),
                    ApplyForTime = refund.ApplyDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    RefundAmount = refund.Amount.ToString("f2"),
                    RefundType = refund.RefundPayType.GetHashCode(),
                    RefundTypeText = refund.RefundPayType.ToDescription(),
                    SkuId = orderItem.SkuId,
                    UserExpressCompanyName = refund.ExpressCompanyName,
                    UserRemark = "",
                    UserShipOrderNumber = refund.ShipOrderNumber,
                    IsRefund = refund.RefundMode != OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund,
                    IsReturn = refund.RefundMode == OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund,
                    ShopName = refund.ShopName,
                    Vshopid = vshop.Id,
                    Remark = refund.ReasonDetail,
                    ContactPerson = refund.ContactPerson,
                    ContactCellPhone = refund.ContactCellPhone,
                    LineItems = orderItems.Select(d => new OrderRefundSku
                    {
                        Status = refund.SellerAuditStatus.GetHashCode(),
                        StatusText = refund.SellerAuditStatus.ToDescription(),
                        SkuId = d.SkuId,
                        Name = d.ProductName,
                        Price = d.SalePrice,
                        Amount = d.SalePrice * d.Quantity,
                        Quantity = d.Quantity,
                        Image = Core.MallIO.GetRomoteProductSizeImage(d.ThumbnailsUrl, 1, (int)Mall.CommonModel.ImageSize.Size_150),
                        SkuText = d.Color + " " + d.Size + " " + d.Version,
                        ProductId = d.ProductId
                    }).ToList(),
                };
            });

            return Json(result);
        }
        /// <summary>
        ///获取售后详情
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="RefundId"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetRefundDetail")]
        public object GetRefundDetail(string openId, long RefundId)
        {
            CheckUserLogin();
            var _iOrderService = ServiceProvider.Instance<IOrderService>.Create;
            var refundser = ServiceProvider.Instance<IRefundService>.Create;
            var refundinfo = refundser.GetOrderRefund(RefundId, CurrentUser.Id);
            var order = _iOrderService.GetOrder(refundinfo.OrderId, CurrentUser.Id);
            var status = refundinfo.RefundStatus;
            if (order != null &&  order.ShopBranchId > 0)
                status = status.Replace("商家", "门店");
            bool ResetActive = false;
            if ((refundinfo.RefundMode != Entities.OrderRefundInfo.OrderRefundMode.OrderRefund) || (refundinfo.RefundMode == Entities.OrderRefundInfo.OrderRefundMode.OrderRefund && order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitDelivery))
                ResetActive = true;
            string DealTime = refundinfo.SellerAuditDate.ToString("yyyy-MM-dd HH:mm:ss");
            if (refundinfo.SellerAuditStatus == Entities.OrderRefundInfo.OrderRefundAuditStatus.Audited)
            {
                DealTime = refundinfo.ManagerConfirmDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
            var orderItems = new List<Entities.OrderItemInfo>();
            if (refundinfo.RefundMode == Entities.OrderRefundInfo.OrderRefundMode.OrderRefund)
            {
                orderItems = _iOrderService.GetOrderItemsByOrderId(order.Id);
            }
            else
            {
                var temp = OrderApplication.GetOrderItem(refundinfo.OrderItemId);
                orderItems.Add(temp);
            }
            //OrderRefundGetRefundDetailModel result = new OrderRefundGetRefundDetailModel(true);
            var result = new RefundDetail()
            {
                AdminRemark = (refundinfo.ManagerConfirmStatus == Entities.OrderRefundInfo.OrderRefundConfirmStatus.Confirmed ? refundinfo.ManagerRemark : refundinfo.SellerRemark),
                ApplyForTime = refundinfo.ApplyDate.ToString("yyyy-MM-dd HH:mm:ss"),
                Remark = refundinfo.ReasonDetail,
                Status = refundinfo.RefundStatusValue.Value,
                StatusText = status,
                DealTime = DealTime,
                Operator = (refundinfo.SellerAuditStatus == Entities.OrderRefundInfo.OrderRefundAuditStatus.Audited ? "管理员" : "商家"),
                Reason = refundinfo.Reason,
                RefundId = refundinfo.Id,
                OrderId = refundinfo.OrderId.ToString(),
                Quantity = refundinfo.ReturnQuantity,
                RefundMoney = refundinfo.Amount.ToString("f2"),
                RefundType = refundinfo.RefundPayType.ToDescription(),
                ShopName = refundinfo.ShopName,
                BankAccountName = "",
                BankAccountNo = "",
                BankName = "",
                OrderTotal = order.RefundTotalAmount.ToString("f2"),
                CanResetActive = ResetActive,
                IsOrderRefundTimeOut = _iOrderService.IsRefundTimeOut(refundinfo.OrderId),
                FinishTime = refundinfo.ManagerConfirmStatus == Entities.OrderRefundInfo.OrderRefundConfirmStatus.Confirmed ? refundinfo.ManagerConfirmDate.ToString("yyyy-MM-dd HH:mm:ss") : "",
                ContactPerson = string.IsNullOrEmpty(refundinfo.ContactPerson) ? "" : refundinfo.ContactPerson,
                ContactCellPhone = string.IsNullOrEmpty(refundinfo.ContactCellPhone) ? "" : refundinfo.ContactCellPhone,
                ProductInfo = orderItems.Select(l => new RefundDetailSKU
                {
                    ProductId = l.ProductId,
                    ProductName = l.ProductName,
                    SKU = l.SKU,
                    SKUContent = l.Color + " " + l.Size + " " + l.Version,
                    Price = l.SalePrice.ToString("f2"),
                    Quantity = (int)l.Quantity,
                    ThumbnailsUrl = Core.MallIO.GetRomoteProductSizeImage(l.ThumbnailsUrl, 1, (int)Mall.CommonModel.ImageSize.Size_150),
                }).ToList()
            };
            result.OrderType = order.OrderType;
            if (order.OrderType == OrderInfo.OrderTypes.Virtual)
            {
                var list = refundinfo.VerificationCodeIds.Split(',').ToList();
                if (list.Count > 0)
                {
                    result.VerificationCodeIds = list.Select(a => a = Regex.Replace(a, @"(\d{4})", "$1 ")).ToList();
                }
            }
            return Json(result);
        }
        /// <summary>
        ///获取售后详情
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="RefundId"></param>
        /// <returns></returns>
        /// 

        [HttpGet("GetReturnDetail")]
        public object  GetReturnDetail(string openId, long returnId)
        {
            CheckUserLogin();
            var _iOrderService = ServiceProvider.Instance<IOrderService>.Create;
            var refundser = ServiceProvider.Instance<IRefundService>.Create;
            var refundinfo = refundser.GetOrderRefund(returnId, CurrentUser.Id);
            var order = _iOrderService.GetOrder(refundinfo.OrderId, CurrentUser.Id);
            var orderitem = OrderApplication.GetOrderItem(refundinfo.OrderItemId);
            var status = refundinfo.RefundStatus;
            if (order != null &&  order.ShopBranchId > 0)
                status = status.Replace("商家", "门店");
            bool ResetActive = false;
            if ((refundinfo.RefundMode != Entities.OrderRefundInfo.OrderRefundMode.OrderRefund) || (refundinfo.RefundMode == Entities.OrderRefundInfo.OrderRefundMode.OrderRefund && order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitDelivery))
                ResetActive = true;
            string DealTime = refundinfo.SellerAuditDate.ToString("yyyy-MM-dd HH:mm:ss");
            if (refundinfo.SellerAuditStatus == Entities.OrderRefundInfo.OrderRefundAuditStatus.Audited)
            {
                DealTime = refundinfo.ManagerConfirmDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
            var orderItems = new List<OrderItemInfo>();
            if (refundinfo.RefundMode == Entities.OrderRefundInfo.OrderRefundMode.OrderRefund)
            {
                orderItems = _iOrderService.GetOrderItemsByOrderId(order.Id);
            }
            else
            {
                var temp = OrderApplication.GetOrderItem(refundinfo.OrderItemId);
                orderItems.Add(temp);
            }
            //是否弃货
            var isDiscard = false;
            if (refundinfo.SellerAuditStatus == Entities.OrderRefundInfo.OrderRefundAuditStatus.Audited
                && refundinfo.BuyerDeliverDate == null
                && refundinfo.RefundMode != Entities.OrderRefundInfo.OrderRefundMode.OrderRefund
                && refundinfo.IsReturn == true
               )
            {
                isDiscard = true;
            }
            //是否拒绝
            bool isUnAudit = (refundinfo.SellerAuditStatus == Entities.OrderRefundInfo.OrderRefundAuditStatus.UnAudit);
            //是否退货
            bool isReturnGoods = (refundinfo.RefundMode == Entities.OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund);
            //OrderRefundGetReturnDetailModel result = new OrderRefundGetReturnDetailModel(true);
            int curappnum =  refundinfo.ApplyNumber;
            var log = refundser.GetRefundLogs(refundinfo.Id, curappnum, false).Select(d => new DTO.OrderRefundlog
            {
                Id = d.Id,
                ApplyNumber = d.ApplyNumber,
                OperateContent = d.OperateContent,
                OperateDate = d.OperateDate,
                ShowOperateDate=d.OperateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                Operator = d.Operator,
                RefundId = d.RefundId,
                Remark = d.Remark,
                Step = d.Step
            }).ToList();
            var result = new ReturnDetail()
            {
                AdminRemark = (refundinfo.ManagerConfirmStatus == Entities.OrderRefundInfo.OrderRefundConfirmStatus.Confirmed ? refundinfo.ManagerRemark : refundinfo.SellerRemark),
                ApplyForTime = refundinfo.ApplyDate.ToString("yyyy-MM-dd HH:mm:ss"),
                Remark = refundinfo.ReasonDetail,
                Status = refundinfo.RefundStatusValue.Value,
                StatusText = status,
                DealTime = DealTime,
                Operator = (refundinfo.SellerAuditStatus == Entities.OrderRefundInfo.OrderRefundAuditStatus.Audited ? "管理员" : "商家"),
                Reason = refundinfo.Reason,
                ReturnId = refundinfo.Id,
                OrderId = refundinfo.OrderId.ToString(),
                Quantity = refundinfo.ReturnQuantity,
                RefundMoney = refundinfo.Amount.ToString("f2"),
                RefundType = refundinfo.RefundPayType.ToDescription(),
                ShopName = refundinfo.ShopName,
                BankAccountName = "",
                BankAccountNo = "",
                BankName = "",
                OrderTotal = order.RefundTotalAmount.ToString("f2"),
                CanResetActive = ResetActive,
                IsOrderRefundTimeOut = _iOrderService.IsRefundTimeOut(refundinfo.OrderId),
                SkuId = orderitem.SkuId,
                Cellphone =  "",
                ShipAddress = "",
                ShipTo =  "",
                ShipOrderNumber = refundinfo.ShipOrderNumber,
                IsOnlyRefund = refundinfo.ReturnQuantity == 0,
                UserCredentials = new List<string>(),
                UserSendGoodsTime = refundinfo.BuyerDeliverDate.HasValue ? refundinfo.BuyerDeliverDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                ConfirmGoodsTime = refundinfo.SellerConfirmArrivalDate.HasValue ? refundinfo.SellerConfirmArrivalDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : refundinfo.SellerAuditDate.ToString("yyyy-MM-dd HH:mm:ss"),
                FinishTime = refundinfo.ManagerConfirmStatus == Entities.OrderRefundInfo.OrderRefundConfirmStatus.Confirmed ? refundinfo.ManagerConfirmDate.ToString("yyyy-MM-dd HH:mm:ss") : "",
                ContactPerson = refundinfo.ContactPerson,
                ContactCellPhone = refundinfo.ContactCellPhone,

                RefundLogs = log,
                ManagerConfirmStatus = refundinfo.ManagerConfirmStatus.ToDescription(),
                ManagerConfirmStatusValue = (int)refundinfo.ManagerConfirmStatus,
                ManagerConfirmDate = refundinfo.ManagerConfirmDate.ToString("yyyy-MM-dd HH:mm:ss"),
                ManagerRemark = refundinfo.ManagerRemark,

                SellerAuditStatus = refundinfo.SellerAuditStatus.ToDescription(),
                SellerAuditStatusValue = (int)refundinfo.SellerAuditStatus,
                SellerConfirmArrivalDate = refundinfo.SellerConfirmArrivalDate.HasValue ? refundinfo.SellerConfirmArrivalDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                SellerRemark = refundinfo.SellerRemark,
                SellerAuditDate = refundinfo.SellerAuditDate.ToString("yyyy-MM-dd HH:mm:ss"),

                BuyerDeliverDate = refundinfo.BuyerDeliverDate.HasValue ? refundinfo.BuyerDeliverDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                ExpressCompanyName = refundinfo.ExpressCompanyName,
                ApplyDate = refundinfo.ApplyDate,

                isDiscard = isDiscard,
                isUnAudit = isUnAudit,
                isReturnGoods = isReturnGoods,

                ProductInfo = orderItems.Select(l => new RefundDetailSKU
                {
                    ProductId = l.ProductId,
                    ProductName = l.ProductName,
                    SKU = l.SKU,
                    SKUContent = l.Color + " " + l.Size + " " + l.Version,
                    Price = l.SalePrice.ToString("f2"),
                    Quantity = (int)l.Quantity,
                    ThumbnailsUrl = Core.MallIO.GetRomoteProductSizeImage(l.ThumbnailsUrl, 1, (int)Mall.CommonModel.ImageSize.Size_150),
                }).ToList()
            };
            if (!string.IsNullOrWhiteSpace(refundinfo.CertPic1))
            {
                result.UserCredentials.Add(Mall.Core.MallIO.GetRomoteImagePath(refundinfo.CertPic1));
            }
            if (!string.IsNullOrWhiteSpace(refundinfo.CertPic2))
            {
                result.UserCredentials.Add(Mall.Core.MallIO.GetRomoteImagePath(refundinfo.CertPic2));
            }
            if (!string.IsNullOrWhiteSpace(refundinfo.CertPic3))
            {
                result.UserCredentials.Add(Mall.Core.MallIO.GetRomoteImagePath(refundinfo.CertPic3));
            }

            if (order.ShopBranchId <= 0)
            {
                var data = ShopShippersApplication.GetDefaultGetGoodsShipper(order.ShopId);
                if (data != null)
                {
                    data.RegionStr = RegionApplication.GetFullName(data.RegionId);
                    result.Cellphone = data.TelPhone;
                    result.ShipAddress = data.RegionStr + data.Address;
                    result.ShipTo = data.ShipperName;
                }
            }
            else
            {
                var data = ShopBranchApplication.GetShopBranchById(order.ShopBranchId);
                string redionstr = "";
                if (data != null)
                {
                    redionstr = RegionApplication.GetFullName(data.AddressId);
                    result.Cellphone = data.ContactPhone;
                    result.ShipAddress = redionstr + data.AddressDetail;
                    result.ShipTo = data.ContactUser;
                }
            }
            return Json(result);
        }
        /// <summary>
        /// 获取物流列表
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetExpressList")]
        public object GetExpressList()
        {
            var express = ExpressApplication.GetAllExpress();
            var result = express.Select(d => new ExpressItem
            {
                ExpressName = d.Name,
                Kuaidi100Code = d.Kuaidi100Code,
                TaobaoCode = d.TaobaoCode,
            });
            return Json(result);
        }
        /// <summary>
        /// 售后检测
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetOrderRefundModel")]
        public object GetOrderRefundModel(string openId, long orderId, string skuId = "", bool isReturn = false)
        {
            CheckUserLogin();
            try
            {
                dynamic result = new System.Dynamic.ExpandoObject();
                //计算可退金额 预留
                OrderApplication.CalculateOrderItemRefund(orderId);
                var order = OrderApplication.GetOrder(orderId, CurrentUser.Id);
                if (order == null) return Json(ErrorResult<dynamic>("该订单已删除或不属于该用户"));
                if (order.OrderType != OrderInfo.OrderTypes.Virtual && (int)order.OrderStatus < 2) return Json(ErrorResult<dynamic>("错误的售后申请,订单状态有误"));

                var orderitems = OrderApplication.GetOrderItemsByOrderId(order.Id);
                
                if (order.OrderType != OrderInfo.OrderTypes.Virtual && string.IsNullOrEmpty(skuId) && order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitDelivery && order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                    return Json(ErrorResult<dynamic>("错误的订单退款申请,订单状态有误"));
                if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                {
                    //如果为虚拟商品，则要判断该商品是否允许退款，且该订单中是否至少有一个待核销的核销码
                    var orderItemInfo = OrderApplication.GetOrderItemsByOrderId(order.Id).FirstOrDefault();
                    if (orderItemInfo != null)
                    {
                        //itemId = orderItemInfo.Id;
                        var virtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(orderItemInfo.ProductId);
                        if (virtualProductInfo != null)
                        {
                            if (virtualProductInfo.SupportRefundType == 3)
                            {
                                return Json(ErrorResult<dynamic>("该商品不支持退款"));
                            }
                            if (virtualProductInfo.SupportRefundType == 1 && DateTime.Now > virtualProductInfo.EndDate.Value)
                            {
                                return Json(ErrorResult<dynamic>("该商品不支持过期退款"));
                            }
                            var orderVerificationCodes = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(new List<long>() { order.Id });
                            long num = orderVerificationCodes.Where(a => a.Status == OrderInfo.VerificationCodeStatus.WaitVerification).Count();
                            if (num == 0)
                            {
                                return Json(ErrorResult<dynamic>("该商品没有可退的核销码"));
                            }
                            int validityType = 0; string startDate = string.Empty, endDate = string.Empty;
                            validityType = virtualProductInfo.ValidityType ? 1 : 0;
                            if (validityType == 1)
                            {
                                startDate = virtualProductInfo.StartDate.Value.ToString("yyyy-MM-dd");
                                endDate = virtualProductInfo.EndDate.Value.ToString("yyyy-MM-dd");
                            }
                            result.ValidityType = validityType;
                            result.StartDate = startDate;
                            result.EndDate = endDate;
                            result.OrderItemId = orderItemInfo.Id;
                        }
                    }
                }
                result.CanBackReturn = order.CanBackOut();
                result.CanToBalance = true;
                if (order.OrderType == OrderInfo.OrderTypes.Virtual || order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                {
                    if (isReturn)
                    {
                        return Json(ErrorResult<dynamic>("订单状态不可以退货"));
                    }
                    var _tmprefund = RefundApplication.GetOrderRefundByOrderId(orderId);
                    if (order.OrderType != OrderInfo.OrderTypes.Virtual && _tmprefund != null  && _tmprefund.SellerAuditStatus != Entities.OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
                    {
                        return Json(ErrorResult<dynamic>("售后中，不可激活"));
                    }
                    result.MaxRefundQuantity = 0;
                    result.oneReundAmount = "0.00";
                    result.MaxRefundAmount = order.OrderEnabledRefundAmount;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(skuId))
                    {
                        return Json(ErrorResult<dynamic>("错误的参数:SkuId"));
                    }
                    var _ordItem = orderitems.FirstOrDefault(d => d.SkuId == skuId);
                    if (_ordItem != null)
                    {
                        var _tmprefund = RefundApplication.GetOrderRefundList(orderId).FirstOrDefault(d => d.OrderItemId == _ordItem.Id);
                        if (order.OrderType != OrderInfo.OrderTypes.Virtual && _tmprefund != null && _tmprefund.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
                        {
                            return Json(ErrorResult<dynamic>("售后中，不可激活"));
                        }
                    }
                    else
                    {
                        return Json(ErrorResult<dynamic>("错误的参数:SkuId"));
                    }

                    result.MaxRefundQuantity = _ordItem.Quantity - _ordItem.ReturnQuantity;
                    result.oneReundAmount = (_ordItem.EnabledRefundAmount.Value / _ordItem.Quantity);
                    result.MaxRefundAmount = (_ordItem.EnabledRefundAmount.Value - _ordItem.RefundPrice);
                }
                if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                {
                    var item = orderitems.FirstOrDefault();
                    var count = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(new List<long>() { order.Id }).Where(a => a.Status != OrderInfo.VerificationCodeStatus.WaitVerification).ToList().Count;
                    if (item.EnabledRefundAmount.HasValue)
                    {
                        decimal price = item.EnabledRefundAmount.Value / item.Quantity;
                        result.MaxRefundAmount = item.EnabledRefundAmount.Value - Math.Round(count * price, 2, MidpointRounding.AwayFromZero);
                    }
                }
                var reasonlist = RefundApplication.GetRefundReasons();
                result.RefundReasons = reasonlist;    //理由List

                result.ContactPerson = string.IsNullOrEmpty(order.ShipTo) ? CurrentUser.RealName : order.ShipTo;
                result.ContactCellPhone = string.IsNullOrEmpty(order.CellPhone) ? CurrentUser.CellPhone : order.CellPhone;
                
                if (order.DeliveryType == CommonModel.DeliveryType.SelfTake)
                {
                    var shopBranch = ShopBranchApplication.GetShopBranchById(order.ShopBranchId);
                    result.ReturnGoodsAddress = RegionApplication.GetFullName(shopBranch.AddressId);
                    result.ReturnGoodsAddress += " " + shopBranch.AddressDetail;
                    result.ReturnGoodsAddress += " " + shopBranch.ContactPhone;
                }
                result.IsVirtual = order.OrderType == OrderInfo.OrderTypes.Virtual ? 1 : 0;
                if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                {
                    var codes = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(new List<long>() { order.Id }).Where(a => a.Status == OrderInfo.VerificationCodeStatus.WaitVerification).ToList();
                    result.OrderVerificationCode = codes.Select(a => a.VerificationCode).Select(p => p = Regex.Replace(p, @"(\d{4})", "$1 "));
                }
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(ErrorResult<dynamic>(ex.Message));
            }
        }
        #endregion

        /// <summary>
        /// 申请订单退款
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost("PostApplyRefund")]
        public  object  PostApplyRefund(RefundApplyRefundPModel model)
        {
            CheckUserLogin();

            try
            {
                //计算可退金额 预留
                OrderApplication.CalculateOrderItemRefund(model.orderId);
                //参数处理
                if (!model.refundId.HasValue)
                {
                    var _tmprefund = RefundApplication.GetOrderRefundByOrderId(model.orderId);
                    if (_tmprefund != null)
                    {
                        model.refundId = _tmprefund.Id;
                    }
                }
                OrderRefundInfo info = new OrderRefundInfo();

                #region 表单数据
                info.OrderId = model.orderId;
                if (null != model.OrderItemId)
                    info.OrderItemId = model.OrderItemId.Value;
                if (null != model.refundId)
                    info.Id = model.refundId.Value;
                info.RefundType = 1;
                info.ReturnQuantity = 0;
                info.Amount = model.Amount;
                info.Reason = model.RefundReason;
                info.ContactPerson = model.ContactPerson;
                info.ContactCellPhone = model.ContactCellPhone;
                info.RefundPayType = model.RefundType;
                info.VerificationCodeIds = model.VerificationCodeIds;

                info.formId = model.formId;
                info.ReasonDetail = model.ReasonDetail;

                if (info.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund && !model.refundId.HasValue && model.OrderItemId.HasValue)
                {
                    var _refobj = OrderApplication.GetOrderRefunds(new long[] { model.OrderItemId.Value }).FirstOrDefault(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                    if (_refobj != null)
                    {
                        info.Id = _refobj.Id;
                    }
                }
                #endregion

                #region 初始化售后单的数据
                var order = OrderApplication.GetOrder(info.OrderId, CurrentUser.Id);
                if (order.OrderType != OrderInfo.OrderTypes.Virtual)
                    info.Amount = order.OrderEnabledRefundAmount;
                if (order == null) throw new Mall.Core.MallException("该订单已删除或不属于该用户");
                if (order.OrderType != OrderInfo.OrderTypes.Virtual && (int)order.OrderStatus < 2) throw new Mall.Core.MallException("错误的售后申请,订单状态有误");
                if (order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitSelfPickUp || order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitVerification)
                {
                    info.RefundMode = OrderRefundInfo.OrderRefundMode.OrderRefund;
                    info.ReturnQuantity = 0;
                }
                else
                {
                    throw new Mall.Core.MallException("仅待发货或待自提订单可以申请订单退款");
                }
                if (info.RefundType == 1)
                {
                    info.ReturnQuantity = 0;
                    info.IsReturn = false;
                }
                if (order.OrderType != OrderInfo.OrderTypes.Virtual && info.ReturnQuantity < 0) throw new Mall.Core.MallException("错误的退货数量");
                var orderitem = OrderApplication.GetOrderItem(info.OrderItemId);
                if (orderitem == null && info.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund) throw new Mall.Core.MallException("该订单条目已删除或不属于该用户");
                if (order.OrderType != OrderInfo.OrderTypes.Virtual)
                {
                    if (info.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                    {
                        if (order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitDelivery && order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitSelfPickUp) throw new Mall.Core.MallException("错误的订单退款申请,订单状态有误");
                        info.IsReturn = false;
                        info.ReturnQuantity = 0;
                        if (info.Amount > order.OrderEnabledRefundAmount) throw new Mall.Core.MallException("退款金额不能超过订单的实际支付金额");
                    }
                    else
                    {
                        if (info.Amount > (orderitem.EnabledRefundAmount - orderitem.RefundPrice)) throw new Mall.Core.MallException("退款金额不能超过订单的可退金额");
                        if (info.ReturnQuantity > (orderitem.Quantity - orderitem.ReturnQuantity)) throw new Mall.Core.MallException("退货数量不可以超出可退数量");
                    }
                }
                info.IsReturn = false;
                if (info.ReturnQuantity > 0) info.IsReturn = true;
                if (info.RefundType == 2) info.IsReturn = true;
                if (order.OrderType != OrderInfo.OrderTypes.Virtual && info.IsReturn == true && info.ReturnQuantity < 1) throw new Mall.Core.MallException("错误的退货数量");
                if (info.Amount <= 0) throw new Mall.Core.MallException("错误的退款金额");
                info.ShopId = order.ShopId;
                info.ShopName = order.ShopName;
                info.UserId = CurrentUser.Id;
                info.Applicant = CurrentUser.UserName;
                info.ApplyDate = DateTime.Now;
                info.Reason = HTMLEncode(info.Reason.Replace("'", "‘").Replace("\"", "”"));
                info.ReasonDetail = model.ReasonDetail;
                if (!string.IsNullOrEmpty(model.UserCredentials))
                {
                    var certPics = model.UserCredentials.Split(',');
                    switch (certPics.Length)
                    {
                        case 1:
                            info.CertPic1 = MoveImages(certPics[0], CurrentUser.Id, info.OrderItemId);
                            break;
                        case 2:
                            info.CertPic1 = MoveImages(certPics[0], CurrentUser.Id, info.OrderItemId);
                            info.CertPic2 = MoveImages(certPics[1], CurrentUser.Id, info.OrderItemId);
                            break;
                        case 3:
                            info.CertPic1 = MoveImages(certPics[0], CurrentUser.Id, info.OrderItemId);
                            info.CertPic2 = MoveImages(certPics[1], CurrentUser.Id, info.OrderItemId);
                            info.CertPic3 = MoveImages(certPics[2], CurrentUser.Id, info.OrderItemId);
                            break;
                    }
                }
                //info.CertPic1 = MoveImages(model.CertPic1, CurrentUser.Id, info.OrderItemId);
                //info.CertPic2 = MoveImages(model.CertPic2, CurrentUser.Id, info.OrderItemId);
                //info.CertPic3 = MoveImages(model.CertPic3, CurrentUser.Id, info.OrderItemId);
                #endregion
                //info.RefundAccount = HTMLEncode(info.RefundAccount.Replace("'", "‘").Replace("\"", "”"));
                if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                {
                    if (string.IsNullOrWhiteSpace(info.VerificationCodeIds))
                        throw new Mall.Core.MallException("虚拟订单退款核销码不能为空");

                    info.VerificationCodeIds = Regex.Replace(info.VerificationCodeIds, @"\s", "");

                    //检测核销码都为正确的
                    var codeList = info.VerificationCodeIds.Split(',').ToList();
                    var codes = OrderApplication.GetOrderVerificationCodeInfoByCodes(codeList);
                    if (codes.Count != codeList.Count)
                        throw new Mall.Core.MallException("包含无效的核销码");
                    foreach (var item in codes)
                    {
                        if (item.Status != OrderInfo.VerificationCodeStatus.WaitVerification)
                        {
                            throw new Mall.Core.MallException("包含已申请售后的核销码");
                        }
                    }
                    info.ReturnQuantity = codes.Count;
                }
                if (order.OrderType != OrderInfo.OrderTypes.Virtual)
                {
                    if (info.Id > 0)
                    {
                        RefundApplication.ActiveRefund(info);
                    }
                    else
                    {
                        RefundApplication.AddOrderRefund(info);
                    }
                }
                else
                {
                    RefundApplication.AddOrderRefund(info);
                    #region 处理退款
                    try
                    {
                        //虚拟订单自动退款，异常不提示用户,进入平台待审核
                        string notifyurl = CurrentUrlHelper.CurrentUrlNoPort() + "/Pay/RefundNotify/{0}";
                        var result = RefundApplication.ConfirmRefund(info.Id, "虚拟订单申请售后自动退款", "", notifyurl);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("虚拟商品自动退异常", ex);
                    }
                    #endregion
                }
                return Json(new{ msg = "成功的申请了退款"});
            }
            catch (MallException he)
            {
                return Json(ErrorResult<int>(he.Message));
            }
            catch (Exception ex)
            {
                Log.Error("[SPAPI]Refund：" + ex.Message);
                return Json(ErrorResult<int>("系统内部异常"));
            }
        }
        /// <summary>
        /// 订单项退货退款
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost("PostApplyReturn")]
        public  object PostApplyReturn(RefundApplyReturnPModel model)
        {
            CheckUserLogin();
            try
            {
                OrderRefundInfo info = new OrderRefundInfo();
                //计算可退金额 预留
                OrderApplication.CalculateOrderItemRefund(model.orderId);
                var order = OrderApplication.GetOrder(model.orderId, CurrentUser.Id);
                if (order == null) throw new Mall.Core.MallException("该订单已删除或不属于该用户");
                if ((int)order.OrderStatus < 2) throw new Mall.Core.MallException("错误的售后申请,订单状态有误");
                if (order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                {
                    info.RefundMode = OrderRefundInfo.OrderRefundMode.OrderRefund;
                    info.ReturnQuantity = 0;
                }
                var orderitems = OrderApplication.GetOrderItemsByOrderId(order.Id);
                info.formId = model.formId;
                //参数处理
                if (!model.refundId.HasValue)  // 通过skuid获取售后编号
                {
                    if (info.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                    {
                        var _tmprefund = RefundApplication.GetOrderRefundByOrderId(model.orderId);
                        if (_tmprefund != null)
                        {
                            model.refundId = _tmprefund.Id;
                        }
                    }
                    else
                    {
                        if (!model.OrderItemId.HasValue)
                        {
                            if (string.IsNullOrWhiteSpace(model.skuId))
                            {
                                throw new Mall.Core.MallException("参数错误");
                            }
                           
                            foreach (var item in orderitems)
                            {
                                if (item.SkuId == model.skuId)
                                {
                                    model.OrderItemId = item.Id;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (info.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund)
                    {
                        var _tmprefund = RefundApplication.GetOrderRefund(model.refundId.Value);
                        if (_tmprefund == null)
                        {
                            throw new Mall.Core.MallException("参数错误");
                        }
                        model.OrderItemId = _tmprefund.OrderItemId;

                    }
                }

                #region 表单数据
                info.OrderId = model.orderId;
                if (null != model.OrderItemId)
                    info.OrderItemId = model.OrderItemId.Value;
                if (null != model.refundId)
                    info.Id = model.refundId.Value;
                info.RefundType = model.Quantity > 0 ? 2 : 1;
                info.ReturnQuantity = model.Quantity;
                info.Amount = model.RefundAmount;
                info.Reason = model.RefundReason;
                info.ContactPerson = model.ContactPerson;
                info.ContactCellPhone = model.ContactCellPhone;
                info.RefundPayType = model.RefundType;
                info.ReasonDetail = model.ReasonDetail;

                if (info.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund && !model.refundId.HasValue && model.OrderItemId.HasValue)
                {
                    var _refobj = OrderApplication.GetOrderRefunds(new long[] { model.OrderItemId.Value }).FirstOrDefault(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                    if (_refobj != null)
                    {
                        info.Id = _refobj.Id;
                    }
                }
                #endregion

                #region 初始化售后单的数据
                if (info.RefundType == 1)
                {
                    info.ReturnQuantity = 0;
                    info.IsReturn = false;
                }
                if (info.ReturnQuantity < 0) throw new Mall.Core.MallException("错误的退货数量");
                var orderitem = OrderApplication.GetOrderItem(info.OrderItemId);
                if (orderitem == null && info.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund) throw new Mall.Core.MallException("该订单条目已删除或不属于该用户");
                if (info.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    if (order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitDelivery && order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitSelfPickUp) throw new Mall.Core.MallException("错误的订单退款申请,订单状态有误");
                    info.IsReturn = false;
                    info.ReturnQuantity = 0;
                    if (info.Amount > order.OrderEnabledRefundAmount) throw new Mall.Core.MallException("退款金额不能超过订单的实际支付金额");
                }
                else
                {
                    if (info.Amount > (orderitem.EnabledRefundAmount - orderitem.RefundPrice)) throw new Mall.Core.MallException("退款金额不能超过订单的可退金额");
                    if (info.ReturnQuantity > (orderitem.Quantity - orderitem.ReturnQuantity)) throw new Mall.Core.MallException("退货数量不可以超出可退数量");
                }
                info.IsReturn = false;
                if (info.ReturnQuantity > 0) info.IsReturn = true;
                if (info.RefundType == 2) info.IsReturn = true;
                if (info.IsReturn == true && info.ReturnQuantity < 1) throw new Mall.Core.MallException("错误的退货数量");
                if (info.Amount <= 0) throw new Mall.Core.MallException("错误的退款金额");
                info.ShopId = order.ShopId;
                info.ShopName = order.ShopName;
                info.UserId = CurrentUser.Id;
                info.Applicant = CurrentUser.UserName;
                info.ApplyDate = DateTime.Now;
                info.Reason = HTMLEncode(info.Reason.Replace("'", "‘").Replace("\"", "”"));
                #endregion
                if (!string.IsNullOrEmpty(model.UserCredentials))
                {
                    var certPics = model.UserCredentials.Split(',');
                    switch (certPics.Length)
                    {
                        case 1:
                            info.CertPic1 = MoveImages(certPics[0], CurrentUser.Id, info.OrderItemId);
                            break;
                        case 2:
                            info.CertPic1 = MoveImages(certPics[0], CurrentUser.Id, info.OrderItemId);
                            info.CertPic2 = MoveImages(certPics[1], CurrentUser.Id, info.OrderItemId);
                            break;
                        case 3:
                            info.CertPic1 = MoveImages(certPics[0], CurrentUser.Id, info.OrderItemId);
                            info.CertPic2 = MoveImages(certPics[1], CurrentUser.Id, info.OrderItemId);
                            info.CertPic3 = MoveImages(certPics[2], CurrentUser.Id, info.OrderItemId);
                            break;
                    }
                }
                //info.RefundAccount = HTMLEncode(info.RefundAccount.Replace("'", "‘").Replace("\"", "”"));

                if (info.Id > 0)
                {
                    RefundApplication.ActiveRefund(info);
                }
                else
                {
                    RefundApplication.AddOrderRefund(info);
                }
                return Json(new { msg = "成功的申请了售后" });
            }
            catch (MallException he)
            {
                return Json(ErrorResult<int>(he.Message));
            }
            catch (Exception ex)
            {
                Log.Error("[SPAPI]Refund：" + ex.Message);
                return Json(ErrorResult<int>("系统内部异常"));
            }
        }
        private string MoveImages(string image, long userId, long itemid)
        {
            if (!string.IsNullOrWhiteSpace(image))
            {
                var ext = Path.GetFileName(image);
                string ImageDir = string.Empty;
                //转移图片
                string relativeDir = "/Storage/Plat/Refund/" + userId.ToString() + "/";
                string fileName = itemid.ToString() + "_" + DateTime.Now.ToString("yyMMddHHmmssffff") + ext;
                if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
                {
                    var de = image.Substring(image.LastIndexOf("/temp/"));
                    Core.MallIO.CopyFile(de, relativeDir + fileName, true);
                    return relativeDir + fileName;
                }  //目标地址
                else if (image.Contains("/Storage/"))
                {
                    return image.Substring(image.LastIndexOf("/Storage/"));
                }

                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="context"></param>
        [HttpPost("PostUploadAppletImage")]
        public object PostUploadAppletImage(int orientation = 0)
        {
            CheckUserLogin();
            IList<string> images = new List<string>();
            IFormFileCollection files = HttpContext.Request.Form.Files;
            if (files != null)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    IFormFile file = files[i];

                    string filename = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ".png";
                    var fname = "/temp/" + filename;
                    var ioname = Core.MallIO.GetImagePath(fname);
                    try
                    {

                        /*
                        System.Drawing.Bitmap bitImg = new System.Drawing.Bitmap(100, 100);
                        bitImg = (System.Drawing.Bitmap)System.Drawing.Image.FromStream(file.InputStream);
                        //bitImg = ResourcesHelper.GetThumbnail(bitImg, 735, 480); //处理成对应尺寸图片
                        //iphone图片旋转
                        switch (orientation)
                        {
                            case 3: bitImg.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone); break;
                            case 6: bitImg.RotateFlip(System.Drawing.RotateFlipType.Rotate90FlipNone); break;
                            case 8: bitImg.RotateFlip(System.Drawing.RotateFlipType.Rotate270FlipNone); break;
                            default: break;

                                
                        }
                        var path = AppDomain.CurrentDomain.BaseDirectory + "/temp/";
                        bitImg.Save(Path.Combine(path, filename));
                        //Core.MallIO.CreateFile(fname, file.InputStream);
                        images.Add(ioname);*/

                        Stream stream = new MemoryStream();

                        file.CopyTo(stream);
                        Core.MallIO.CreateFile(fname, stream);
                        images.Add(ioname);

                    }
                    catch (Exception ex)
                    {
                        Log.Error("WeChatApplet_FileUpload_Error:" + ex.Message);
                        images.Add("upload error");
                    }
                }
            }
            return Json(new
            {
                Count = images.Count,
                Data = images.Select(c => new
                {
                    ImageUrl = c,
                })
            });
        }

        /// <summary>
        /// 发货
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// 
        [HttpPost("PostReturnSendGoods")]
        public object PostReturnSendGoods(RefundReturnSendGoodsPModel model)
        {
            CheckUserLogin();
            RefundApplication.UserConfirmRefundGood(model.ReturnsId, CurrentUser.UserName, model.express, model.shipOrderNumber);

            return Json(new { msg = "发货成功！" });
        }

        private string HTMLEncode(string txt)
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
        
    }
}
