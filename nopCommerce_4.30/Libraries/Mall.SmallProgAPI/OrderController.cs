using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.Entities;
using Mall.SmallProgAPI.Model;
using Mall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;

using Mall.SmallProgAPI.Model.ParamsModel;
using Microsoft.AspNetCore.Mvc;

namespace Mall.SmallProgAPI
{

    public class OrderController : BaseApiController
    {

        //public object GetExpressInfo(long orderId)
        //{
        //    CheckUserLogin();
        //    Entities.OrderInfo order = ServiceProvider.Instance<IOrderService>.Create.GetOrder(orderId, CurrentUser.Id);
        //    var expressData = ServiceProvider.Instance<IExpressService>.Create.GetExpressData(order.ExpressCompanyName, order.ShipOrderNumber);

        //    if (expressData.Success)
        //        expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
        //    var json = new
        //    {
        //        success = expressData.Success,
        //        traces = expressData.ExpressDataItems.Select(item => new
        //        {
        //            acceptTime = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
        //            acceptStation = item.Content
        //        })
        //    };
        //    return Json(new { ShipTo = order.ShipTo, CellPhone = order.CellPhone, Address = order.RegionFullName + order.Address, ShipOrderNumber = order.DeliveryType == DeliveryType.ShopStore ? "" : order.ShipOrderNumber, ExpressCompanyName = order.DeliveryType == DeliveryType.ShopStore ? "门店店员配送" : order.ExpressCompanyName, LogisticsData = json });
        //}

        /// <summary>
        /// 获取物流公司信息
        /// </summary>
        /// <returns></returns>
        //public JsonResult<Result<IEnumerable<dynamic>>> GetExpressList()
        //{
        //    var express = ServiceProvider.Instance<IExpressService>.Create.GetAllExpress();
        //    var list = express.ToList().Select(item => new
        //    {
        //        ExpressName = item.Name,
        //        Kuaidi100Code = item.Kuaidi100Code,
        //        TaobaoCode = item.TaobaoCode
        //    });
        //    return JsonResult<IEnumerable<dynamic>>(list);
        //}


        [HttpGet("GetSubmitModel")]
        public object GetSubmitModel(string cartItemIds, string productSku, WXSmallProgFromPageType fromPage, long buyAmount = 0, long shippingAddressId = 0, string couponIds = "", bool isStore = false, sbyte productType = 0, long shopBranchId = 0)
        {
            CheckUserLogin();
            var coupons = CouponApplication.ConvertUsedCoupon(couponIds);
            if (fromPage == WXSmallProgFromPageType.SignBuy)
            {
                if (!string.IsNullOrWhiteSpace(productSku))
                {
                    return GetSubmitModelById(productSku, (int)buyAmount, shippingAddressId, coupons, isStore, productType, shopBranchId);
                }
            }
            if (fromPage == WXSmallProgFromPageType.Cart)
            {
                return GetSubmitByCartModel(cartItemIds, shippingAddressId, coupons, isStore);
            }
            return Json(ErrorResult<dynamic>(string.Empty));
        }
        /// <summary>
        /// 获取立即购买提交页面的数据
        /// </summary>
        /// <param name="skuIds">库存ID集合</param>
        /// <param name="counts">库存ID对应的数量</param>
        object GetSubmitModelById(string skuId, int count, long shippingAddressId = 0, IEnumerable<string[]> CouponIdsStr = null, bool isStore = false, sbyte productType = 0, long shopBranchId = 0)
        {
            CheckUserLogin();
            dynamic d = new System.Dynamic.ExpandoObject();
            var siteconfig = SiteSettingApplication.SiteSettings;

