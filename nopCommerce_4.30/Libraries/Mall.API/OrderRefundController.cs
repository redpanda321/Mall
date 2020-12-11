using Mall.API.Model;
using Mall.API.Model.ParamsModel;
using Mall.Application;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.Entities;
using Mall.Web.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace Mall.API
{
    public class OrderRefundController : BaseApiController
    {
        /// <summary>
        /// 获取申请售后的信息
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <param name="itemId">子订单ID</param>
        /// <returns></returns>
        /// 
        [HttpGet("GetOrderRefundModel")]
        public object GetOrderRefundModel(long id, long? itemId = null, long? refundId = null)
        {
            CheckUserLogin();
            try
            {
                dynamic d = new System.Dynamic.ExpandoObject();
                var ordser = ServiceProvider.Instance<IOrderService>.Create;
                var refundser = ServiceProvider.Instance<IRefundService>.Create;

                var order = ordser.GetOrder(id, CurrentUser.Id);
                if (order == null)
                    throw new Mall.Core.MallException("该订单已删除或不属于该用户");
                if (order.OrderType != OrderInfo.OrderTypes.Virtual && (int)order.OrderStatus < 2)
                    throw new Mall.Core.MallException("错误的售后申请,订单状态有误");
                if (order.OrderType != OrderInfo.OrderTypes.Virtual && itemId == null && order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitDelivery && order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                    throw new Mall.Core.MallException("错误的订单退款申请,订单状态有误");

                if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                {
                    //如果为虚拟商品，则要判断该商品是否允许退款，且该订单中是否至少有一个待核销的核销码
                    var orderItemInfo = OrderApplication.GetOrderItemsByOrderId(order.Id).FirstOrDefault();
                    if (orderItemInfo != null)
                    {
                        itemId = orderItemInfo.Id;
                        var virtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(orderItemInfo.ProductId);
                        if (virtualProductInfo != null)
                        {
                            if (virtualProductInfo.SupportRefundType == 3)
                            {
                                return new { success = false, msg = "该商品不支持退款" };
                            }
                            if (virtualProductInfo.SupportRefundType == 1 && DateTime.Now > virtualProductInfo.EndDate.Value)
                            {
                                return new { success = false, msg = "该商品不支持过期退款" };
                            }
                            var orderVerificationCodes = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(new List<long>() { order.Id });
                            long num = orderVerificationCodes.Where(a => a.Status == OrderInfo.VerificationCodeStatus.WaitVerification).Count();
                            if (num == 0)
                            {
                                return new { success = false, msg = "该商品没有可退的核销码" };
                            }
                            int validityType = 0; string startDate = string.Empty, endDate = string.Empty;
                            validityType = virtualProductInfo.ValidityType ? 1 : 0;
                            if (validityType == 1)
                            {
                                startDate = virtualProductInfo.StartDate.Value.ToString("yyyy-MM-dd");
                                endDate = virtualProductInfo.EndDate.Value.ToString("yyyy-MM-dd");
                            }
                            d.ValidityType = validityType;
                            d.StartDate = startDate;
                            d.EndDate = endDate;
                        }
                    }
                }


                //计算可退金额 预留
                ordser.CalculateOrderItemRefund(id);
                var orderitems = OrderApplication.GetOrderItems(order.Id);
                OrderRefundModel refundModel = new OrderRefundModel();
                var item = new OrderItemInfo();
                refundModel.MaxRGDNumber = 0;
                refundModel.MaxRefundAmount = order.OrderEnabledRefundAmount;
                if (itemId == null)
                {
                    item = orderitems.FirstOrDefault();
                }
                else
                {
                    item = orderitems.Where(a => a.Id == itemId).FirstOrDefault();
                    refundModel.MaxRGDNumber = item.Quantity - item.ReturnQuantity;
                    refundModel.MaxRefundAmount = (decimal)(item.EnabledRefundAmount - item.RefundPrice);
                }
                if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                {
                    var count = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(new List<long>() { order.Id }).Where(a => a.Status != OrderInfo.VerificationCodeStatus.WaitVerification).ToList().Count;
                    if (item.EnabledRefundAmount.HasValue)
                    {
                        decimal price = item.EnabledRefundAmount.Value / item.Quantity;
                        refundModel.MaxRefundAmount = item.EnabledRefundAmount.Value - Math.Round(count * price, 2, MidpointRounding.AwayFromZero);
                    }
                }
                bool isCanApply = false;
                if (refundModel.MaxRefundAmount <= 0)
                {
                    return new { success = false, msg = "此为优惠券全额抵扣订单不支持退款" };
                }
                if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery)
                {
                    isCanApply = refundser.CanApplyRefund(id, item.Id);
                }
                else
                {
                    isCanApply = refundser.CanApplyRefund(id, item.Id, false);
                }

                if (!refundId.HasValue)
                {
                    if (!isCanApply)
                        throw new Mall.Core.MallException("您已申请过售后，不可重复申请");
                    d.ContactPerson = string.IsNullOrEmpty(order.ShipTo) ? CurrentUser.RealName : order.ShipTo;
                    d.ContactCellPhone = string.IsNullOrEmpty(order.CellPhone) ? CurrentUser.CellPhone : order.CellPhone;
                    d.OrderItemId = itemId;
                    d.RefundType = 0;
                    d.IsRefundOrder = false;
                    if (!itemId.HasValue)
                    {
                        d.IsRefundOrder = true;
                        d.RefundType = 1;
                    }
                    var reasonlist = refundser.GetRefundReasons();
                    d.Id = order.Id;
                    d.MaxRGDNumber = refundModel.MaxRGDNumber;
                    d.MaxRefundAmount = refundModel.MaxRefundAmount;
                    d.OrderStatus = order.OrderStatus.ToDescription();
                    d.OrderStatusValue = (int)order.OrderStatus;
                    d.BackOut = false;
                    d.RefundReasons = reasonlist;
                    if (order.CanBackOut())
                        d.BackOut = true;
                }
                else
                {
                    var refunddata = refundser.GetOrderRefund(refundId.Value, CurrentUser.Id);
                    if (refunddata == null)
                    {
                        throw new Mall.Core.MallException("错误的售后数据");
                    }
                    if (order.OrderType != OrderInfo.OrderTypes.Virtual && refunddata.SellerAuditStatus != Entities.OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
                    {
                        throw new Mall.Core.MallException("错误的售后状态，不可激活");
                    }
                    d.ContactPerson = refunddata.ContactPerson;
                    d.ContactCellPhone = refunddata.ContactCellPhone;
                    d.OrderItemId = refunddata.OrderItemId;
                    d.IsRefundOrder = (refunddata.RefundMode == Entities.OrderRefundInfo.OrderRefundMode.OrderRefund);
                    d.RefundType = (refunddata.RefundMode == Entities.OrderRefundInfo.OrderRefundMode.OrderRefund ? 1 : 0);
                    var reasonlist = refundser.GetRefundReasons();
                    d.RefundReasons = reasonlist;    //理由List
                    d.Id = id;
                    d.MaxRGDNumber = refundModel.MaxRGDNumber;
                    d.MaxRefundAmount = refundModel.MaxRefundAmount;
                    d.OrderStatus = order.OrderStatus.ToDescription();
                    d.OrderStatusValue = (int)order.OrderStatus;
                    d.BackOut = false;
                    d.ReasonDetail = refunddata.ReasonDetail;
                    d.CertPic1 = Mall.Core.MallIO.GetRomoteImagePath(refunddata.CertPic1);
                    d.CertPic2 = Mall.Core.MallIO.GetRomoteImagePath(refunddata.CertPic2);
                    d.CertPic3 = Mall.Core.MallIO.GetRomoteImagePath(refunddata.CertPic3);
                    if (order.CanBackOut())
                        d.BackOut = true;
                }

                if (!d.IsRefundOrder && item.EnabledRefundAmount.HasValue)
                {
                    d.RefundGoodsPrice = item.EnabledRefundAmount.Value / item.Quantity;
                }
                d.DeliveryType = order.DeliveryType;
                if (order.DeliveryType == CommonModel.DeliveryType.SelfTake)
                {
                    var shopBranch = ShopBranchApplication.GetShopBranchById(order.ShopBranchId);
                    d.ReturnGoodsAddress = RegionApplication.GetFullName(shopBranch.AddressId);
                    d.ReturnGoodsAddress += " " + shopBranch.AddressDetail;
                    d.ReturnGoodsAddress += " " + shopBranch.ContactPhone;
                }

                d.IsVirtual = order.OrderType == OrderInfo.OrderTypes.Virtual ? 1 : 0;
                if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                {
                    var codes = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(new List<long>() { order.Id }).Where(a => a.Status == OrderInfo.VerificationCodeStatus.WaitVerification).ToList();
                    d.OrderVerificationCode = codes.Select(a => a.VerificationCode).Select(p => p = Regex.Replace(p, @"(\d{4})", "$1 "));
                }

                return new { success = true, RefundMode = d };
            }
            catch (MallException Mallex)
            {
                return ErrorResult(Mallex.Message);
            }
            catch (Exception ex)
            {
                return ErrorResult("系统异常：" + ex.Message);
            }
        }

        /// <summary>
        /// 提交退款/售后申请
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
        [HttpGet("PostRefundApply")]
        public object PostRefundApply(OrderRefundApplyModel value)
        {
            CheckUserLogin();
            try
            {
                var ordser = ServiceProvider.Instance<IOrderService>.Create;
                var refundser = ServiceProvider.Instance<IRefundService>.Create;
                OrderRefundInfo info = new OrderRefundInfo();
                #region 表单数据
                info.OrderId = value.OrderId;
                if (null != value.OrderItemId)
                    info.OrderItemId = value.OrderItemId.Value;
                if (null != value.refundId)
                    info.Id = value.refundId.Value;
                info.RefundType = value.RefundType;
                info.ReturnQuantity = value.ReturnQuantity;
                info.Amount = value.Amount;
                info.Reason = value.Reason;
                info.ContactPerson = value.ContactPerson;
                info.ContactCellPhone = value.ContactCellPhone;
                info.RefundPayType = value.RefundPayType;
                info.VerificationCodeIds = value.VerificationCodeIds;

                if (info.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund && !value.refundId.HasValue && value.OrderItemId.HasValue)
                {
                    var _refobj = OrderApplication.GetOrderRefunds(new long[] { value.OrderItemId.Value }).FirstOrDefault(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                    if (_refobj != null)
                    {
                        info.Id = _refobj.Id;
                    }
                }
                #endregion

                #region 初始化售后单的数据
                var order = ordser.GetOrder(info.OrderId, CurrentUser.Id);
                if (order == null) throw new Mall.Core.MallException("该订单已删除或不属于该用户");
                if (order.OrderType != OrderInfo.OrderTypes.Virtual && (int)order.OrderStatus < 2) throw new Mall.Core.MallException("错误的售后申请,订单状态有误");
                if (value.ReasonDetail != null && value.ReasonDetail.Length > 1000)
                    throw new Mall.Core.MallException("退款理由不能超过1000字符");
                if (order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                {
                    info.RefundMode = OrderRefundInfo.OrderRefundMode.OrderRefund;
                    info.ReturnQuantity = 0;
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
                info.ReasonDetail = value.ReasonDetail;
                info.CertPic1 = MoveImages(value.CertPic1, CurrentUser.Id, info.OrderItemId);
                info.CertPic2 = MoveImages(value.CertPic2, CurrentUser.Id, info.OrderItemId);
                info.CertPic3 = MoveImages(value.CertPic3, CurrentUser.Id, info.OrderItemId);
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
                        refundser.ActiveRefund(info);
                    }
                    else
                    {
                        refundser.AddOrderRefund(info);
                    }
                }
                else
                {
                    refundser.AddOrderRefund(info);
                    #region 处理退款
                    try
                    {
                        //虚拟订单自动退款，异常不提示用户,进入平台待审核
                        string notifyurl = CurrentUrlHelper.CurrentUrlNoPort() + "/Pay/RefundNotify/{0}";
                        var result = refundser.ConfirmRefund(info.Id, "虚拟订单申请售后自动退款", "", notifyurl);
                    }
                    catch(Exception ex)
                    {
                        Log.Error("虚拟商品自动退异常", ex);
                    }
                    #endregion
                }
                return new { success = true, msg = "提交成功", id = info.Id };
            }
            catch (MallException he)
            {
                return new { success = false, msg = he.Message };
            }
            catch (Exception ex)
            {
                return new { success = false, msg = "系统异常：" + ex.Message };
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
        /// 获取退款/售后列表
        /// </summary>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns></returns>
        /// 
        [HttpGet("GetRefundList")]
        public object GetRefundList(int pageNo, int pageSize)
        {
            CheckUserLogin();
            var orderser = ServiceProvider.Instance<IOrderService>.Create;
            var refundser = ServiceProvider.Instance<IRefundService>.Create;
            var vshopser = ServiceProvider.Instance<IVShopService>.Create;

            DateTime? startDate = null;
            DateTime? endDate = null;
            var queryModel = new RefundQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                UserId = CurrentUser.Id,
                PageSize = pageSize,
                PageNo = pageNo,
                ShowRefundType = 0
            };
            var refunds = refundser.GetOrderRefunds(queryModel);
            var list = refunds.Models.Select(item =>
            {
                var vshop = vshopser.GetVShopByShopId(item.ShopId) ?? new VShopInfo() { Id = 0 };
                var order = orderser.GetOrder(item.OrderId, CurrentUser.Id);
                var orderItem = OrderApplication.GetOrderItem(item.OrderItemId);
                var status = item.RefundStatus;
                if (order != null && order.ShopBranchId > 0)
                {
                    if (!string.IsNullOrWhiteSpace(status))
                    {
                        status = status.Replace("商家", "门店");
                    }
                }

                IEnumerable<Entities.OrderItemInfo> orderItems = null;
                if (item.RefundMode == Entities.OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    var cOrder = orderser.GetOrder(item.OrderId, CurrentUser.Id);
                    orderItems = OrderApplication.GetOrderItems(cOrder.Id);
                }
                var shopBranch = ShopBranchApplication.GetShopBranchById(order.ShopBranchId);
                var branchName = shopBranch == null ? "" : shopBranch.ShopBranchName;
                return new
                {
                    ShopName = item.ShopName,
                    Vshopid = vshop.Id,
                    ShopBranchId = order.ShopBranchId,
                    ShopBranchName = branchName,
                    RefundStatus = status,
                    Id = item.Id,
                    ProductName = orderItem.ProductName,
                    ColorAlias = orderItem.ColorAlias,
                    Color = orderItem.Color,
                    SizeAlias = orderItem.SizeAlias,
                    Size = orderItem.Size,
                    VersionAlias = orderItem.VersionAlias,
                    Version = orderItem.Version,
                    EnabledRefundAmount = item.EnabledRefundAmount,
                    Amount = item.Amount,
                    Img = Core.MallIO.GetRomoteProductSizeImage(orderItem.ThumbnailsUrl, 1, (int)Mall.CommonModel.ImageSize.Size_350),
                    ShopId = item.ShopId,
                    RefundMode = item.RefundMode.ToDescription(),
                    RefundModeValue = (int)item.RefundMode,
                    OrderId = item.OrderId,
                    OrderTotal = order.OrderTotalAmount.ToString("f2"),
                    OrderItems = orderItems != null ? orderItems.Select(e => new
                    {
                        ThumbnailsUrl = Core.MallIO.GetRomoteProductSizeImage(e.ThumbnailsUrl, 1, (int)Mall.CommonModel.ImageSize.Size_350),
                        ProductName = e.ProductName,
                        ColorAlias = e.ColorAlias,
                        Color = e.Color,
                        SizeAlias = e.SizeAlias,
                        Size = e.Size,
                        VersionAlias = e.VersionAlias,
                        Version = e.Version
                    }) : null,
                    SellerAuditStatus = item.SellerAuditStatus.ToDescription(),
                    SellerAuditStatusValue = (int)item.SellerAuditStatus,
                };
            });

            return new { total = refunds.Total, data = list, success = true };
        }

        /// <summary>
        /// 获取 退款/售后 详情
        /// </summary>
        /// <param name="id">退款/售后ID</param>
        /// <returns></returns>
        /// 
        [HttpGet("GetRefundDetail")]
        public object GetRefundDetail(long id)
        {
            CheckUserLogin();
            var _iOrderService = ServiceProvider.Instance<IOrderService>.Create;
            var refundser = ServiceProvider.Instance<IRefundService>.Create;
            var refundinfo = refundser.GetOrderRefund(id, CurrentUser.Id);
            if (refundinfo == null)
            {
                return new { success = false, msg = "售后单不存在或不属于该用户" };
            }
            var order = _iOrderService.GetOrder(refundinfo.OrderId, CurrentUser.Id);
            var shopBranch = ShopBranchApplication.GetShopBranchById(order.ShopBranchId);
            var branchName = shopBranch == null ? "" : shopBranch.ShopBranchName;
            string refundStatus = refundinfo.RefundStatus;
            dynamic d = SuccessResult();
            d.ManagerConfirmStatus = refundinfo.ManagerConfirmStatus.ToDescription();
            d.ManagerConfirmStatusValue = (int)refundinfo.ManagerConfirmStatus;
            d.ManagerConfirmDate = refundinfo.ManagerConfirmDate;
            d.SellerAuditStatus = refundinfo.SellerAuditStatus.ToDescription();
            d.SellerAuditStatusValue = (int)refundinfo.SellerAuditStatus;
            d.SellerRemark = refundinfo.SellerRemark;
            d.SellerAuditDate = refundinfo.SellerAuditDate;
            d.Amount = refundinfo.Amount;
            d.Id = refundinfo.Id;
            d.OrderId = refundinfo.OrderId;
            d.OrderItemId = refundinfo.OrderItemId;
            d.ShopName = string.IsNullOrEmpty(branchName) ? refundinfo.ShopName : branchName;
            d.RefundMode = refundinfo.RefundMode.ToDescription();
            d.RefundModeValue = (int)refundinfo.RefundMode;
            d.ReturnQuantity = refundinfo.ReturnQuantity;
            d.RefundPayType = refundinfo.RefundPayType.ToDescription();
            d.RefundPayTypeValue = (int)refundinfo.RefundPayType;
            d.Reason = refundinfo.Reason;
            d.ApplyDate = refundinfo.ApplyDate;
            d.IsOrderRefundTimeOut = _iOrderService.IsRefundTimeOut(refundinfo.OrderId);
            d.ReasonDetail = refundinfo.ReasonDetail;
            d.CertPic1 = Mall.Core.MallIO.GetRomoteImagePath(refundinfo.CertPic1);
            d.CertPic2 = Mall.Core.MallIO.GetRomoteImagePath(refundinfo.CertPic2);
            d.CertPic3 = Mall.Core.MallIO.GetRomoteImagePath(refundinfo.CertPic3);
            d.LastConfirmDate = (refundinfo.ManagerConfirmDate > refundinfo.SellerAuditDate ? refundinfo.ManagerConfirmDate : refundinfo.SellerAuditDate);
            if ((refundinfo.RefundMode != Entities.OrderRefundInfo.OrderRefundMode.OrderRefund) || (refundinfo.RefundMode == Entities.OrderRefundInfo.OrderRefundMode.OrderRefund && order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitDelivery))
                d.ResetActive = true;
            else
                d.ResetActive = false;
            if (order != null && order.ShopBranchId > 0)
            {
                if (!string.IsNullOrWhiteSpace(refundStatus))
                {
                    refundStatus = refundStatus.Replace("商家", "门店");
                }
            }
            d.RefundStatus = refundStatus;
            d.ContactPerson = refundinfo.ContactPerson;
            d.ContactCellPhone = refundinfo.ContactCellPhone;
            d.OrderType = order.OrderType;
            if (order.OrderType == OrderInfo.OrderTypes.Virtual)
            {
                var list = refundinfo.VerificationCodeIds.Split(',').ToList();
                if (list.Count > 0)
                {
                    d.VerificationCodeIds = list.Select(a => a = Regex.Replace(a, @"(\d{4})", "$1 ")).ToList();
                }
            }
            return d;
        }

        /// <summary>
        /// 获取 退款/售后进程 详情
        /// </summary>
        /// <param name="id">退款/售后ID</param>
        /// <returns></returns>
        /// 
        [HttpGet("GetRefundProcessDetail")]
        public object GetRefundProcessDetail(long id)
        {
            CheckUserLogin();
            var refundser = ServiceProvider.Instance<IRefundService>.Create;
            var refundinfo = refundser.GetOrderRefund(id, CurrentUser.Id);
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

            dynamic d = SuccessResult();
            d.IsDiscard = isDiscard;
            d.IsUnAudit = isUnAudit;
            d.IsReturnGoods = isReturnGoods;

            d.ManagerConfirmStatus = refundinfo.ManagerConfirmStatus.ToDescription();
            d.ManagerConfirmStatusValue = (int)refundinfo.ManagerConfirmStatus;
            d.ManagerConfirmDate = refundinfo.ManagerConfirmDate;
            d.ManagerRemark = refundinfo.ManagerRemark;

            d.SellerAuditStatus = refundinfo.SellerAuditStatus.ToDescription();
            d.SellerAuditStatusValue = (int)refundinfo.SellerAuditStatus;
            d.SellerConfirmArrivalDate = refundinfo.SellerConfirmArrivalDate;
            d.SellerRemark = refundinfo.SellerRemark;
            d.SellerAuditDate = refundinfo.SellerAuditDate;

            d.BuyerDeliverDate = refundinfo.BuyerDeliverDate;
            d.ExpressCompanyName = refundinfo.ExpressCompanyName;
            d.ShipOrderNumber = refundinfo.ShipOrderNumber;
            d.ApplyDate = refundinfo.ApplyDate;

            int curappnum = refundinfo.ApplyNumber;
            var log = refundser.GetRefundLogs(refundinfo.Id, curappnum, false);
            d.RefundLogs = log;

            d.ContactPerson = refundinfo.ContactPerson;
            d.ContactCellPhone = refundinfo.ContactCellPhone;
            return d;
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


        [HttpPost("PostSellerSendGoods")]
        public object PostSellerSendGoods(OrderRefundSellerSendGoodsModel value)
        {
            CheckUserLogin();
            var refundser = ServiceProvider.Instance<IRefundService>.Create;
            long id = value.Id;
            string expressCompanyName = value.ExpressCompanyName;
            string shipOrderNumber = value.ShipOrderNumber;
            refundser.UserConfirmRefundGood(id, CurrentUser.UserName, expressCompanyName, shipOrderNumber);
            return SuccessResult("提交成功");
        }
        [HttpGet("GetShopGetAddress")]
        public object GetShopGetAddress(long shopId, long shopBranchId = 0)
        {
            if (shopBranchId <= 0)
            {
                var data = ShopShippersApplication.GetDefaultGetGoodsShipper(shopId);
                if (data == null)
                {
                    data = new DTO.ShopShipper() { };
                }
                else
                {
                    data.RegionStr = RegionApplication.GetFullName(data.RegionId);
                }
                var model = new
                {
                    success = true,
                    Region = data.RegionStr,
                    Address = data.Address,
                    Phone = data.TelPhone,
                    ShipperName = data.ShipperName
                };
                return model;
            }
            else
            {
                var data = ShopBranchApplication.GetShopBranchById(shopBranchId);
                string redionstr = "";
                if (data != null)
                {
                    redionstr = RegionApplication.GetFullName(data.AddressId);
                }
                var model = new
                {
                    success = true,
                    Region = redionstr,
                    Address = data.AddressDetail,
                    Phone = data.ContactPhone,
                    ShipperName = data.ContactUser
                };
                return model;
            }
        }
    }
}
