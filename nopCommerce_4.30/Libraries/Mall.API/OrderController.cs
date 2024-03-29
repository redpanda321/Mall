﻿using Mall.API.Model.ParamsModel;
using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Service;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mall.API
{
    public class OrderController : BaseApiController
    {
        private IOrderService _iOrderService;
        private IMemberIntegralService _iMemberIntegralService;
        private ICartService _iCartService;
        private IMemberService _iMemberService;
        private IProductService _iProductService;
        private IPaymentConfigService _iPaymentConfigService;
        private IShippingAddressService _iShippingAddressService;
        private IRegionService _iRegionService;
        private ICashDepositsService _iCashDepositsService;
        private IShopService _iShopService;
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private ICouponService _iCouponService;
        private IShopBonusService _iShopBonusService;
        private ICollocationService _iCollocationService;
        private IMemberCapitalService _iMemberCapitalService;
        private IVShopService _iVShopService;
        private IRefundService _iRefundService;
        private IFightGroupService _iFightGroupService;
        public OrderController()
        {
            _iOrderService = new OrderService();
            _iCartService = new CartService();
            _iMemberService = new MemberService();
            _iProductService = new ProductService();
            _iPaymentConfigService = new PaymentConfigService();
            _iCashDepositsService = new CashDepositsService();
            _iShopService = ServiceProvider.Instance<IShopService>.Create;
            _iLimitTimeBuyService = new LimitTimeBuyService();
            _iCouponService = new CouponService();
            _iShopBonusService = new ShopBonusService();
            _iCollocationService = new CollocationService();
            _iMemberCapitalService = new MemberCapitalService();
            _iShippingAddressService = new ShippingAddressService();
            _iMemberIntegralService = new MemberIntegralService();
            _iRegionService = new RegionService();
            _iVShopService = new VShopService();
            _iRefundService = new RefundService();
            _iFightGroupService = new FightGroupService();
        }
        /// <summary>
        /// 获取立即购买提交页面的数据
        /// </summary>
        /// <param name="skuIds">库存ID集合</param>
        /// <param name="counts">库存ID对应的数量</param>
        /// 

        [HttpGet("GetSubmitModel")]
        public object GetSubmitModel(string skuId, int count, long shippingAddressId = 0, string couponIds = "")
        {
            CheckUserLogin();
            var coupons = CouponApplication.ConvertUsedCoupon(couponIds);
            var result = OrderApplication.GetMobileSubmit(CurrentUserId, skuId.ToString(), count.ToString(), shippingAddressId, coupons);
            dynamic d = SuccessResult();

            if (result.Address != null && !result.Address.NeedUpdate)
            {
                var addDetail = result.Address.AddressDetail ?? "";
                var add = new
                {
                    Id = result.Address.Id,
                    ShipTo = result.Address.ShipTo,
                    Phone = result.Address.Phone,
                    Address = result.Address.RegionFullName + " " + result.Address.Address + " " + addDetail,
                    RegionId = result.Address.RegionId
                };
                d.Address = add;
            }
            else
                d.Address = null;

            bool canIntegralPerMoney = true, canCapital = true;
            CanDeductible(out canIntegralPerMoney, out canCapital);

            d.canIntegralPerMoney = canIntegralPerMoney;
            d.canCapital = canCapital;
            //发票信息
            d.InvoiceContext = result.InvoiceContext;//发票类容
            d.InvoiceTitle = result.InvoiceTitle;//发票抬头
            d.cellPhone = result.cellPhone;//默认收票人手机
            d.email = result.email;//默认收票人邮箱
            d.vatInvoice = result.vatInvoice;//默认增值税发票
            d.invoiceName = result.invoiceName;//默认抬头(普通、电子)
            d.invoiceCode = result.invoiceCode;//默认税号(普通、电子)
            d.products = result.products;
            d.capitalAmount = result.capitalAmount;
            d.TotalAmount = result.totalAmount;
            d.Freight = result.Freight;
            d.orderAmount = result.orderAmount;
            d.IsCashOnDelivery = result.IsCashOnDelivery;
            d.IsOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            d.ProvideInvoice = ShopApplication.HasProvideInvoice(result.products.Select(s => s.shopId).Distinct().ToList());

            d.integralPerMoney = result.integralPerMoney;
            d.userIntegralMaxDeductible = result.userIntegralMaxDeductible;
            d.integralPerMoneyRate = result.integralPerMoneyRate;
            d.userIntegralMaxRate = SiteSettingApplication.SiteSettings.IntegralDeductibleRate;
            d.userIntegrals = result.userIntegrals;
            d.TotalMemberIntegral = result.memberIntegralInfo.AvailableIntegrals;
            d.productType = result.ProductType;
            string shipperAddress = string.Empty, shipperTelPhone = string.Empty;
            if (result.ProductType == 1)
            {
                var virtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(result.ProductId);
                if (virtualProductInfo != null && virtualProductInfo.ValidityType && DateTime.Now > virtualProductInfo.EndDate.Value)
                {
                    throw new MallException("该虚拟商品已过期，不支持下单");
                }
                if (result.products != null && result.products.Count > 0)
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
            return d;
        }

        /// <summary>
        /// 获取购物车提交页面的数据
        /// </summary>
        /// <param name="cartItemIds">购物车物品id集合</param>
        /// <returns></returns>
        /// 
        [HttpGet("GetSubmitByCartModel")]
        public object GetSubmitByCartModel(string cartItemIds = "", long shippingAddressId = 0, string couponIds = "")
        {
            CheckUserLogin();
            var coupons = CouponApplication.ConvertUsedCoupon(couponIds);
            var result = OrderApplication.GetMobileSubmiteByCart(CurrentUserId, cartItemIds, shippingAddressId, coupons);

            //解决循环引用的序列化的问题
            dynamic address = new System.Dynamic.ExpandoObject();
            if (result.Address != null && !result.Address.NeedUpdate)
            {
                var addDetail = result.Address.AddressDetail ?? "";
                var add = new
                {
                    Id = result.Address.Id,
                    ShipTo = result.Address.ShipTo,
                    Phone = result.Address.Phone,
                    Address = result.Address.RegionFullName + " " + result.Address.Address + " " + addDetail,
                    RegionId = result.Address.RegionId
                };
                address = add;
            }
            else
                address = null;

            bool canIntegralPerMoney = true, canCapital = true;
            CanDeductible(out canIntegralPerMoney, out canCapital);

            var _result = new
            {
                success = true,
                Address = address,
                IsCashOnDelivery = result.IsCashOnDelivery,
                InvoiceContext = result.InvoiceContext,
                InvoiceTitle = result.InvoiceTitle,
                cellPhone = result.cellPhone,
                email = result.email,
                invoiceName = result.invoiceName,//默认抬头(普通、电子)
                invoiceCode = result.invoiceCode,//默认税号(普通、电子)
                vatInvoice = result.vatInvoice,
                products = result.products,
                capitalAmount = result.capitalAmount,
                TotalAmount = result.totalAmount,
                Freight = result.Freight,
                orderAmount = result.orderAmount,
                IsOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore,
                ProvideInvoice = ShopApplication.HasProvideInvoice(result.products.Select(s => s.shopId).Distinct().ToList()),

                integralPerMoney = result.integralPerMoney,
                userIntegralMaxDeductible = result.userIntegralMaxDeductible,
                integralPerMoneyRate = result.integralPerMoneyRate,
                userIntegralMaxRate = SiteSettingApplication.SiteSettings.IntegralDeductibleRate,
                userIntegrals = result.userIntegrals,
                TotalMemberIntegral = result.memberIntegralInfo.AvailableIntegrals,
                canIntegralPerMoney = canIntegralPerMoney,
                canCapital = canCapital
            };
            return _result;
        }

        /// <summary>
        /// 立即购买方式提交的订单
        /// </summary>
        /// <param name="value">数据</param>
        /// 
        [HttpPost("PostSubmitOrder")]
        public object PostSubmitOrder(OrderSubmitOrderModel value)
        {
            CheckUserLogin();
            if (value.CapitalAmount > 0 && !string.IsNullOrEmpty(value.PayPwd))
            {
                var flag = MemberApplication.VerificationPayPwd(CurrentUser.Id, value.PayPwd);
                if (!flag)
                {
                    return ErrorResult("预存款支付密码错误");
                }
            }
            string skuIds = value.skuIds;
            string counts = value.counts;
            long recieveAddressId = value.recieveAddressId;
            string couponIds = value.couponIds;
            int integral = value.integral;

            bool isCashOnDelivery = value.isCashOnDelivery;
            int invoiceType = value.invoiceType;
            string invoiceTitle = value.invoiceTitle;
            string invoiceContext = value.invoiceContext;
            string invoiceCode = value.invoiceCode;

            //使用的预存款
            decimal capitalAmount = value.CapitalAmount;

            //end
            string orderRemarks = string.Empty;//value.orderRemarks;//订单备注

            OrderCreateModel model = new OrderCreateModel();
            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var skuIdArr = skuIds.Split(',').Select(item => item.ToString());
            var countArr = counts.Split(',').Select(item => int.Parse(item));
            model.CouponIdsStr = CouponApplication.ConvertUsedCoupon(couponIds);
            IEnumerable<long> orderIds;
            model.PlatformType = PlatformType.Android;
            model.CurrentUser = CurrentUser;
            model.ReceiveAddressId = recieveAddressId;
            model.SkuIds = skuIdArr;
            model.Counts = countArr;
            model.Integral = integral;

            model.Capital = capitalAmount;//预存款

            model.IsCashOnDelivery = isCashOnDelivery;
            model.Invoice = (InvoiceType)invoiceType;
            model.InvoiceContext = invoiceContext;
            model.InvoiceTitle = invoiceTitle;
            model.InvoiceCode = invoiceCode;

            CommonModel.OrderShop[] OrderShops = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderShop[]>(value.jsonOrderShops);
            CommonModel.VirtualProductItem[] VirtualProductItems = Newtonsoft.Json.JsonConvert.DeserializeObject<VirtualProductItem[]>(value.VirtualProductItems);
            model.OrderShops = OrderShops;//用户APP选择门店自提时用到，2.5版本未支持门店自提
            model.OrderRemarks = OrderShops.Select(p => p.Remark).ToArray();

            //end
            model.IsVirtual = value.ProductType == 1;
            if (model.IsVirtual && skuIdArr.Count() > 0)
            {
                var skuInfo = ProductManagerApplication.GetSKU(skuIdArr.FirstOrDefault());
                if (skuInfo != null)
                {
                    var virtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(skuInfo.ProductId);
                    if (virtualProductInfo != null && virtualProductInfo.ValidityType && DateTime.Now > virtualProductInfo.EndDate.Value)
                    {
                        return ErrorResult("该虚拟商品已过期，不支持下单");
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
                decimal orderTotals = orders.Sum(item => item.OrderTotalAmount) - capitalAmount;
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
                return new { success = true, OrderIds = orderIds, RealTotalIsZero = orderTotals == 0 };
            }
            catch (MallException he)
            {
                return ErrorResult(he.Message);
            }
            catch (Exception ex)
            {
                return ErrorResult("提交订单异常" + ex.Message);
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
        /// 
        [HttpPost("PostSubmitOrderByCart")]
        public object PostSubmitOrderByCart(OrderSubmitOrderByCartModel value)
        {
            CheckUserLogin();
            if (value.CapitalAmount > 0 && !string.IsNullOrEmpty(value.PayPwd))
            {
                var flag = MemberApplication.VerificationPayPwd(CurrentUser.Id, value.PayPwd);
                if (!flag)
                {
                    return ErrorResult("预存款支付密码错误");
                }
            }
            string cartItemIds = value.cartItemIds;
            long recieveAddressId = value.recieveAddressId;
            string couponIds = value.couponIds;
            int integral = value.integral;

            decimal capitalAmount = value.CapitalAmount;

            bool isCashOnDelivery = value.isCashOnDelivery;
            int invoiceType = value.invoiceType;
            string invoiceTitle = value.invoiceTitle;
            string invoiceContext = value.invoiceContext;
            string invoiceCode = value.invoiceCode;

            OrderCreateModel model = new OrderCreateModel();
            var orderService = ServiceProvider.Instance<IOrderService>.Create;
            IEnumerable<long> orderIds;
            model.PlatformType = PlatformType.Android;
            model.CurrentUser = CurrentUser;
            model.ReceiveAddressId = recieveAddressId;
            model.Integral = integral;

            model.Capital = capitalAmount;//预存款

            model.IsCashOnDelivery = isCashOnDelivery;
            model.Invoice = (InvoiceType)invoiceType;
            model.InvoiceContext = invoiceContext;
            model.InvoiceTitle = invoiceTitle;
            model.InvoiceCode = invoiceCode;
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
                return new { success = true, OrderIds = orderIds, RealTotalIsZero = orderTotals == 0 };
            }
            catch (MallException he)
            {
                return ErrorResult(he.Message);
            }
            catch (Exception ex)
            {
                return ErrorResult("提交订单异常");
            }
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
                    return ErrorResult("预存款支付密码错误");
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
                return ErrorResult(string.Format("每人限购数量：{0}！", groupData.LimitQuantity));
            }
            try
            {
                var model = OrderApplication.GetGroupOrder(CurrentUser.Id, skuIds, counts.ToString(), recieveAddressId, invoiceType, invoiceTitle, invoiceCode, invoiceContext, activeId, PlatformType.Android, groupId, isCashOnDelivery, orderRemarks, capitalAmount);
                CommonModel.OrderShop[] OrderShops = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderShop[]>(value.jsonOrderShops);
                model.OrderShops = OrderShops;//用户APP选择门店自提时用到，2.5版本未支持门店自提
                model.OrderRemarks = OrderShops.Select(p => p.Remark).ToArray();
                var ret = OrderApplication.OrderSubmit(model);
                AddVshopBuyNumber(ret.OrderIds);//添加微店购买数量
                return new { success = true, OrderIds = ret.OrderIds, RealTotalIsZero = ret.OrderTotal == 0 };
            }
            catch (MallException he)
            {
                return ErrorResult(he.Message);
            }
            catch (Exception ex)
            {
                return ErrorResult("提交订单异常" + ex.Message);
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
        /// 是否可用积分购买
        /// </summary>
        /// <param name="orderIds">订单id</param>
        /// 
        [HttpGet("GetPayOrderByIntegral")]
        public object GetPayOrderByIntegral(string orderIds)
        {
            CheckUserLogin();
            var orderIdArr = orderIds.Split(',').Select(item => long.Parse(item));
            var service = ServiceApplication.Create<IOrderService>();
            service.ConfirmZeroOrder(orderIdArr, CurrentUser.Id);
            return SuccessResult();
        }

        /// <summary>
        /// 获取拼团订单信息Model
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
            dynamic d = SuccessResult();
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
            //d.InvoiceContext = result.InvoiceContext;
            //d.InvoiceTitle = result.InvoiceTitle;
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
            return d;
        }
        [HttpGet("GetOrderShareProduct")]
        public object GetOrderShareProduct(string orderids)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(orderids))
            {
                throw new MallException("订单号不能为空！");
            }
            long orderId = 0;
            var ids = orderids.Split(',').Select(e =>
            {
                if (long.TryParse(e, out orderId))
                {
                    return orderId;
                }
                else
                {
                    return 0;
                }
            }
            );
            var orders = OrderApplication.GetOrderDetailViews(ids);
            return new { success = true, OrderDetail = orders };
        }

        [HttpPost("PostOrderShareAddIntegral")]
        public object PostOrderShareAddIntegral(OrderShareAddIntegralModel OrderIds)
        {
            CheckUserLogin();
            var orderids = OrderIds.orderids;
            if (string.IsNullOrWhiteSpace(orderids))
            {
                throw new MallException("订单号不能为空！");
            }
            long orderId = 0;
            var ids = orderids.Split(',').Select(e =>
            {
                if (long.TryParse(e, out orderId))
                    return orderId;
                else
                    throw new MallException("订单分享增加积分时，订单号异常！");
            }
            );
            if (MemberIntegralApplication.OrderIsShared(ids))
            {
                throw new MallException("订单已经分享过！");
            }
            Mall.Entities.MemberIntegralRecordInfo record = new Mall.Entities.MemberIntegralRecordInfo();
            record.MemberId = CurrentUser.Id;
            record.UserName = CurrentUser.UserName;
            record.RecordDate = DateTime.Now;
            record.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.Share;
            record.ReMark = string.Format("订单号:{0}", orderids);
            List<Mall.Entities.MemberIntegralRecordActionInfo> recordAction = new List<Mall.Entities.MemberIntegralRecordActionInfo>();

            foreach (var id in ids)
            {
                recordAction.Add(new Mall.Entities.MemberIntegralRecordActionInfo
                {
                    VirtualItemId = id,
                    VirtualItemTypeId = Mall.Entities.MemberIntegralInfo.VirtualItemType.ShareOrder
                });
            }
            record.MemberIntegralRecordActionInfo = recordAction;
            MemberIntegralApplication.AddMemberIntegralByEnum(record, Mall.Entities.MemberIntegralInfo.IntegralType.Share);
            return new { success = true, msg = "晒单添加积分成功！" };
        }

        /// <summary>
        /// 获取自提门店点
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetShopBranchs")]
        public object GetShopBranchs(long shopId, bool getParent, string skuIds, string counts, int page, int rows, long shippingAddressId, long regionId)
        {
            string[] _skuIds = skuIds.Split(',');
            int[] _counts = counts.Split(',').Select(p => Mall.Core.Helper.TypeHelper.ObjectToInt(p)).ToArray();

            var shippingAddressInfo = ShippingAddressApplication.GetUserShippingAddress(shippingAddressId);
            int streetId = 0, districtId = 0;//收货地址的街道、区域

            var query = new ShopBranchQuery()
            {
                ShopId = shopId,
                PageNo = page,
                PageSize = rows,
                Status = ShopBranchStatus.Normal,
                ShopBranchProductStatus = ShopBranchSkuStatus.Normal,
                IsAboveSelf = true    //自提点，只取自提门店
            };
            if (shippingAddressInfo != null)
            {
                query.FromLatLng = string.Format("{0},{1}", shippingAddressInfo.Latitude, shippingAddressInfo.Longitude);//需要收货地址的经纬度
                streetId = shippingAddressInfo.RegionId;
                var parentAreaInfo = RegionApplication.GetRegion(shippingAddressInfo.RegionId, Region.RegionLevel.Town);//判断当前区域是否为第四级
                if (parentAreaInfo != null && parentAreaInfo.ParentId > 0) districtId = parentAreaInfo.ParentId;
                else { districtId = streetId; streetId = 0; }
            }
            bool hasLatLng = false;
            if (!string.IsNullOrWhiteSpace(query.FromLatLng)) hasLatLng = query.FromLatLng.Split(',').Length == 2;

            var region = RegionApplication.GetRegion(regionId, getParent ? CommonModel.Region.RegionLevel.City : CommonModel.Region.RegionLevel.County);//同城内门店
            if (region != null) query.AddressPath = region.GetIdPath();

            #region 3.0版本排序规则
            var skuInfos = ProductManagerApplication.GetSKUs(_skuIds);
            query.ProductIds = skuInfos.Select(p => p.ProductId).ToArray();
            var data = ShopBranchApplication.GetShopBranchsAll(query);
            var shopBranchSkus = ShopBranchApplication.GetSkus(shopId, data.Models.Select(p => p.Id).ToList());//获取该商家下具有订单内所有商品的门店状态正常数据,不考虑库存
            data.Models.ForEach(p =>
            {
                p.Enabled = skuInfos.All(skuInfo => shopBranchSkus.Any(sbSku => sbSku.ShopBranchId == p.Id && sbSku.Stock >= _counts[skuInfos.IndexOf(skuInfo)] && sbSku.SkuId == skuInfo.Id));
            });

            List<Mall.DTO.ShopBranch> newList = new List<Mall.DTO.ShopBranch>();
            List<long> fillterIds = new List<long>();
            var currentList = data.Models.Where(p => hasLatLng && p.Enabled && (p.Latitude > 0 && p.Longitude > 0)).OrderBy(p => p.Distance).ToList();
            if (currentList != null && currentList.Count() > 0)
            {
                fillterIds.AddRange(currentList.Select(p => p.Id));
                newList.AddRange(currentList);
            }
            var currentList2 = data.Models.Where(p => !fillterIds.Contains(p.Id) && p.Enabled && p.AddressPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + streetId + CommonConst.ADDRESS_PATH_SPLIT)).ToList();
            if (currentList2 != null && currentList2.Count() > 0)
            {
                fillterIds.AddRange(currentList2.Select(p => p.Id));
                newList.AddRange(currentList2);
            }
            var currentList3 = data.Models.Where(p => !fillterIds.Contains(p.Id) && p.Enabled && p.AddressPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + districtId + CommonConst.ADDRESS_PATH_SPLIT)).ToList();
            if (currentList3 != null && currentList3.Count() > 0)
            {
                fillterIds.AddRange(currentList3.Select(p => p.Id));
                newList.AddRange(currentList3);
            }
            var currentList4 = data.Models.Where(p => !fillterIds.Contains(p.Id) && p.Enabled).ToList();//非同街、非同区，但一定会同市
            if (currentList4 != null && currentList4.Count() > 0)
            {
                fillterIds.AddRange(currentList4.Select(p => p.Id));
                newList.AddRange(currentList4);
            }
            var currentList5 = data.Models.Where(p => !fillterIds.Contains(p.Id)).ToList();//库存不足的排最后
            if (currentList5 != null && currentList5.Count() > 0)
            {
                newList.AddRange(currentList5);
            }
            if (newList.Count() != data.Models.Count())//如果新组合的数据与原数据数量不一致
            {
                return new
                {
                    success = false
                };
            }
            var needDistance = false;
            if (shippingAddressInfo != null && shippingAddressInfo.Latitude != 0 && shippingAddressInfo.Longitude != 0)
            {
                needDistance = true;
            }
            var storeList = newList.Select(sb =>
            {
                return new
                {
                    ContactUser = sb.ContactUser,
                    ContactPhone = sb.ContactPhone,
                    AddressDetail = sb.AddressDetail,
                    ShopBranchName = sb.ShopBranchName,
                    Id = sb.Id,
                    Enabled = sb.Enabled,
                    Distance = needDistance ? RegionApplication.GetDistance(sb.Latitude, sb.Longitude, shippingAddressInfo.Latitude, shippingAddressInfo.Longitude) : 0
                };
            });

            #endregion

            var result = new
            {
                success = true,
                StoreList = storeList
            };
            return result;
        }

        /// <summary>
        /// 是否允许门店自提
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="regionId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetExistShopBranch")]
        public object GetExistShopBranch(long shopId, long regionId, string productIds)
        {
            var query = new ShopBranchQuery();
            query.Status = ShopBranchStatus.Normal;
            query.ShopId = shopId;

            var region = RegionApplication.GetRegion(regionId, CommonModel.Region.RegionLevel.City);
            query.AddressPath = region.GetIdPath();
            query.ProductIds = productIds.Split(',').Select(p => long.Parse(p)).ToArray();
            query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
            var existShopBranch = ShopBranchApplication.Exists(query);

            return new { success = true, ExistShopBranch = existShopBranch ? 1 : 0 };
        }
        /// <summary>
        /// 删除发票抬头
        /// </summary>
        /// <param name="id">抬头ID</param>
        /// <returns>是否完成</returns>
        /// 
        [HttpPost("PostDeleteInvoiceTitle")]
        public object PostDeleteInvoiceTitle(PostDeleteInvoiceTitlePModel para)
        {
            CheckUserLogin();
            OrderApplication.DeleteInvoiceTitle(para.id, CurrentUserId);
            return new { success = true };
        }
        /// <summary>
        /// 设置发票抬头
        /// </summary>
        /// <param name="name">抬头名称</param>
        /// <returns>返回抬头ID</returns>
        [HttpPost("PostSaveInvoiceTitle")]
        public object PostSaveInvoiceTitle(PostSaveInvoiceTitlePModel para)
        {
            CheckUserLogin();
            return OrderApplication.SaveInvoiceTitle(CurrentUserId, para.name, para.code);
        }

        /// <summary>
        /// 保存发票信息(新)
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        /// 
        [HttpPost("PostSaveInvoiceTitleNew")]
        public object PostSaveInvoiceTitleNew(InvoiceTitleInfo para)
        {
            CheckUserLogin();
            para.UserId = CurrentUserId;
            OrderApplication.SaveInvoiceTitleNew(para);
            return new { success = true, msg = "保存成功" };
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
                if (!(siteInfo.IsOpenPC || siteInfo.IsOpenH5 || siteInfo.IsOpenApp))
                {
                    canCapital = false;
                }
            }
        }
    }
}