            var result = OrderApplication.GetMobileSubmit(CurrentUserId, skuId.ToString(), count.ToString(), shippingAddressId, CouponIdsStr, shopBranchId);
            dynamic add = new System.Dynamic.ExpandoObject();
            if (result.Address != null)
            {
                add = new
                {
                    ShippingId = result.Address.Id,
                    ShipTo = result.Address.ShipTo,
                    CellPhone = result.Address.Phone,
                    FullRegionName = result.Address.RegionFullName,
                    FullAddress = result.Address.RegionFullName + " " + result.Address.Address + " " + result.Address.AddressDetail,
                    Address = result.Address.Address,
                    RegionId = result.Address.RegionId
                };
            }
            else
            {
                add = null;
            }
            string shipperAddress = string.Empty, shipperTelPhone = string.Empty;
            if (isStore)
            {
                if (productType == 0)
                    throw new MallException("门店订单暂时不允许立即购买");
                //门店订单
                Mall.DTO.ShopBranch storeInfo = Application.ShopBranchApplication.GetShopBranchById(shopBranchId);
                if (storeInfo == null)
                    throw new MallException("获取门店信息失败，不可提交非门店商品");
                d.shopBranchId = shopBranchId;
                d.shopBranchInfo = storeInfo;
                if (storeInfo != null)
                {
                    shipperAddress = RegionApplication.GetFullName(storeInfo.AddressId) + storeInfo.AddressDetail;
                    shipperTelPhone = storeInfo.ContactPhone;
                }
            }
            d.ProductType = productType;
            if (result.ProductType == 1)
            {
                d.VirtualProductItemInfos = ProductManagerApplication.GetVirtualProductItemInfoByProductId(result.ProductId);
                var virtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(result.ProductId);
                if (virtualProductInfo != null && virtualProductInfo.ValidityType && DateTime.Now > virtualProductInfo.EndDate.Value)
                {
                    throw new MallException("该虚拟商品已过期，不支持下单");
                }
                if (result.products != null && result.products.Count > 0 && !isStore)
                {
                    var verificationShipper = ShopShippersApplication.GetDefaultVerificationShipper(result.products.FirstOrDefault().shopId);//虚拟订单支持立即购买所以商家只有一个
                    if (verificationShipper != null)
                    {
                        shipperAddress = RegionApplication.GetFullName(verificationShipper.RegionId) + " " + verificationShipper.Address;
                        shipperTelPhone = verificationShipper.TelPhone;
                    }
                }
            }
            d.shipperAddress = shipperAddress;
            d.shipperTelPhone = shipperTelPhone;
            bool canIntegralPerMoney = true, canCapital = true;
            CanDeductible(out canIntegralPerMoney, out canCapital);
            d.InvoiceContext = result.InvoiceContext;
            d.InvoiceTitle = result.InvoiceTitle;
            d.cellPhone = result.cellPhone;
            d.email = result.email;
            d.vatInvoice = result.vatInvoice;
            d.invoiceName = result.invoiceName;//默认抬头(普通、电子)
            d.invoiceCode = result.invoiceCode;//默认税号(普通、电子)
            d.products = result.products;
            d.TotalAmount = result.totalAmount;
            d.Freight = result.Freight;
            d.orderAmount = result.orderAmount;
            d.IsCashOnDelivery = result.IsCashOnDelivery;
            d.IsOpenStore = siteconfig.IsOpenStore;
            d.Address = add;
            d.integralPerMoney = result.integralPerMoney;
            d.userIntegralMaxDeductible = result.userIntegralMaxDeductible;
            d.integralPerMoneyRate = result.integralPerMoneyRate;
            d.userIntegralMaxRate = siteconfig.IntegralDeductibleRate;
            d.userIntegrals = result.userIntegrals;
            d.TotalMemberIntegral = result.memberIntegralInfo.AvailableIntegrals;
            d.canIntegralPerMoney = canIntegralPerMoney;
            d.canCapital = canCapital;
            d.capitalAmount = result.capitalAmount;

            return Json(d);
        }

        private void CanDeductible(out bool canIntegralPerMoney, out bool canCapital)
        {
            //授权模块控制积分抵扣、余额抵扣功能是否开放
            canIntegralPerMoney = true;
            canCapital = true;
            var siteInfo = SiteSettingApplication.SiteSettings;
            if (siteInfo != null)
            {
                if (!(siteInfo.IsOpenPC || siteInfo.IsOpenH5 || siteInfo.IsOpenApp || siteInfo.IsOpenMallSmallProg))
                {
                    canIntegralPerMoney = false;
                }
                if (!(siteInfo.IsOpenPC || siteInfo.IsOpenH5 || siteInfo.IsOpenApp || siteInfo.IsOpenMallSmallProg))
                {
                    canCapital = false;
                }
            }
        }

        /// <summary>
        /// 获取购物车提交页面的数据
        /// </summary>
        /// <param name="cartItemIds">购物车物品id集合</param>
        /// <returns></returns>
        object GetSubmitByCartModel(string cartItemIds = "", long shippingAddressId = 0, IEnumerable<string[]> CouponIdsStr = null, bool isStore = false)
        {
            CheckUserLogin();
            var result = OrderApplication.GetMobileSubmiteByCart(CurrentUserId, cartItemIds, shippingAddressId, CouponIdsStr);

            Mall.DTO.ShopBranch storeInfo = null;
            if (isStore)
            {
                if (result.shopBranchInfo == null)
                    throw new MallException("获取门店信息失败，不可提交非门店商品");
                else
                {
                    storeInfo = Application.ShopBranchApplication.GetShopBranchById(result.shopBranchInfo.Id);
                }
            }
            //解决循环引用的序列化的问题
            dynamic address = new System.Dynamic.ExpandoObject();
            if (result.Address != null)
            {
                var add = new
                {
                    ShippingId = result.Address.Id,
                    ShipTo = result.Address.ShipTo,
                    CellPhone = result.Address.Phone,
                    FullRegionName = result.Address.RegionFullName,
                    FullAddress = result.Address.RegionFullName + " " + result.Address.Address + " " + result.Address.AddressDetail,
                    Address = result.Address.Address,
                    RegionId = result.Address.RegionId
                };
                address = add;
            }
            else
                address = null;

