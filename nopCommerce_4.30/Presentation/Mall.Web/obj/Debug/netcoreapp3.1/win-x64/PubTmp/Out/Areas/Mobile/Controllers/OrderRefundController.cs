using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Containers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;


namespace Mall.Web.Areas.Mobile.Controllers
{
    public class OrderRefundController : BaseMobileMemberController
    {
        private IOrderService _iOrderService;
        private IRefundService _iRefundService;
        private IShopService _iShopService;
        private IVShopService _iVShopService;
        public OrderRefundController(IOrderService iOrderService, IRefundService iRefundService, IShopService iShopService, IVShopService iVShopService)
        {
            _iOrderService = iOrderService;
            _iRefundService = iRefundService;
            _iShopService = iShopService;
            _iVShopService = iVShopService;
        }
        
        // GET: Web/OrderRefund
        /// <summary>
        /// 退款申请
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public ActionResult RefundApply(long orderid, long? itemId, long? refundid)
        {
            Mall.Web.Areas.Web.Models.RefundApplyModel model = new Mall.Web.Areas.Web.Models.RefundApplyModel();
            model.RefundMode = null;
            model.OrderItemId = null;
            var ordser = ServiceApplication.Create<IOrderService>();
            var order = ordser.GetOrder(orderid, CurrentUser.Id);
            string errormsg = "";
            string jumpurl = "/" + ViewBag.AreaName + "/Member/Center";
            bool isok = true;
            VirtualProductInfo vProductInfo = null;
            if (isok)
            {
                if (order == null)
                {
                    isok = false;
                    errormsg = "该订单已删除或不属于该用户";
                    return Redirect(jumpurl);
                    throw new Mall.Core.MallException("该订单已删除或不属于该用户");
                }
            }

            if (isok)
            {
                if (order.OrderType != OrderInfo.OrderTypes.Virtual && (int)order.OrderStatus < 2)
                {
                    isok = false;
                    errormsg = "错误的售后申请,订单状态有误";
                    return Redirect(jumpurl);
                    throw new Mall.Core.MallException("错误的售后申请,订单状态有误");
                }
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
                                isok = false;
                                errormsg = "该商品不支持退款";
                                return Redirect(jumpurl);
                                throw new Mall.Core.MallException("该商品不支持退款");
                            }
                            if (virtualProductInfo.SupportRefundType == 1 && DateTime.Now > virtualProductInfo.EndDate.Value)
                            {
                                throw new Mall.Core.MallException("该商品不支持过期退款");
                            }
                            var orderVerificationCodes = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(new List<long>() { order.Id });
                            long num = orderVerificationCodes.Where(a => a.Status == OrderInfo.VerificationCodeStatus.WaitVerification).Count();
                            if (num == 0)
                            {
                                isok = false;
                                errormsg = "该商品没有可退的核销码";
                                return Redirect(jumpurl);
                                throw new Mall.Core.MallException("该商品没有可退的核销码");
                            }
                            vProductInfo = virtualProductInfo;
                        }
                    }
                }
            }

            if (isok)
            {
                if (order.OrderType != OrderInfo.OrderTypes.Virtual)
                {
                    if (itemId == null && order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitDelivery && order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                    {
                        isok = false;
                        errormsg = "错误的订单退款申请,订单状态有误";
                        return Redirect(jumpurl);
                        throw new Mall.Core.MallException("错误的订单退款申请,订单状态有误");
                    }
                }
            }

            if (isok)
            {
                if (order.OrderType != OrderInfo.OrderTypes.Virtual)
                {
                    //售后时间限制
                    if (_iOrderService.IsRefundTimeOut(orderid))
                    {
                        isok = false;
                        errormsg = "订单已超过售后期";
                        return Redirect(jumpurl);
                        throw new Mall.Core.MallException("订单已超过售后期");
                    }
                }
            }

            if (isok)
            {
                //计算可退金额 预留
                ordser.CalculateOrderItemRefund(orderid);

                var item = new OrderItemInfo();
                model.MaxRefundGoodsNumber = 0;
                model.MaxRefundAmount = order.OrderEnabledRefundAmount;
                if (itemId == null)
                {
                    model.OrderItems = _iOrderService.GetOrderItemsByOrderId(order.Id);
                }
                else
                {
                    item = _iOrderService.GetOrderItem(itemId.Value);
                    model.OrderItems.Add(item);
                    model.MaxRefundGoodsNumber = item.Quantity - item.ReturnQuantity;
                    model.MaxRefundAmount = item.EnabledRefundAmount - item.RefundPrice;
                }
                if (!model.MaxRefundAmount.HasValue)
                {
                    model.MaxRefundAmount = 0;
                }
                if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                {
                    var count = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(new List<long>() { order.Id }).Where(a => a.Status != OrderInfo.VerificationCodeStatus.WaitVerification).ToList().Count;
                    if (item.EnabledRefundAmount.HasValue)
                    {
                        decimal price = item.EnabledRefundAmount.Value / item.Quantity;
                        model.MaxRefundAmount = item.EnabledRefundAmount.Value - Math.Round(count * price, 2, MidpointRounding.AwayFromZero);
                    }
                }
                bool isCanApply = false;
                var refundser = _iRefundService;
                Entities.OrderRefundInfo refunddata;

                if (order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitDelivery)
                {
                    isCanApply = refundser.CanApplyRefund(orderid, item.Id);
                }
                else
                {
                    isCanApply = refundser.CanApplyRefund(orderid, item.Id, false);
                }
                if (!refundid.HasValue)
                {
                    if (!isCanApply)
                    {
                        isok = false;
                        errormsg = "您已申请过售后，不可重复申请";
                        return Redirect(jumpurl);
                        throw new Mall.Core.MallException("您已申请过售后，不可重复申请");
                    }

                    //model.ContactPerson = CurrentUser.RealName;
                    //model.ContactCellPhone = CurrentUser.CellPhone;
                    // model.ContactCellPhone = order.CellPhone;

                    model.ContactPerson = string.IsNullOrEmpty(order.ShipTo) ? CurrentUser.RealName : order.ShipTo;
                    model.ContactCellPhone = string.IsNullOrEmpty(order.CellPhone) ? CurrentUser.CellPhone : order.CellPhone;

                    model.OrderItemId = itemId;
                    if (!model.OrderItemId.HasValue)
                    {
                        model.IsOrderAllRefund = true;
                        model.RefundMode = Entities.OrderRefundInfo.OrderRefundMode.OrderRefund;
                    }
                }
                else
                {
                    refunddata = refundser.GetOrderRefund(refundid.Value, CurrentUser.Id);
                    if (refunddata == null)
                    {
                        isok = false;
                        errormsg = "错误的售后数据";
                        return Redirect(jumpurl);
                        throw new Mall.Core.MallException("错误的售后数据");
                    }
                    if (isok)
                    {
                        if (order.OrderType != OrderInfo.OrderTypes.Virtual)
                        {
                            if (refunddata.SellerAuditStatus != Entities.OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
                            {
                                isok = false;
                                errormsg = "错误的售后状态，不可激活";
                                return Redirect(jumpurl);
                                throw new Mall.Core.MallException("错误的售后状态，不可激活");
                            }
                        }
                    }
                    if (isok)
                    {
                        model.ContactPerson = refunddata.ContactPerson;
                        model.ContactCellPhone = refunddata.ContactCellPhone;
                        model.OrderItemId = refunddata.OrderItemId;
                        model.IsOrderAllRefund = (refunddata.RefundMode == Entities.OrderRefundInfo.OrderRefundMode.OrderRefund);
                        model.RefundMode = refunddata.RefundMode;
                        model.RefundReasonValue = refunddata.Reason;
                        model.RefundWayValue = refunddata.RefundPayType;
                        model.CertPic1 = refunddata.CertPic1;
                        model.CertPic2 = refunddata.CertPic2;
                        model.CertPic3 = refunddata.CertPic3;
                    }
                }
                if (!model.IsOrderAllRefund && item.EnabledRefundAmount.HasValue)
                {
                    model.RefundGoodsPrice = item.EnabledRefundAmount.Value / item.Quantity;
                }

                if (isok)
                {
                    model.OrderInfo = order;
                    model.OrderId = orderid;
                    model.RefundId = refundid;

                    var reasons = refundser.GetRefundReasons();
                    foreach (var _ir in reasons)
                    {
                        _ir.AfterSalesText = _ir.AfterSalesText.Trim();
                    }
                    List<SelectListItem> reasel = new List<SelectListItem>();
                    SelectListItem _tmpsel;
                    _tmpsel = new SelectListItem { Text = "选择售后理由", Value = "" };
                    //reasel.Add(_tmpsel);
                    foreach (var _i in reasons)
                    {
                        _tmpsel = new SelectListItem { Text = _i.AfterSalesText, Value = _i.AfterSalesText };
                        if (!string.IsNullOrWhiteSpace(model.RefundReasonValue))
                        {
                            if (_i.AfterSalesText == model.RefundReasonValue)
                            {
                                _tmpsel.Selected = true;
                            }
                        }
                        reasel.Add(_tmpsel);
                    }
                    model.RefundReasons = reasel;

                    List<SelectListItem> list = new List<SelectListItem> {
                        new SelectListItem{
                            Text=OrderRefundInfo.OrderRefundPayType.BackCapital.ToDescription(),
                            Value=((int)OrderRefundInfo.OrderRefundPayType.BackCapital).ToString()
                        }
                    };
                    if (order.CanBackOut())
                    {
                        _tmpsel = new SelectListItem
                        {
                            Text = OrderRefundInfo.OrderRefundPayType.BackOut.ToDescription(),
                            Value = ((int)OrderRefundInfo.OrderRefundPayType.BackOut).ToString()
                        };
                        //if (model.RefundWayValue.HasValue)
                        //{
                        //    if (_tmpsel.Value == model.RefundWayValue.ToString())
                        //    {
                        //        _tmpsel.Selected = true;
                        //    }
                        //}
                        _tmpsel.Selected = true;  //若订单支付方式为支付宝、微信支付则退款方式默认选中“退款原路返回”
                        list.Add(_tmpsel);
                        model.BackOut = 1;
                    }
                    model.RefundWay = list;
                }
                if (order.DeliveryType == DeliveryType.SelfTake)
                {
                    var shopBranch = ShopBranchApplication.GetShopBranchById(order.ShopBranchId);
                    model.ReturnGoodsAddress = RegionApplication.GetFullName(shopBranch.AddressId);
                    model.ReturnGoodsAddress += " " + shopBranch.AddressDetail;
                    model.ReturnGoodsAddress += " " + shopBranch.ContactPhone;
                }
            }
            ViewBag.errormsg = errormsg;
            #region 虚拟订单退款
            ViewBag.orderVerificationCode = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(new List<long>() { order.Id }).Where(a => a.Status == OrderInfo.VerificationCodeStatus.WaitVerification).ToList();
            #endregion
            ViewBag.IsVirtual = order.OrderType == OrderInfo.OrderTypes.Virtual ? 1 : 0;
            ViewBag.VirtualProductInfo = vProductInfo;
            return View(model);
        }
        /// <summary>
        /// 退款申请处理
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
      
        [HttpPost]
        public JsonResult RefundApply(OrderRefundInfo info)
        {
            if (info.ReasonDetail != null && info.ReasonDetail.Length > 1000)
                throw new Mall.Core.MallException("退款说明不能超过1000字符");
            var order = _iOrderService.GetOrder(info.OrderId, CurrentUser.Id);
            if (order == null) throw new Mall.Core.MallException("该订单已删除或不属于该用户");
            if (order.OrderType != OrderInfo.OrderTypes.Virtual && (int)order.OrderStatus < 2) throw new Mall.Core.MallException("错误的售后申请,订单状态有误");
            //售后时间限制
            if (order.OrderType != OrderInfo.OrderTypes.Virtual && _iOrderService.IsRefundTimeOut(info.OrderId))
            {
                throw new Mall.Core.MallException("订单已超过售后期");
            }
            if (order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitSelfPickUp)
            {
                info.RefundMode = OrderRefundInfo.OrderRefundMode.OrderRefund;
                info.ReturnQuantity = 0;
            }
            if (info.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund && info.Id == 0)
            {
                var _refobj = OrderApplication.GetOrderRefunds(new long[] { info.OrderItemId }).FirstOrDefault(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                if (_refobj != null)
                {
                    info.Id = _refobj.Id;
                }
            }
            if (info.RefundType == 1)
            {
                info.ReturnQuantity = 0;
                info.IsReturn = false;
            }
            if (order.OrderType != OrderInfo.OrderTypes.Virtual && info.ReturnQuantity < 0)
                throw new MallException("错误的退货数量");
            var orderitem = _iOrderService.GetOrderItem(info.OrderItemId);
            if (orderitem == null && info.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund)
                throw new Mall.Core.MallException("该订单条目已删除或不属于该用户");

            if (order.OrderType != OrderInfo.OrderTypes.Virtual)
            {
                if (info.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    if (order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitDelivery && order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                        throw new Mall.Core.MallException("错误的订单退款申请,订单状态有误");
                    info.IsReturn = false;
                    info.ReturnQuantity = 0;
                    if (info.Amount > order.OrderEnabledRefundAmount)
                        throw new Mall.Core.MallException("退款金额不能超过订单的实际支付金额");
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
            info.ReasonDetail = HttpUtility.HtmlEncode(info.ReasonDetail);
            info.Reason = HTMLEncode(info.Reason.Replace("'", "‘").Replace("\"", "”"));
            if (!info.IsWxUpload)
            {
                info.CertPic1 = MoveImages(info.CertPic1, CurrentUser.Id, info.OrderItemId);
                info.CertPic2 = MoveImages(info.CertPic2, CurrentUser.Id, info.OrderItemId);
                info.CertPic3 = MoveImages(info.CertPic3, CurrentUser.Id, info.OrderItemId);
            }
            else
            {
                info.CertPic1 = DownloadWxImage(info.CertPic1, CurrentUser.Id, info.OrderItemId);
                info.CertPic2 = DownloadWxImage(info.CertPic2, CurrentUser.Id, info.OrderItemId);
                info.CertPic3 = DownloadWxImage(info.CertPic3, CurrentUser.Id, info.OrderItemId);
            }
            if (order.OrderType == OrderInfo.OrderTypes.Virtual)
            {
                if (string.IsNullOrWhiteSpace(info.VerificationCodeIds))
                    throw new Mall.Core.MallException("虚拟订单退款核销码不能为空");

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
            //info.RefundAccount = HTMLEncode(info.RefundAccount.Replace("'", "‘").Replace("\"", "”"));
            if (order.OrderType != OrderInfo.OrderTypes.Virtual)
            {
                if (info.Id > 0)
                {
                    _iRefundService.ActiveRefund(info);
                }
                else
                {
                    _iRefundService.AddOrderRefund(info);
                }
            }
            else
            {
                _iRefundService.AddOrderRefund(info);
                #region 处理退款
                try
                {
                    //虚拟订单自动退款，异常不提示用户,进入平台待审核
                    //获取异步通知地址
                    string notifyurl = CurrentUrlHelper.CurrentUrlNoPort() + "/Pay/RefundNotify/{0}";
                    var result = _iRefundService.ConfirmRefund(info.Id, "虚拟订单申请售后自动退款", "", notifyurl);
                }
                catch(MallException ex)
                {
                    Log.Error("虚拟商品自动退异常", ex);
                }
                #endregion
            }
            return SuccessResult<dynamic>(msg: "提交成功", data: info.Id);
        }

        /// <summary>
        /// 显示售后记录
        /// </summary>
        /// <param name="applyDate"></param>
        /// <param name="auditStatus"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="showtype">0 所有 1 订单退款 2 仅退款(包含订单退款) 3 退货 4 仅退款</param>
        /// <returns></returns>
        public JsonResult List(int pageNo = 1, int pageSize = 10)
        {
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
            var refunds = _iRefundService.GetOrderRefunds(queryModel);
            var list = refunds.Models.Select(item =>
            {
                var vshop = _iVShopService.GetVShopByShopId(item.ShopId) ?? new Entities.VShopInfo() { Id = 0 };
                bool IsSelfTake = false;
                var order = _iOrderService.GetOrder(item.OrderId, CurrentUser.Id);
                if (order.DeliveryType == CommonModel.DeliveryType.SelfTake)
                {
                    IsSelfTake = true;
                }
                var status = string.Empty;
                if (IsSelfTake || order.ShopBranchId > 0)//分配门店订单与自提订单一致
                {
                    status = item.RefundStatus.Replace("商家", "门店");
                }
                var shopBranch = ShopBranchApplication.GetShopBranchById(order.ShopBranchId);
                var branchName = shopBranch == null ? "" : shopBranch.ShopBranchName;
                var orderItem = OrderApplication.GetOrderItem(item.OrderItemId);
                var orderItems = new List<Entities.OrderItemInfo>();
                if (item.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                    orderItems = _iOrderService.GetOrderItemsByOrderId(item.OrderId);
                else
                    orderItems.Add(orderItem);
                return new
                {
                    ShopName = item.ShopName,
                    Vshopid = vshop.Id,
                    ShopBranchId = order.ShopBranchId,
                    ShopBranchName = branchName,
                    RefundStatus = string.IsNullOrEmpty(status) ? item.RefundStatus : status,
                    Id = item.Id,
                    ProductName = orderItem.ProductName,
                    EnabledRefundAmount = item.EnabledRefundAmount,
                    Amount = item.Amount,
                    Img = MallIO.GetProductSizeImage(orderItem.ThumbnailsUrl, 1, (int)ImageSize.Size_100),
                    ShopId = item.ShopId,
                    RefundMode = item.RefundMode,
                    OrderId = item.OrderId,
                    OrderTotal = order.OrderTotalAmount.ToString("f2"),
                    OrderItems = orderItems != null ? orderItems.Select(e => new
                    {
                        ThumbnailsUrl = MallIO.GetProductSizeImage(e.ThumbnailsUrl, 1, (int)ImageSize.Size_100),
                        ProductName = e.ProductName,
                        SkuText = e.Color + " " + e.Size + " " + e.Version,
                    }) : null,
                    SellerAuditStatus = item.SellerAuditStatus
                };
            });
            return SuccessResult<dynamic>(data: list);
        }


        [HttpGet]
        public JsonResult GetShopInfo(long shopId)
        {
            var shopinfo = _iShopService.GetShop(shopId);
            var model = new { SenderAddress = shopinfo.SenderAddress, SenderPhone = shopinfo.SenderPhone, SenderName = shopinfo.SenderName };
            return Json(model);
        }

        [HttpPost]
        public JsonResult UpdateRefund(long id, string expressCompanyName, string shipOrderNumber)
        {
            _iRefundService.UserConfirmRefundGood(id, CurrentUser.UserName, expressCompanyName, shipOrderNumber);
            return SuccessResult<dynamic>(msg: "提交成功");
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


        public ActionResult RefundList()
        {
            return View();
        }
        public ActionResult RefundDetail(long id)
        {
            var refundinfo = _iRefundService.GetOrderRefund(id, CurrentUser.Id);
            ViewBag.RefundPayType = refundinfo.RefundPayType.ToDescription();
            refundinfo.IsOrderRefundTimeOut = _iOrderService.IsRefundTimeOut(refundinfo.OrderId);
            var order = _iOrderService.GetOrder(refundinfo.OrderId, CurrentUser.Id);
            string status = refundinfo.RefundStatus;
            if (order.DeliveryType == DeliveryType.SelfTake || order.ShopBranchId > 0)
            {
                status = refundinfo.RefundStatus.Replace("商家", "门店");
            }
            var shopBranch = ShopBranchApplication.GetShopBranchById(order.ShopBranchId);
            var branchName = shopBranch == null ? "" : shopBranch.ShopBranchName;
            ViewBag.OrderInfo = order;
            ViewBag.ShopBranchId = order.ShopBranchId;
            ViewBag.ShopBranchName = branchName;
            ViewBag.RefundStatus = status;
            if (order.OrderType == OrderInfo.OrderTypes.Virtual)
            {
                var list = refundinfo.VerificationCodeIds.Split(',').ToList();
                if (list.Count > 0)
                {
                    list = list.Select(a => a = Regex.Replace(a, @"(\d{4})", "$1 ")).ToList();
                    ViewBag.VerificationCodeIds = list;
                }
            }
            return View(refundinfo);
        }
        public ActionResult RefundProcessDetail(long id)
        {
            var refundinfo = _iRefundService.GetOrderRefund(id, CurrentUser.Id);
            int curappnum = refundinfo.ApplyNumber;
            ViewBag.RefundLogs = _iRefundService.GetRefundLogs(refundinfo.Id, curappnum, false);
            return View(refundinfo);
        }
        [HttpGet]
        public JsonResult GetShopGetAddress(long shopId, long shopBranchId = 0)
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
                    Region = string.IsNullOrEmpty(data.RegionStr) ? "" : data.RegionStr,
                    Address = string.IsNullOrEmpty(data.Address) ? "" : data.Address,
                    Phone = string.IsNullOrEmpty(data.TelPhone) ? "" : data.TelPhone,
                    ShipperName = string.IsNullOrEmpty(data.ShipperName) ? "" : data.ShipperName
                };
                return Json<dynamic>(true, data: model);
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
                    Region = redionstr,
                    Address = data == null ? "" : data.AddressDetail,
                    Phone = data == null ? "" : data.ContactPhone,
                    ShipperName = data == null ? "" : data.ContactUser
                };
                return Json<dynamic>(true, data: model);
            }
        }
        /// <summary>
        /// 转移图片
        /// </summary>
        /// <param name="image"></param>
        /// <param name="userId"></param>
        /// <param name="itemid"></param>
        /// <returns></returns>
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
        /// 下载微信图片
        /// </summary>
        /// <param name="link">下载地址</param>
        /// <param name="filePath">保存相对路径</param>
        /// <param name="fileName">保存地址</param>
        /// <returns></returns>
        private string DownloadWxImage(string mediaId, long userId, long itemid)
        {
            var token = AccessTokenContainer.TryGetAccessToken(SiteSettings.WeixinAppId, SiteSettings.WeixinAppSecret);
            var address = string.Format("https://file.api.weixin.qq.com/cgi-bin/media/get?access_token={0}&media_id={1}", token, mediaId);
            Random ra = new Random();
            string fileName = itemid.ToString() + "_" + DateTime.Now.ToString("yyMMddHHmmssffff") + ".jpg";
            var ImageDir = "/Storage/Plat/Refund/" + userId.ToString() + "/";
            WebClient wc = new WebClient();
            try
            {
                string fullPath = Path.Combine(ImageDir, fileName);
                var data = wc.DownloadData(address);
                MemoryStream stream = new MemoryStream(data);
                Core.MallIO.CreateFile(fullPath, stream, FileCreateType.Create);
                return ImageDir + fileName;
            }
            catch (Exception ex)
            {
                Log.Error("下载图片发生异常" + ex.Message);
                return string.Empty;
            }
        }
    }
}