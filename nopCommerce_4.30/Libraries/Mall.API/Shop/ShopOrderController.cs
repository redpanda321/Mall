using Mall.API.Model;
using Mall.API.Model.ParamsModel;
using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mall.API
{
    public class ShopOrderController : BaseShopLoginedApiController
    {
        /// <summary>
        /// 根据核销码取订单
        /// </summary>
        /// <param name="pickcode"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetShopOrder")]
        public object GetShopOrder(string pickcode)
        {
            CheckUserLogin();

            var codeInfo = OrderApplication.GetOrderVerificationCodeInfoByCode(pickcode);
            if (codeInfo == null)
                return new { success = false, msg = "该核销码无效" };

            var order = OrderApplication.GetOrderInfo(codeInfo.OrderId);
            if (order == null)
                return new { success = false, msg = "该核销码无效" };

            if (order.OrderType != OrderInfo.OrderTypes.Virtual)
                return new { success = false, msg = "核销订单无效" };

            if (order.ShopId != this.CurrentShop.Id)
                return new { success = false, msg = "非本店核销码，请买家核对信息" };

            if (order.ShopBranchId > 0)//商家只能核销本商家，而非下面门店的
                return new { success = false, msg = "非本店核销码，请买家核对信息" };

            if (codeInfo.Status == OrderInfo.VerificationCodeStatus.AlreadyVerification)
                return new { success = false, msg = string.Format("该核销码于{0}已核销", codeInfo.VerificationTime.Value.ToString("yyyy-MM-dd HH:mm")) };

            if (codeInfo.Status == OrderInfo.VerificationCodeStatus.Expired)
                return new { success = false, msg = "此核销码已过期，无法核销" };

            if (codeInfo.Status == OrderInfo.VerificationCodeStatus.Refund)
                return new { success = false, msg = "此核销码正处于退款中，无法核销" };

            if (codeInfo.Status == OrderInfo.VerificationCodeStatus.RefundComplete)
                return new { success = false, msg = "此核销码已经退款成功，无法核销" };

            var orderItem = Application.OrderApplication.GetOrderItemsByOrderId(order.Id);
            var orderItemInfo = orderItem.FirstOrDefault();
            var virtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(orderItemInfo.ProductId);
            if (virtualProductInfo != null && virtualProductInfo.ValidityType && DateTime.Now < virtualProductInfo.StartDate.Value)
            {
                return new { success = false, msg = "该核销码暂时不能核销，请留意生效时间！" };
            }
            if (orderItemInfo.EffectiveDate.HasValue)
            {
                if (DateTime.Now < orderItemInfo.EffectiveDate.Value)
                    return new { success = false, msg = "该核销码暂时不能核销，请留意生效时间！" };
            }

            foreach (var item in orderItem)
            {
                item.ThumbnailsUrl = Core.MallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Mall.CommonModel.ImageSize.Size_100);
                Entities.TypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetTypeByProductId(item.ProductId);
                item.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                item.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                item.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                var productInfo = ProductManagerApplication.GetProduct(item.ProductId);
                if (productInfo != null)
                {
                    item.ColorAlias = !string.IsNullOrWhiteSpace(productInfo.ColorAlias) ? productInfo.ColorAlias : item.ColorAlias;
                    item.SizeAlias = !string.IsNullOrWhiteSpace(productInfo.SizeAlias) ? productInfo.SizeAlias : item.SizeAlias;
                    item.VersionAlias = !string.IsNullOrWhiteSpace(productInfo.VersionAlias) ? productInfo.VersionAlias : item.VersionAlias;
                }
            }
            int validityType = 0; string startDate = string.Empty, endDate = string.Empty;
            if (virtualProductInfo != null)
            {
                validityType = virtualProductInfo.ValidityType ? 1 : 0;
                if (validityType == 1)
                {
                    startDate = virtualProductInfo.StartDate.Value.ToString("yyyy-MM-dd");
                    endDate = virtualProductInfo.EndDate.Value.ToString("yyyy-MM-dd");
                }
            }
            var verifications = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(new List<long>() { order.Id }).Where(a => a.Status == OrderInfo.VerificationCodeStatus.WaitVerification);
            var codes = verifications.ToList();//待消费的核销码
            if (codes != null)
            {
                codes.ForEach(a =>
                {
                    a.SourceCode = a.VerificationCode;
                    a.VerificationCode = System.Text.RegularExpressions.Regex.Replace(a.VerificationCode, "(\\d{4})\\d{4}(\\d{4})", "$1****$2");
                });
            }
            var verificationCode = codes.Select(p =>
            {
                return new
                {
                    Status = p.Status,
                    VerificationCode = p.VerificationCode,
                    SourceCode = p.SourceCode
                };
            });
            var virtualOrderItemInfos = OrderApplication.GetVirtualOrderItemInfosByOrderId(order.Id).Select(p =>
            {
                return new
                {
                    VirtualProductItemType = p.VirtualProductItemType,
                    VirtualProductItemName = p.VirtualProductItemName,
                    Content = ReplaceImage(p.Content, p.VirtualProductItemType)
                };
            });
            order.OrderStatusText = order.OrderStatus.ToDescription();
            order.PaymentTypeName = PaymentApplication.GetPaymentTypeDescById(order.PaymentTypeGateway) ?? order.PaymentTypeName;//统一显示支付方式名称
            return new { success = true, order = order, orderItem = orderItem, virtualOrderItemInfos = virtualOrderItemInfos, verificationCodes = verificationCode, validityType = validityType, startDate = startDate, endDate = endDate };

        }
        /// <summary>
        /// 商家核销订单
        /// </summary>
        /// <param name="pickcode"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetShopOrderConfirm")]
        public object GetShopOrderConfirm(string pickcode)
        {
            CheckUserLogin();

            if (string.IsNullOrWhiteSpace(pickcode))
                return new { success = false, msg = "该核销码无效" };

            var codes = pickcode.Split(',').ToList();
            var list = OrderApplication.GetOrderVerificationCodeInfoByCodes(codes);
            if (list != null && list.Count > 0)
            {
                var orderCount = list.Select(a => a.OrderId).Distinct().Count();
                if (orderCount > 1)
                {
                    return new { success = false, msg = "非同一个订单的核销码无法一起核销" };
                }

                var order = OrderApplication.GetOrderInfo(list[0].OrderId);
                if (order == null)
                    return new { success = false, msg = "该核销码无效" };

                if (order.OrderType != OrderInfo.OrderTypes.Virtual)
                    return new { success = false, msg = "核销订单无效" };

                if (order.ShopId != this.CurrentShop.Id)
                    return new { success = false, msg = "非本店核销码，请买家核对信息" };

                if (order.ShopBranchId > 0)
                    return new { success = false, msg = "非本店核销码，请买家核对信息" };

                var waitVerificationNum = list.Where(a => a.Status == OrderInfo.VerificationCodeStatus.WaitVerification).Count();
                if (waitVerificationNum != list.Count)
                {
                    return new { success = false, msg = "包含非待消费的核销码" };
                }

                var orderItem = Application.OrderApplication.GetOrderItemsByOrderId(order.Id).FirstOrDefault();
                if (orderItem != null && orderItem.EffectiveDate.HasValue)
                {
                    if (DateTime.Now < orderItem.EffectiveDate.Value)
                        return new { success = false, msg = "该核销码暂时不能核销，请留意生效时间！" };
                }
                var verificationTime = DateTime.Now;
                OrderApplication.UpdateOrderVerificationCodeStatusByCodes(list.Select(a => a.VerificationCode).ToList(), order.Id, OrderInfo.VerificationCodeStatus.AlreadyVerification, DateTime.Now, this.CurrentUser.UserName);
                OrderApplication.AddVerificationRecord(new VerificationRecordInfo()
                {
                    OrderId = order.Id,
                    VerificationCodeIds = "," + string.Join(",", list.Select(a => a.VerificationCode)) + ",",
                    VerificationTime = verificationTime,
                    VerificationUser = this.CurrentUser.UserName
                });
                return new { success = true, msg = "已核销", verificationCodes = string.Join(",", list.Select(a => a.VerificationCode)), verificationTime = verificationTime.ToString("yyyy-MM-dd HH:mm:ss"), verificationUser = this.CurrentUser.UserName };
            }
            return new { success = false, msg = "错误的核销码" };
        }

        /// <summary>
        /// 搜索商家订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost("PostSearchShopOrder")]
        public List<DTO.FullOrder> PostSearchShopOrder(OrderQuery query)
        {
            if (query.PageNo < 1)
                query.PageNo = 1;
            if (query.PageSize < 1)
                query.PageSize = 10;

            CheckShopManageLogin();
            query.Operator = Operator.Seller;
            query.ShopId = CurrentShop.Id;
            if (query.IsSelfTake.HasValue && query.IsSelfTake.Value == 1)
            {
                query.Status = OrderInfo.OrderOperateStatus.Finish;
            }
            var data = Application.OrderApplication.GetFullOrders(query);


            return data.Models;
        }


        [HttpGet("GetShopBranchs")]
        public object GetShopBranchs()
        {
            CheckUserLogin();
            var branchs = ShopBranchApplication.GetShopBranchByShopId(CurrentUser.ShopId);
            return new { success = true, branchs = branchs };
        }

        [HttpPost("PostSearchVerificationRecord")]
        public object PostSearchVerificationRecord(VerificationRecordQuery query)
        {
            if (query.PageNo < 1)
                query.PageNo = 1;
            if (query.PageSize < 1)
                query.PageSize = 10;

            CheckUserLogin();

            query.ShopId = this.CurrentShop.Id;

            var data = OrderApplication.GetVerificationRecords(query);
            return data.Models.Select(p =>
            {
                return new
                {
                    Quantity = p.Quantity,
                    ImagePath = p.ImagePath,
                    Specifications = p.Specifications,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    OrderId = p.OrderId,
                    VerificationUser = p.VerificationUser,
                    VerificationTime = p.VerificationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Name = this.CurrentShop.ShopName,
                    Id = p.Id
                };

            });
        }

        [HttpGet("GetVerificationRecordDetail")]
        public object GetVerificationRecordDetail(long id)
        {
            CheckUserLogin();

            var recordInfo = OrderApplication.GetVerificationRecordInfoById(id);
            if (recordInfo == null)
                return new { success = false, msg = "错误的ID" };

            var order = OrderApplication.GetOrderInfo(recordInfo.OrderId);
            if (order == null)
                return new { success = false, msg = "该核销码无效" };

            if (order.ShopId != this.CurrentShop.Id)
                return new { success = false, msg = "非本店核销记录" };

            var orderItem = Application.OrderApplication.GetOrderItemsByOrderId(order.Id);
            foreach (var item in orderItem)
            {
                item.ThumbnailsUrl = Core.MallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Mall.CommonModel.ImageSize.Size_100);
                Entities.TypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetTypeByProductId(item.ProductId);
                item.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                item.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                item.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                var productInfo = ProductManagerApplication.GetProduct(item.ProductId);
                if (productInfo != null)
                {
                    item.ColorAlias = !string.IsNullOrWhiteSpace(productInfo.ColorAlias) ? productInfo.ColorAlias : item.ColorAlias;
                    item.SizeAlias = !string.IsNullOrWhiteSpace(productInfo.SizeAlias) ? productInfo.SizeAlias : item.SizeAlias;
                    item.VersionAlias = !string.IsNullOrWhiteSpace(productInfo.VersionAlias) ? productInfo.VersionAlias : item.VersionAlias;
                }
            }
            int validityType = 0; string startDate = string.Empty, endDate = string.Empty;
            var virtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(orderItem.FirstOrDefault().ProductId);
            if (virtualProductInfo != null)
            {
                validityType = virtualProductInfo.ValidityType ? 1 : 0;
                if (validityType == 1)
                {
                    startDate = virtualProductInfo.StartDate.Value.ToString("yyyy-MM-dd");
                    endDate = virtualProductInfo.EndDate.Value.ToString("yyyy-MM-dd");
                }
            }

            var verificationCode = recordInfo.VerificationCodeIds.Trim(',').Split(',').ToList();
            verificationCode = verificationCode.Select(a => a = Regex.Replace(a, @"(\d{4})", "$1 ")).ToList();
            var virtualOrderItemInfos = OrderApplication.GetVirtualOrderItemInfosByOrderId(order.Id).Select(p =>
            {
                return new
                {
                    VirtualProductItemType = p.VirtualProductItemType,
                    VirtualProductItemName = p.VirtualProductItemName,
                    Content = ReplaceImage(p.Content, p.VirtualProductItemType)
                };
            });
            order.OrderStatusText = order.OrderStatus.ToDescription();
            return new { success = true, order = order, orderItem = orderItem, virtualOrderItemInfos = virtualOrderItemInfos, verificationCodes = verificationCode, validityType = validityType, startDate = startDate, endDate = endDate, verificationTime = recordInfo.VerificationTime.ToString("yyyy-MM-dd HH:mm:ss"), verificationUser = recordInfo.VerificationUser };
        }

        [HttpGet("GetShopOrderCount")]
        public object GetShopOrderCount()
        {
            CheckUserLogin();
            var statistic = StatisticApplication.GetSellerOrderStatistic(CurrentUser.ShopId);
            return new
            {
                success = true,
                waitPayCount = statistic.WaitingForPay,
                waitReceive = statistic.WaitingForRecieve,
                waitDelivery = statistic.WaitingForDelivery,
                waitSelfPickUp = statistic.WaitingForSelfPickUp,
                waitConsumption = OrderApplication.GetWaitConsumptionOrderNumByUserId(shopId: CurrentUser.ShopId)
            };
        }
        [HttpGet("GetOrderDetail")]
        public object GetOrderDetail(long id)
        {
            CheckUserLogin();
            long shopid = CurrentUser.ShopId;

            var ordser = ServiceProvider.Instance<IOrderService>.Create;

            Entities.OrderInfo order = ordser.GetOrder(id);
            if (order == null || order.ShopId != shopid)
            {
                throw new MallApiException("错误的订单编号");
            }
            var bonusService = ServiceProvider.Instance<IShopBonusService>.Create;
            var orderRefundService = ServiceProvider.Instance<IRefundService>.Create;
            var shopService = ServiceProvider.Instance<IShopService>.Create;
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var vshop = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(order.ShopId);
            bool isCanApply = false;
            DTO.ShopBranch ShopBranchInfo = null;
            if (order.ShopBranchId > 0)
                ShopBranchInfo = ShopBranchApplication.GetShopBranchById(order.ShopBranchId);

            var orderitems = OrderApplication.GetOrderItems(order.Id);
            //获取订单商品项数据
            var orderDetail = new
            {
                ShopName = shopService.GetShop(order.ShopId).ShopName,
                ShopId = order.ShopId,
                OrderItems = orderitems.Select(item =>
                {
                    var productinfo = productService.GetProduct(item.ProductId);
                    if (order.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitDelivery)
                    {
                        isCanApply = orderRefundService.CanApplyRefund(id, item.Id);
                    }
                    else
                    {
                        isCanApply = orderRefundService.CanApplyRefund(id, item.Id, false);
                    }

                    Entities.TypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(productinfo.TypeId);
                    string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                    if (productinfo != null)
                    {
                        colorAlias = !string.IsNullOrWhiteSpace(productinfo.ColorAlias) ? productinfo.ColorAlias : colorAlias;
                        sizeAlias = !string.IsNullOrWhiteSpace(productinfo.SizeAlias) ? productinfo.SizeAlias : sizeAlias;
                        versionAlias = !string.IsNullOrWhiteSpace(productinfo.VersionAlias) ? productinfo.VersionAlias : versionAlias;
                    }
                    return new
                    {
                        ItemId = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Count = item.Quantity,
                        Price = item.SalePrice,
                        //ProductImage = "http://" + Url.Request.RequestUri.Host + productService.GetProduct(item.ProductId).GetImage(ProductInfo.ImageSize.Size_100),
                        ProductImage = Core.MallIO.GetRomoteProductSizeImage(productService.GetProduct(item.ProductId).RelativePath, 1, (int)Mall.CommonModel.ImageSize.Size_100),
                        color = item.Color,
                        size = item.Size,
                        version = item.Version,
                        IsCanRefund = isCanApply,
                        ColorAlias = colorAlias,
                        SizeAlias = sizeAlias,
                        VersionAlias = versionAlias
                    };
                })
            };
            VirtualProductInfo virtualProductInfo = null;
            List<dynamic> codes = null;
            List<dynamic> virtualItems = null;

            int validityType = 0; string startDate = string.Empty, endDate = string.Empty;
            if (order.OrderType == OrderInfo.OrderTypes.Virtual && orderDetail.OrderItems != null)
            {
                virtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(orderDetail.OrderItems.FirstOrDefault().ProductId);
                if (virtualProductInfo != null)
                {
                    validityType = virtualProductInfo.ValidityType ? 1 : 0;
                    if (validityType == 1)
                    {
                        startDate = virtualProductInfo.StartDate.Value.ToString("yyyy-MM-dd");
                        endDate = virtualProductInfo.EndDate.Value.ToString("yyyy-MM-dd");
                    }
                }
                var verificationCodes = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(new List<long>() { order.Id });
                if (verificationCodes != null)
                {
                    verificationCodes.ForEach(a =>
                    {
                        if (a.Status == OrderInfo.VerificationCodeStatus.WaitVerification || a.Status == OrderInfo.VerificationCodeStatus.Refund)
                        {
                            a.VerificationCode = System.Text.RegularExpressions.Regex.Replace(a.VerificationCode, "(\\d{4})\\d{4}(\\d{4})", "$1****$2");
                        }
                        a.VerificationCode = System.Text.RegularExpressions.Regex.Replace(a.VerificationCode, @"(\d{4})", "$1 ");
                    });
                }
                codes = verificationCodes.Select(p =>
                {
                    return new
                    {
                        VerificationCode = p.VerificationCode,
                        Status = p.Status,
                        StatusText = p.Status.ToDescription()
                    };
                }).ToList<dynamic>();

                var virtualOrderItems = OrderApplication.GetVirtualOrderItemInfosByOrderId(order.Id);
                virtualItems = virtualOrderItems.Select(p =>
                {
                    return new
                    {
                        VirtualProductItemName = p.VirtualProductItemName,
                        VirtualProductItemType = p.VirtualProductItemType,
                        Content = ReplaceImage(p.Content, p.VirtualProductItemType)
                    };
                }).ToList<dynamic>();
            }
            var orderModel = new
            {
                Id = order.Id,
                OrderType = order.OrderType,
                OrderTypeName = order.OrderType.ToDescription(),
                Status = order.OrderStatus.ToDescription(),
                ShipTo = order.ShipTo,
                Phone = order.CellPhone,
                Address = order.RegionFullName + " " + order.Address,
                HasExpressStatus = !string.IsNullOrWhiteSpace(order.ShipOrderNumber),
                ExpressCompanyName = order.ExpressCompanyName,
                Freight = order.Freight,
                Tax = order.Tax,
                IntegralDiscount = order.IntegralDiscount,
                RealTotalAmount = order.OrderTotalAmount - order.RefundTotalAmount,
                CapitalAmount = order.CapitalAmount,
                RefundTotalAmount = order.RefundTotalAmount,
                ProductTotalAmount = order.ProductTotalAmount,
                OrderDate = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                ShopName = order.ShopName,
                ShopBranchName = (ShopBranchInfo != null ? ShopBranchInfo.ShopBranchName : ""),
                VShopId = vshop == null ? 0 : vshop.Id,
                commentCount = OrderApplication.GetOrderCommentCount(order.Id),
                ShopId = order.ShopId,
                orderStatus = (int)order.OrderStatus,
                //Invoice = order.InvoiceType.ToDescription(),
                //InvoiceValue = (int)order.InvoiceType,
                //InvoiceContext = order.InvoiceContext,
                //InvoiceTitle = order.InvoiceTitle,
                PaymentType = order.PaymentType.ToDescription(),
                PaymentTypeDesc=order.PaymentTypeDesc,
                OrderPayAmount=order.OrderPayAmount,
                PaymentTypeName = PaymentApplication.GetPaymentTypeDescById(order.PaymentTypeGateway) ?? order.PaymentTypeName,
                PaymentTypeValue = (int)order.PaymentType,
                FullDiscount = order.FullDiscount,
                DiscountAmount = order.DiscountAmount,
                OrderRemarks = order.OrderRemarks,
                DeliveryType = order.DeliveryType,
                //InvoiceCode = order.InvoiceCode,
                OrderInvoice = OrderApplication.GetOrderInvoiceInfo(order.Id)
            };
            return new { success = true, Order = orderModel, OrderItem = orderDetail.OrderItems, VerificationCodes = codes, VirtualOrderItems = virtualItems, StartDate = startDate, EndDate = endDate, ValidityType = validityType };
        }

        /// <summary>
        /// 获取订单信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        /// 

        [HttpGet("GetOrderInfo")]
        public object GetOrderInfo(long orderId)
        {
          //  AutoMapper.Mapper.CreateMap<DTO.Order, ShopOrderGetOrderModel>();
            var data = Application.OrderApplication.GetOrder(orderId);
            //  ShopOrderGetOrderModel result = AutoMapper.Mapper.Map<DTO.Order, ShopOrderGetOrderModel>(data);

            ShopOrderGetOrderModel result = data.Map<ShopOrderGetOrderModel>();

            result.CanDaDaExpress = CityExpressConfigApplication.GetStatus(data.ShopId);
            return result;
        }
        /// <summary>
        /// 获取所有快递公司名称
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetAllExpress")]
        public string GetAllExpress()
        {
            var listData = ExpressApplication.GetAllExpress().Select(i => i.Name);
            return String.Join(",", listData);
        }
        [HttpGet("GetIsOrderAfterService")]
        /// <summary>
        /// 订单是否正在申请售后
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public object GetIsOrderAfterService(long orderId)
        {
            bool isAfterService = Application.OrderApplication.IsOrderAfterService(orderId);
            if (isAfterService)
            {
                return new { success = true, isAfterService = true };
            }
            else
            {
                return new { success = true, isAfterService = false };
            }
        }

        /// <summary>
        /// 商家发货
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// 
        [HttpPost("PostShopSendGood")]
        public object PostShopSendGood(OrderDeliveryModel model)
        {
            CheckUserLogin();
            long shopid = CurrentShop.Id;
            string shopkeeperName = "";
            shopkeeperName = CurrentUser.UserName;
            var returnurl = String.Format("{0}/Common/ExpressData/SaveExpressData", CurrentUrlHelper.CurrentUrlNoPort());
            if (model.deliveryType == DeliveryType.CityExpress.GetHashCode())  //同城物流
            {
                var dadaconfig = CityExpressConfigApplication.GetDaDaCityExpressConfig(shopid);
                if (!dadaconfig.IsEnable)
                {
                    throw new MallApiException("未开启同城合作物流");
                }
                var order = OrderApplication.GetOrder(model.orderId);
                if (order == null || order.ShopId != shopid || order.ShopBranchId > 0 || order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitDelivery)
                {
                    throw new MallApiException("错误的订单编号");
                }
                //Log.Error("达达发货ShopOrder-开始01：orderId:" + model.orderId);

                string json = ExpressDaDaHelper.addAfterQuery(shopid, dadaconfig.source_id, model.shipOrderNumber);
                var resultObj = JsonConvert.DeserializeObject(json) as JObject;
                string status = resultObj["status"].ToString();

                //Log.Error("达达发货ShopOrder-开始02：status:" + status);
                if (status != "success")
                {
                    //订单码过期，重发单
                    json = SendDaDaExpress(model.orderId, shopid, false);
                    var rObj2 = JsonConvert.DeserializeObject(json) as JObject;
                    string status2 = rObj2["status"].ToString();
                    if (status2 != "success")
                    {
                        string msg = rObj2["msg"].ToString();
                        return ErrorResult(msg);
                    }
                }
            }
            OrderApplication.ShopSendGood(model.orderId, model.deliveryType, shopkeeperName, model.companyName, model.shipOrderNumber, returnurl);
            return SuccessResult("发货成功");
        }

        [HttpGet("GetCancelDadaExpress")]
        public object GetCancelDadaExpress(long orderId, int reasonId, string cancelReason)
        {
            CheckUserLogin();
            long shopid = CurrentShop.Id;
            var order = OrderApplication.GetOrder(orderId);
            if (order == null || order.ShopId != shopid || order.ShopBranchId > 0 || order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitReceiving || order.DeliveryType != DeliveryType.CityExpress)
            {
                throw new MallApiException("错误的订单编号");
            }
            if (order.DadaStatus > DadaStatus.WaitTake.GetHashCode())
            {
                throw new MallApiException("订单配送不可取消！");
            }
            var dadaconfig = CityExpressConfigApplication.GetDaDaCityExpressConfig(shopid);
            string json = ExpressDaDaHelper.orderFormalCancel(shopid, orderId.ToString(), reasonId, cancelReason);
            var resultObj = JsonConvert.DeserializeObject(json) as JObject;
            string status = resultObj["status"].ToString();
            if (status != "success")
            {
                throw new MallApiException(resultObj["msg"].ToString());
            }
            ExpressDaDaApplication.SetOrderCancel(orderId, "商家主动取消");
            var result = JsonConvert.DeserializeObject(resultObj["result"].ToString()) as JObject;
            return new
            {
                success = true,
                deduct_fee = result["deduct_fee"].ToString()
            };
        }
        [HttpGet("GetCityExpressDaDa")]
        public object GetCityExpressDaDa(long orderId)
        {
            CheckUserLogin();
            long shopid = CurrentShop.Id;
            string json = SendDaDaExpress(orderId, shopid, true);
            var resultObj = JsonConvert.DeserializeObject(json) as JObject;
            string status = resultObj["status"].ToString();
            if (status != "success")
            {
                throw new MallApiException(resultObj["msg"].ToString());
            }
            var result = JsonConvert.DeserializeObject(resultObj["result"].ToString()) as JObject;

            return new
            {
                success = true,
                distance = result["distance"].ToString(),
                fee = result["fee"].ToString(),
                deliveryNo = result["deliveryNo"].ToString(),
            };
        }

        [HttpGet("GetDadaCancelReasons")]
        public object GetDadaCancelReasons()
        {
            CheckUserLogin();
            long shopid = CurrentShop.Id;
            string json = ExpressDaDaHelper.orderCancelReasons(shopid);
            var resultObj = JsonConvert.DeserializeObject(json) as JObject;
            return resultObj;
        }

        private string SendDaDaExpress(long orderId, long shopid, bool isQueryOrder)
        {
            //Log.Error("达达发货ShopOrder-01：orderId:" + orderId);
            var order = OrderApplication.GetOrder(orderId);
            if (order == null || order.ShopId != shopid || order.ShopBranchId > 0 || order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                throw new MallApiException("错误的订单编号");
            }
            var dadaconfig = CityExpressConfigApplication.GetDaDaCityExpressConfig(shopid);
            if (!dadaconfig.IsEnable)
            {
                throw new MallApiException("未开启同城合作物流");
            }
            //Log.Error("达达发货ShopOrder-02：OrderStatus:" + order.OrderStatus + "--ReceiveLatitude:" + order.ReceiveLatitude + "--ReceiveLongitude:" + order.ReceiveLongitude + "--RegionId:" + order.RegionId);
            if (order.ReceiveLatitude <= 0 || order.ReceiveLongitude <= 0)
            {
                throw new MallApiException("未获取到客户收货地址坐标信息，无法使用该配送方式");
            }
            var shopsend = ShopShippersApplication.GetDefaultSendGoodsShipper(shopid);
            if (shopsend == null || shopsend.Latitude <= 0 || shopsend.Longitude <= 0)
            {
                throw new MallApiException("店铺没有发货地址或发货地址没有坐标信息，无法发单，请前往后台进行设置");
            }
            //Log.Error("达达发货ShopOrder-03：Latitude:" + shopsend.Latitude + "--Longitude：" + shopsend.Longitude);
            string cityCode = "";
            var _adregion = RegionApplication.GetRegion(order.RegionId);
            var _city = GetCity(_adregion);
            try
            {
                string cityJson = ExpressDaDaHelper.cityCodeList(shopid);
                var cityObj = JsonConvert.DeserializeObject(cityJson) as JObject;
                JArray citylist = (JArray)cityObj["result"];
                foreach (JToken item in citylist)
                {
                    if (_city.ShortName == item["cityName"].ToString())
                    {
                        cityCode = item["cityCode"].ToString();
                        break;
                    }
                }

            }
            catch
            {
            }
            //达达不支持的城市
            if (cityCode == "")
            {
                throw new MallApiException("配送范围超区，无法配送");
            }
            string callback = CurrentUrlHelper.CurrentUrl() + "/pay/dadaOrderNotify/";

            bool isreaddorder = (order.DadaStatus == DadaStatus.Cancel.GetHashCode());
            if (isQueryOrder)
            {
                isreaddorder = false;
            }

            //Log.Error(DateTime.Now + "ShopOrder-0:callback:" + callback + "|shopid:" + shopid + "|source_id:" + dadaconfig.source_id + "|orderId:" + order.Id + "|cityCode:" + cityCode + "|isreaddorder:" + isreaddorder);
            string json = ExpressDaDaHelper.addOrder(shopid, dadaconfig.source_id, order.Id.ToString()
                , cityCode, (double)order.TotalAmount, 0, ExpressDaDaHelper.DateTimeToUnixTimestamp(DateTime.Now.AddMinutes(15))
                , order.ShipTo, order.Address, order.ReceiveLatitude, order.ReceiveLongitude
                , callback, order.CellPhone, order.CellPhone, isQueryDeliverFee: isQueryOrder
                , isReAddOrder: isreaddorder);

            //Log.Error(DateTime.Now + "达达发货ShopOrder-1：json:" + json);
            return json;
        }

        /// <summary>
        /// 获取市级地区
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        private CommonModel.Region GetCity(CommonModel.Region region)
        {
            CommonModel.Region _city = region;
            if (_city.Level == CommonModel.Region.RegionLevel.City || _city.Level == CommonModel.Region.RegionLevel.Province || _city.Parent == null)
            {
                return _city;
            }
            _city = _city.Parent;
            return GetCity(_city);
        }
        private List<string> ReplaceImage(string content, ProductInfo.VirtualProductItemType type)
        {
            if (type != ProductInfo.VirtualProductItemType.Picture)
                return new List<string>() { content };

            List<string> list = content.Split(',').ToList();
            return list.Select(a => a = CurrentUrlHelper.CurrentUrl() + a).ToList();
        }


    }
}