            bool canIntegralPerMoney = true, canCapital = true;
            CanDeductible(out canIntegralPerMoney, out canCapital);
            var data = new
            {
                Address = address,
                IsCashOnDelivery = result.IsCashOnDelivery,
                InvoiceContext = result.InvoiceContext,
                InvoiceTitle = result.InvoiceTitle,
                cellPhone = result.cellPhone,
                email = result.email,
                vatInvoice = result.vatInvoice,
                invoiceName = result.invoiceName,//默认抬头(普通、电子)
                invoiceCode = result.invoiceCode,//默认税号(普通、电子)
                products = result.products,
                TotalAmount = result.totalAmount,
                Freight = result.Freight,
                orderAmount = result.orderAmount,
                IsOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore,
                integralPerMoney = result.integralPerMoney,
                userIntegralMaxDeductible = result.userIntegralMaxDeductible,
                integralPerMoneyRate = result.integralPerMoneyRate,
                userIntegralMaxRate = SiteSettingApplication.SiteSettings.IntegralDeductibleRate,
                userIntegrals = result.userIntegrals,
                TotalMemberIntegral = result.memberIntegralInfo.AvailableIntegrals,
                canIntegralPerMoney = canIntegralPerMoney,
                canCapital = canCapital,
                capitalAmount = result.capitalAmount,
                shopBranchInfo = storeInfo,
                ProvideInvoice = storeInfo == null ? false : ShopApplication.HasProvideInvoice(result.products.Select(s => s.shopId).Distinct().ToList())
            };
            return Json(data);
        }

        [HttpPost("SubmitOrder")]
        public object SubmitOrder(SmallProgSubmitOrderModel value)
        {
            CheckUserLogin();
            if (value.CapitalAmount > 0 && !string.IsNullOrEmpty(value.PayPwd))
            {
                var flag = MemberApplication.VerificationPayPwd(CurrentUser.Id, value.PayPwd);
                if (!flag)
                {
                    return Json(ErrorResult<dynamic>("预存款支付密码错误"));
                }
            }
            if (value.fromPage == WXSmallProgFromPageType.SignBuy)
            {
                //立即购买（限时购）
                OrderSubmitOrderModel orderModel = new OrderSubmitOrderModel();
                orderModel.counts = value.buyAmount.ToString();
                orderModel.couponIds = value.couponCode;
                orderModel.integral = (int)value.deductionPoints;
                orderModel.recieveAddressId = value.shippingId;
                orderModel.skuIds = value.productSku;
                orderModel.orderRemarks = value.remark;
                orderModel.formId = value.formId;
                orderModel.isCashOnDelivery = false;//货到付款
                orderModel.invoiceType = 0;//发票类型
                orderModel.jsonOrderShops = value.jsonOrderShops;
                orderModel.isStore = value.isStore;
                orderModel.ProductType = value.ProductType;
                orderModel.VirtualProductItems = value.VirtualProductItems;
                orderModel.Capital = value.CapitalAmount;
                //提交
                return SubmitOrderById(orderModel);
            }
            else if (value.fromPage == WXSmallProgFromPageType.Cart)
            {
                //购物车
                OrderSubmitOrderByCartModel cartModel = new OrderSubmitOrderByCartModel();
                cartModel.couponIds = value.couponCode;
                cartModel.integral = (int)value.deductionPoints;
                cartModel.recieveAddressId = value.shippingId;
                cartModel.cartItemIds = value.cartItemIds;//
                cartModel.formId = value.formId;
                cartModel.isCashOnDelivery = false;//货到付款
                cartModel.invoiceType = 0;//发票类型
                cartModel.jsonOrderShops = value.jsonOrderShops;
                cartModel.Capital = value.CapitalAmount;
                cartModel.isStore = value.isStore;
                return SubmitOrderByCart(cartModel);
            }

            return Json(ErrorResult<dynamic>("提交来源异常"));
        }
        /// <summary>
        /// 立即购买方式提交的订单
        /// </summary>
        /// <param name="value">数据</param>
        private object SubmitOrderById(OrderSubmitOrderModel value)
        {
            string skuIds = value.skuIds;
            string counts = value.counts;
            long recieveAddressId = value.recieveAddressId;
            string couponIds = value.couponIds;
            int integral = value.integral;

            bool isCashOnDelivery = value.isCashOnDelivery;
            int invoiceType = value.invoiceType;
            string invoiceTitle = value.invoiceTitle;
            string invoiceContext = value.invoiceContext;
            //end
            string orderRemarks = string.Empty;//value.orderRemarks;//订单备注

            OrderCreateModel model = new OrderCreateModel();
            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var skuIdArr = skuIds.Split(',').Select(item => item.ToString());
            var countArr = counts.Split(',').Select(item => int.Parse(item));
            model.CouponIdsStr = CouponApplication.ConvertUsedCoupon(couponIds);
            IEnumerable<long> orderIds;
            model.PlatformType = PlatformType.WeiXinSmallProg;
            model.CurrentUser = CurrentUser;
            model.ReceiveAddressId = recieveAddressId;
            model.SkuIds = skuIdArr;
            model.Counts = countArr;
            model.Integral = integral;

            model.IsCashOnDelivery = isCashOnDelivery;
            model.Invoice = (InvoiceType)invoiceType;
            model.InvoiceContext = invoiceContext;
            model.InvoiceTitle = invoiceTitle;

            model.formId = value.formId;
            model.Capital = value.Capital;

            CommonModel.OrderShop[] OrderShops = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderShop[]>(value.jsonOrderShops);
            CommonModel.VirtualProductItem[] VirtualProductItems = Newtonsoft.Json.JsonConvert.DeserializeObject<VirtualProductItem[]>(value.VirtualProductItems);
            model.OrderShops = OrderShops;//用户APP选择门店自提时用到，2.5版本未支持门店自提
            model.OrderRemarks = OrderShops.Select(p => p.Remark).ToArray();
            //end

            if (value.isStore)
                model.IsShopbranchOrder = true;

            model.IsVirtual = value.ProductType == 1;
            if (model.IsVirtual && skuIdArr.Count() > 0)
            {
                var skuInfo = ProductManagerApplication.GetSKU(skuIdArr.FirstOrDefault());
                if (skuInfo != null)
                {
                    var virtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(skuInfo.ProductId);
                    if (virtualProductInfo != null && virtualProductInfo.ValidityType && DateTime.Now > virtualProductInfo.EndDate.Value)
                    {
                        return Json(ErrorResult<dynamic>("该虚拟商品已过期，不支持下单"));
                    }
                }
            }

            try
            {
                //处理限时购
                if (skuIdArr.Count() == 1)
                {
                    var skuid = skuIdArr.ElementAt(0);
                    if (!string.IsNullOrWhiteSpace(skuid))
                    {
                        var sku = productService.GetSku(skuid);
                        var flashSale = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetLimitTimeMarketItemByProductId(sku.ProductId);
                        
                        model.IslimitBuy = flashSale!=null;//标识为限时购计算价格按限时购价格核算
                        model.FlashSaleId = flashSale != null ? flashSale.Id : 0;
                    }
                }
                var orders = orderService.CreateOrder(model);
                orderIds = orders.Select(item => item.Id).ToArray();
                decimal orderTotals = orders.Where(d => d.PaymentType != Entities.OrderInfo.PaymentTypes.CashOnDelivery).Sum(item => item.OrderTotalAmount);
                //orderIds = orderService.CreateOrder(CurrentUser.Id, skuIdArr, countArr, recieveAddressId, PlatformType);
                AddVshopBuyNumber(orderIds);//添加微店购买数量

                #region 处理虚拟订单项
                if (value.ProductType == 1 && VirtualProductItems != null && VirtualProductItems.Count() > 0)
                {
                    var orderId = orderIds.FirstOrDefault();
                    if (orderId > 0)
                    {
                        var orderItemInfo = OrderApplication.GetOrderItemsByOrderId(orderId).FirstOrDefault();
                        if (orderItemInfo != null)
                        {
                            var list = VirtualProductItems.ToList().Where(a => !string.IsNullOrWhiteSpace(a.Content)).ToList();//过滤空项
                            list.ForEach(a =>
                            {
                                a.OrderId = orderId;
                                a.OrderItemId = orderItemInfo.Id;
                                if (a.VirtualProductItemType == (sbyte)ProductInfo.VirtualProductItemType.Picture)
                                {
                                    a.Content = MoveImages(a.Content, CurrentUser.Id);
                                }
                            });
                            if (list.Count > 0)
                            {
                                OrderApplication.AddVirtualOrderItemInfo(list);
                            }
                        }
                    }
                }
                #endregion
                return Json(new { OrderId = string.Join(",", orderIds), OrderTotal = orderTotals,
                    RealTotalIsZero = (orderTotals - model.Capital) == 0,msg="提交成功"
                });
            }
            catch (MallException he)
            {
                return Json(ErrorResult<dynamic>(he.Message));
            }
        }

        private string MoveImages(string image, long userId)
        {
            List<string> content = new List<string>();
            if (string.IsNullOrWhiteSpace(image))
            {
                return "";
            }
            var list = image.Split(',').ToList();
            if (list != null && list.Count > 0)
            {
                list.ForEach(a =>
                {
                    var oldname = Path.GetFileName(a);
                    string ImageDir = string.Empty;
                    //转移图片
                    string relativeDir = "/Storage/Plat/VirtualProduct/";
                    string fileName = userId + oldname;
                    if (a.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
                    {
                        var de = a.Substring(a.LastIndexOf("/temp/"));
                        Core.MallIO.CopyFile(de, relativeDir + fileName, true);
                        content.Add(relativeDir + fileName);
                    }  //目标地址
                    else if (a.Contains("/Storage"))
                    {
                        content.Add(a.Substring(a.LastIndexOf("/Storage")));
                    }
                    else
                        content.Add(a);
                });
            }
            return string.Join(",", content);
        }

        /// <summary>
        /// 购物车方式提交的订单
        /// </summary>
        /// <param name="value">数据</param>
        private object SubmitOrderByCart(OrderSubmitOrderByCartModel value)
        {
            string cartItemIds = value.cartItemIds;
            long recieveAddressId = value.recieveAddressId;
            string couponIds = value.couponIds;
            int integral = value.integral;

            bool isCashOnDelivery = value.isCashOnDelivery;
            int invoiceType = value.invoiceType;
            string invoiceTitle = value.invoiceTitle;
            string invoiceContext = value.invoiceContext;

            OrderCreateModel model = new OrderCreateModel();
            List<OrderInfo> infos = new List<OrderInfo>();

            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            IEnumerable<long> orderIds;
            model.PlatformType = PlatformType.WeiXinSmallProg;
            model.CurrentUser = CurrentUser;
            model.ReceiveAddressId = recieveAddressId;
            model.Integral = integral;

            model.formId = value.formId;
            model.Capital = value.Capital;

            model.IsCashOnDelivery = isCashOnDelivery;
            model.Invoice = (InvoiceType)invoiceType;
            model.InvoiceContext = invoiceContext;
            model.InvoiceTitle = invoiceTitle;

            if (value.isStore)
                model.IsShopbranchOrder = true;
            //end
            CommonModel.OrderShop[] OrderShops = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderShop[]>(value.jsonOrderShops);
            model.OrderShops = OrderShops;//用户APP选择门店自提时用到，2.5版本未支持门店自提
            model.OrderRemarks = OrderShops.Select(p => p.Remark).ToArray();
            try
            {
                var cartItemIdsArr = cartItemIds.Split(',').Select(item => long.Parse(item)).ToArray();
                //根据购物车项补充sku数据
                var cartItems = CartApplication.GetCartItems(cartItemIdsArr);
                model.SkuIds = cartItems.Select(e => e.SkuId).ToList();
                model.Counts = cartItems.Select(e => e.Quantity).ToList();

                model.CartItemIds = cartItemIdsArr;
                model.CouponIdsStr = CouponApplication.ConvertUsedCoupon(couponIds);

                var orders = orderService.CreateOrder(model);
                orderIds = orders.Select(item => item.Id).ToArray();
                decimal orderTotals = orders.Where(d => d.PaymentType != Entities.OrderInfo.PaymentTypes.CashOnDelivery).Sum(item => item.OrderTotalAmount);

                return Json(new { OrderId = string.Join(",", orderIds), OrderTotal = orderTotals,
                    RealTotalIsZero = (orderTotals - model.Capital) == 0,msg= "提交成功"
                });
            }
            catch (MallException he)
            {
                return Json(ErrorResult<dynamic>(he.Message));
            }
        }

        /// <summary>
        /// 添加微店购买数量
        /// </summary>
        /// <param name="orderIds"></param>
        void AddVshopBuyNumber(IEnumerable<long> orderIds)
        {
            var shopIds = ServiceProvider.Instance<IOrderService>.Create.GetOrders(orderIds).Select(item => item.ShopId);//从订单信息获取店铺id
            var vshopService = ServiceProvider.Instance<IVShopService>.Create;
            var vshopIds = shopIds.Select(item =>
            {
                var vshop = vshopService.GetVShopByShopId(item);
                if (vshop != null)
                    return vshop.Id;
                else
                    return 0;
            }
                ).Where(item => item > 0);//从店铺id反查vshopId

            foreach (var vshopId in vshopIds)
                vshopService.AddBuyNumber(vshopId);
        }

        /// <summary>
        /// 保存发票信息(新)
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public object PostSaveInvoiceTitle(InvoiceTitleInfo para)
        {
            CheckUserLogin();
            para.UserId = CurrentUserId;
            OrderApplication.SaveInvoiceTitleNew(para);
            return Json(SuccessResult<dynamic>("保存成功"));
        }

        /// <summary>
        /// 删除发票抬头
        /// </summary>
        /// <param name="id">抬头ID</param>
        /// <returns>是否完成</returns>
        /// 
        [HttpPost("PostDeleteInvoiceTitle")]
        public object  PostDeleteInvoiceTitle(PostDeleteInvoiceTitlePModel para)
        {
            CheckUserLogin();
            OrderApplication.DeleteInvoiceTitle(para.id, CurrentUserId);
            return Json("");
        }

        /// <summary>
        /// 支付积分订单
        /// </summary>
        //public JsonResult<Result<int>> GetPayOrderByIntegral(string orderIds)
        //{
        //    CheckUserLogin();
        //    OrderApplication.ConfirmOrder(CurrentUser.Id, orderIds);
        //    return JsonResult<int>();
        //}

        /// <summary>
        /// 获取订单商品明细
        /// </summary>
        //public JsonResult<Result<List<OrderDetailView>>> GetOrderShareProduct(string orderids)
        //{
        //    CheckUserLogin();
        //    if (string.IsNullOrWhiteSpace(orderids))
        //    {
        //        throw new MallException("订单号不能为空！");
        //    }
        //    long orderId = 0;
        //    var ids = orderids.Split(',').Select(e =>
        //    {
        //        if (long.TryParse(e, out orderId))
        //        {
        //            return orderId;
        //        }
        //        else
        //        {
        //            return 0;
        //        }
        //    }
        //    );
        //    var orders = OrderApplication.GetOrderDetailViews(ids);
        //    return JsonResult(orders);
        //}

        /// <summary>
        /// 订单分享获得积分
        /// </summary>
        //public JsonResult<Result<int>> PostOrderShareAddIntegral(PostOrderShareAddIntegralModel OrderIds)
        //{
        //    CheckUserLogin();
        //    var orderids = OrderIds.orderids;
        //    if (string.IsNullOrWhiteSpace(orderids))
        //    {
        //        throw new MallException("订单号不能为空！");
        //    }
        //    long orderId = 0;
        //    var ids = orderids.Split(',').Select(e =>
        //    {
        //        if (long.TryParse(e, out orderId))
        //            return orderId;
        //        else
        //            throw new MallException("订单分享增加积分时，订单号异常！");
        //    }
        //    );
        //    if (MemberIntegralApplication.OrderIsShared(ids))
        //    {
        //        throw new MallException("订单已经分享过！");
        //    }
        //    Mall.Entities.MemberIntegralRecordInfo record = new Mall.Entities.MemberIntegralRecordInfo();
        //    record.MemberId = CurrentUser.Id;
        //    record.UserName = CurrentUser.UserName;
        //    record.RecordDate = DateTime.Now;
        //    record.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.Share;
        //    record.ReMark = string.Format("订单号:{0}", orderids);
        //    List<Mall.Entities.MemberIntegralRecordActionInfo> recordAction = new List<Mall.Entities.MemberIntegralRecordActionInfo>();

        //    foreach (var id in ids)
        //    {
        //        recordAction.Add(new Mall.Entities.MemberIntegralRecordActionInfo
        //        {
        //            VirtualItemId = id,
        //            VirtualItemTypeId = Mall.Entities.MemberIntegralInfo.VirtualItemType.ShareOrder
        //        });
        //    }
        //    record.MemberIntegralRecordActionInfo = recordAction;
        //    MemberIntegralApplication.AddMemberIntegralByEnum(record, Mall.Entities.MemberIntegralInfo.IntegralType.Share);
        //    return JsonResult<int>(msg: "晒单添加积分成功！");
        //}

        /// <summary>
        /// 获取自提门店点
        /// </summary>
        /// <returns></returns>
        //public object GetShopBranchs(long shopId, bool getParent, string skuIds, string counts, int page, int rows, long shippingAddressId, long regionId)
        //{
        //    string[] _skuIds = skuIds.Split(',');
        //    int[] _counts = counts.Split(',').Select(p => Mall.Core.Helper.TypeHelper.ObjectToInt(p)).ToArray();

        //    var shippingAddressInfo = ShippingAddressApplication.GetUserShippingAddress(shippingAddressId);
        //    int streetId = 0, districtId = 0;//收货地址的街道、区域

        //    var query = new ShopBranchQuery()
        //    {
        //        ShopId = shopId,
        //        PageNo = page,
        //        PageSize = rows,
        //        Status = ShopBranchStatus.Normal,
        //        ShopBranchProductStatus = ShopBranchSkuStatus.Normal
        //    };
        //    if (shippingAddressInfo != null)
        //    {
        //        query.FromLatLng = string.Format("{0},{1}", shippingAddressInfo.Latitude, shippingAddressInfo.Longitude);//需要收货地址的经纬度
        //        streetId = shippingAddressInfo.RegionId;
        //        var parentAreaInfo = RegionApplication.GetRegion(shippingAddressInfo.RegionId, Mall.CommonModel.Region.RegionLevel.Town);//判断当前区域是否为第四级
        //        if (parentAreaInfo != null && parentAreaInfo.ParentId > 0) districtId = parentAreaInfo.ParentId;
        //        else { districtId = streetId; streetId = 0; }
        //    }
        //    bool hasLatLng = false;
        //    if (!string.IsNullOrWhiteSpace(query.FromLatLng)) hasLatLng = query.FromLatLng.Split(',').Length == 2;

        //    var region = RegionApplication.GetRegion(regionId, getParent ? CommonModel.Region.RegionLevel.City : CommonModel.Region.RegionLevel.County);//同城内门店
        //    if (region != null) query.AddressPath = region.GetIdPath();

        //    #region 3.0版本排序规则
        //    var skuInfos = ProductManagerApplication.GetSKUs(_skuIds);
        //    query.ProductIds = skuInfos.Select(p => p.ProductId).ToArray();
        //    var data = ShopBranchApplication.GetShopBranchsAll(query);
        //    var shopBranchSkus = ShopBranchApplication.GetSkus(shopId, data.Models.Select(p => p.Id).ToList());//获取该商家下具有订单内所有商品的门店状态正常数据,不考虑库存
        //    data.Models.ForEach(p =>
        //    {
        //        p.Enabled = skuInfos.All(skuInfo => shopBranchSkus.Any(sbSku => sbSku.ShopBranchId == p.Id && sbSku.Stock >= _counts[skuInfos.IndexOf(skuInfo)] && sbSku.SkuId == skuInfo.Id));
        //    });

        //    List<Mall.DTO.ShopBranch> newList = new List<Mall.DTO.ShopBranch>();
        //    List<long> fillterIds = new List<long>();
        //    var currentList = data.Models.Where(p => hasLatLng && p.Enabled && (p.Latitude > 0 && p.Longitude > 0)).OrderBy(p => p.Distance).ToList();
        //    if (currentList != null && currentList.Count() > 0)
        //    {
        //        fillterIds.AddRange(currentList.Select(p => p.Id));
        //        newList.AddRange(currentList);
        //    }
        //    var currentList2 = data.Models.Where(p => !fillterIds.Contains(p.Id) && p.Enabled && p.AddressPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + streetId + CommonConst.ADDRESS_PATH_SPLIT)).ToList();
        //    if (currentList2 != null && currentList2.Count() > 0)
        //    {
        //        fillterIds.AddRange(currentList2.Select(p => p.Id));
        //        newList.AddRange(currentList2);
        //    }
        //    var currentList3 = data.Models.Where(p => !fillterIds.Contains(p.Id) && p.Enabled && p.AddressPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + districtId + CommonConst.ADDRESS_PATH_SPLIT)).ToList();
        //    if (currentList3 != null && currentList3.Count() > 0)
        //    {
        //        fillterIds.AddRange(currentList3.Select(p => p.Id));
        //        newList.AddRange(currentList3);
        //    }
        //    var currentList4 = data.Models.Where(p => !fillterIds.Contains(p.Id) && p.Enabled).ToList();//非同街、非同区，但一定会同市
        //    if (currentList4 != null && currentList4.Count() > 0)
        //    {
        //        fillterIds.AddRange(currentList4.Select(p => p.Id));
        //        newList.AddRange(currentList4);
        //    }
        //    var currentList5 = data.Models.Where(p => !fillterIds.Contains(p.Id)).ToList();//库存不足的排最后
        //    if (currentList5 != null && currentList5.Count() > 0)
        //    {
        //        newList.AddRange(currentList5);
        //    }
        //    if (newList.Count() != data.Models.Count())//如果新组合的数据与原数据数量不一致
        //    {
        //        return Json(ErrorResult<dynamic>("数据异常"));
        //    }
        //    var storeList = newList.Select(sb =>
        //    {
        //        return new
        //        {
        //            ContactUser = sb.ContactUser,
        //            ContactPhone = sb.ContactPhone,
        //            AddressDetail = sb.AddressDetail,
        //            ShopBranchName = sb.ShopBranchName,
        //            Id = sb.Id,
        //            Enabled = sb.Enabled
        //        };
        //    });

        //    #endregion

        //    return Json(storeList);
        //}

        /// <summary>
        /// 是否允许门店自提
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="regionId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        //public JsonResult<Result<bool>> GetExistShopBranch(long shopId, long regionId, string productIds)
        //{
        //    var query = new ShopBranchQuery();
        //    query.Status = ShopBranchStatus.Normal;
        //    query.ShopId = shopId;

        //    var region = RegionApplication.GetRegion(regionId, CommonModel.Region.RegionLevel.City);
        //    query.AddressPath = region.GetIdPath();
        //    query.ProductIds = productIds.Split(',').Select(p => long.Parse(p)).ToArray();
        //    query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
        //    var existShopBranch = ShopBranchApplication.Exists(query);
        //    return JsonResult(existShopBranch);
        //}

        /// <summary>
        /// 获取拼团订单提交页面数据
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="GroupActionId"></param>
        /// <param name="GroupId"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetGroupOrderModel")]
        public object GetGroupOrderModel(string skuId, int count, long GroupActionId, long? GroupId = null)
        {
            CheckUserLogin();
            dynamic d = new System.Dynamic.ExpandoObject();
            Mall.DTO.MobileOrderDetailConfirmModel result = OrderApplication.SubmitByGroupId(CurrentUser.Id, skuId, count, GroupActionId, GroupId);
            if (result.Address != null)
            {
                //result.Address.MemberInfo = new UserMemberInfo();
                d.Address = result.Address;
            }

            bool canIntegralPerMoney = true, canCapital = true;
            CanDeductible(out canIntegralPerMoney, out canCapital);
            d.canIntegralPerMoney = canIntegralPerMoney;
            d.canCapital = canCapital;

            d.IsOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            d.ProvideInvoice = ShopApplication.HasProvideInvoice(result.products.Select(s => s.shopId).Distinct().ToList());
            d.products = result.products;
            d.totalAmount = result.totalAmount;
            d.Freight = result.Freight;
            d.orderAmount = result.orderAmount;
            d.integralPerMoney = result.integralPerMoney;
            d.integralPerMoneyRate = result.integralPerMoneyRate;
            d.userIntegralMaxDeductible = result.userIntegralMaxDeductible;
            d.IntegralDeductibleRate = result.IntegralDeductibleRate;
            d.userIntegrals = result.userIntegrals;
            d.capitalAmount = result.capitalAmount;
            d.memberIntegralInfo = result.memberIntegralInfo;
            d.InvoiceContext = result.InvoiceContext;//发票类容
            d.InvoiceTitle = result.InvoiceTitle;//发票抬头
            d.cellPhone = result.cellPhone;//默认收票人手机
            d.email = result.email;//默认收票人邮箱
            d.vatInvoice = result.vatInvoice;//默认增值税发票
            d.invoiceName = result.invoiceName;//默认抬头(普通、电子)
            d.invoiceCode = result.invoiceCode;//默认税号(普通、电子)
            d.IsCashOnDelivery = result.IsCashOnDelivery;
            d.Sku = result.Sku;
            d.Count = result.Count;
            d.shopBranchInfo = result.shopBranchInfo;

            return Json(result);
        }

        /// <summary>
        /// 拼团订单提交
        /// </summary>
        /// <param name="value">表单数据</param>
        /// <returns></returns>
        /// 
        [HttpPost("PostSubmitFightGroupOrder")]
        public object PostSubmitFightGroupOrder(OrderSubmitFightGroupOrderModel value)
        {
            CheckUserLogin();
            if (value.CapitalAmount > 0 && !string.IsNullOrEmpty(value.PayPwd))
            {
                var flag = MemberApplication.VerificationPayPwd(CurrentUser.Id, value.PayPwd);
                if (!flag)
                {
                    return Json(ErrorResult<dynamic>("预存款支付密码错误！"));
                }
            }
            string skuIds = value.skuId;
            long counts = value.count;
            long recieveAddressId = value.recieveAddressId;
            long activeId = value.GroupActionId;
            long groupId = value.GroupId;

            bool isCashOnDelivery = value.isCashOnDelivery;
            int invoiceType = value.invoiceType;
            string invoiceTitle = value.invoiceTitle;
            string invoiceContext = value.invoiceContext;
            string invoiceCode = value.invoiceCode;

            decimal capitalAmount = value.CapitalAmount;

            string orderRemarks = "";//value.orderRemarks;//订单备注
            List<FightGroupOrderJoinStatus> seastatus = new List<FightGroupOrderJoinStatus>();
            seastatus.Add(FightGroupOrderJoinStatus.Ongoing);
            seastatus.Add(FightGroupOrderJoinStatus.JoinSuccess);
            var groupData = ServiceProvider.Instance<IFightGroupService>.Create.GetActive(activeId, false, false);

            if (counts > groupData.LimitQuantity)
            {
                return Json(ErrorResult<dynamic>(string.Format("每人限购数量：{0}！", groupData.LimitQuantity)));
            }
            try
            {
                var model = OrderApplication.GetGroupOrder(CurrentUser.Id, skuIds, counts.ToString(), recieveAddressId, invoiceType, invoiceTitle, invoiceCode, invoiceContext, activeId, PlatformType.WeiXinSmallProg, groupId, isCashOnDelivery, orderRemarks, capitalAmount);
                CommonModel.OrderShop[] OrderShops = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderShop[]>(value.jsonOrderShops);
                model.OrderShops = OrderShops;//用户APP选择门店自提时用到，2.5版本未支持门店自提
                model.OrderRemarks = OrderShops.Select(p => p.Remark).ToArray();
                var ret = OrderApplication.OrderSubmit(model);
                AddVshopBuyNumber(ret.OrderIds);//添加微店购买数量
                return Json(new { OrderIds = ret.OrderIds, RealTotalIsZero = ret.OrderTotal == 0 });
            }
            catch (MallException he)
            {
                return Json(ErrorResult<dynamic>(he.Message));
            }
            catch (Exception ex)
            {
                return Json(ErrorResult<dynamic>("提交订单异常" + ex.Message));
            }
        }
    }
}
