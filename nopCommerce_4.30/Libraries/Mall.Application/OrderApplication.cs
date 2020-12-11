using AutoMapper;
using Mall.CommonModel;
using Mall.CommonModel.Delegates;
using Mall.CommonModel.Enum;
using Mall.Core;
using Mall.Core.Plugins.Message;
using Mall.Core.Plugins.Payment;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mall.Application
{
    public class OrderApplication : BaseApplicaion<IOrderService>
    {
        #region 字段
        private static IMemberIntegralService _iMemberIntegralService =  EngineContext.Current.Resolve<IMemberIntegralService>();
        private static IMemberCapitalService _iMemberCapitalService =  EngineContext.Current.Resolve<IMemberCapitalService>();
        private static IProductService _iProductService =  EngineContext.Current.Resolve<IProductService>();
        private static ILimitTimeBuyService _iLimitTimeBuyService =  EngineContext.Current.Resolve<ILimitTimeBuyService>();
        private static ITypeService _iTypeService =  EngineContext.Current.Resolve<ITypeService>();
        private static IRefundService _iRefundService =  EngineContext.Current.Resolve<IRefundService>();
        private static IExpressService _iExpressService =  EngineContext.Current.Resolve<IExpressService>();
        private static IShopBranchService _iShopBranchService =  EngineContext.Current.Resolve<IShopBranchService>();


     //   private static IMemberIntegralService _iMemberIntegralService =  EngineContext.Current.Resolve<IMemberIntegralService>();
    //    private static IMemberCapitalService _iMemberCapitalService =  EngineContext.Current.Resolve<IMemberCapitalService>();
   //     private static IProductService _iProductService =  EngineContext.Current.Resolve<IProductService>();
   //     private static ILimitTimeBuyService _iLimitTimeBuyService =  EngineContext.Current.Resolve<ILimitTimeBuyService>();
   //     private static ITypeService _iTypeService =  EngineContext.Current.Resolve<ITypeService>();
  //      private static IRefundService _iRefundService =  EngineContext.Current.Resolve<IRefundService>();
  //      private static IExpressService _iExpressService =  EngineContext.Current.Resolve<IExpressService>();
    //    private static IShopBranchService _iShopBranchService =  EngineContext.Current.Resolve<IShopBranchService>();


        #endregion

        #region 属性
        /// <summary>
        /// 订单支付成功事件
        /// </summary>
        public static event OrderPaySuccessed OnOrderPaySuccessed
        {
            add
            {
                Service.OnOrderPaySuccessed += value;
            }
            remove
            {
                Service.OnOrderPaySuccessed -= value;
            }
        }
        #endregion

        #region web公共方法

        /// <summary>
        /// 立即购买提交订单时调用的POST方法
        /// </summary>
        /// <param name="userid">用户标识</param>
        /// <param name="skuIds">库存标识集合</param>
        /// <param name="counts">每个库存购买数量</param>
        /// <param name="recieveAddressId">客户收货区域ID</param>
        /// <param name="couponIds">商品对应的优惠券ID集合</param>
        /// <param name="invoiceType">发票类型0不要发票1增值税发票2普通发票</param>
        /// <param name="invoiceTitle">发票抬头</param>
        /// <param name="invoiceContext">发票内容</param>
        /// <param name="integral">使用积分</param>
        /// <param name="collIds">组合购Id集合</param>
        /// <param name="PlatformType">订单来源平台</param>
        /// <returns>订单集合,是否操作成功</returns>
        public static OrderReturnModel SubmitOrder(CommonModel.OrderPostModel submitModel, string payPwd = "")
        {
            if (submitModel.Capital > 0 && !string.IsNullOrEmpty(payPwd))
            {
                var flag = MemberApplication.VerificationPayPwd(((MemberInfo)submitModel.CurrentUser).Id, payPwd);
                if (!flag)
                {
                    throw new MallException("预存款支付密码错误");
                }
            }
            var skuIdsArr = submitModel.OrderShops.SelectMany(p => p.OrderSkus.Select(pp => pp.SkuId));
            if (submitModel.ProductType == 1 && skuIdsArr.Count() > 0)
            {
                var skuInfo = ProductManagerApplication.GetSKU(skuIdsArr.FirstOrDefault());
                if (skuInfo != null)
                {
                    var virtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(skuInfo.ProductId);
                    if (virtualProductInfo != null && virtualProductInfo.ValidityType && DateTime.Now > virtualProductInfo.EndDate.Value)
                    {
                        throw new MallException("该虚拟商品已过期，不支持下单");
                    }
                }
            }
            var counts = submitModel.OrderShops.SelectMany(p => p.OrderSkus.Select(pp => pp.Count));
            var productService = _iProductService;
            var orderService = Service;
            if (submitModel.Integral < 0)
            {
                throw new MallException("兑换积分数量不正确");
            }
            if (submitModel.Capital < 0)
            {
                throw new MallException("预存款金额不正确");
            }
            IEnumerable<long> collocationPidArr = null;
            if (!string.IsNullOrEmpty(submitModel.CollpIds))
            {
                collocationPidArr = submitModel.CollpIds.Split(',').Select(item => long.Parse(item));
            }
            if (submitModel.OrderShops == null || submitModel.OrderShops.Any(p => p.OrderSkus == null || p.OrderSkus.Any(pp => string.IsNullOrWhiteSpace(pp.SkuId) || pp.Count <= 0)))
                throw new Mall.Core.MallException("创建订单的时候，SKU为空，或者数量为0");

            float lat = 0; float lng = 0;
            if (!string.IsNullOrEmpty(submitModel.LatAndLng))
            {
                var arrLatAndLng = submitModel.LatAndLng.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (arrLatAndLng.Length == 2)
                {
                    float.TryParse(arrLatAndLng[0], out lat);
                    float.TryParse(arrLatAndLng[1], out lng);
                }
            }
            var model = new OrderCreateModel();
            model.SkuIds = skuIdsArr;
            model.Counts = counts;
            model.CurrentUser = (Entities.MemberInfo)submitModel.CurrentUser;
            model.Integral = submitModel.Integral;
            model.Capital = submitModel.Capital;
            model.IsCashOnDelivery = submitModel.IsCashOnDelivery;
            model.OrderRemarks = submitModel.OrderShops.Select(p => p.Remark).ToArray();
            model.CouponIdsStr = ConvertUsedCoupon(submitModel.CouponIds);
            //model.Invoice = (InvoiceType)submitModel.InvoiceType;
            //model.InvoiceTitle = submitModel.InvoiceTitle;
            //model.InvoiceCode = submitModel.InvoiceCode;
            //model.InvoiceContext = submitModel.InvoiceContext;
            model.CollPids = collocationPidArr;
            model.ReceiveLatitude = lat;
            model.ReceiveLongitude = lng;
            model.ReceiveAddressId = submitModel.RecieveAddressId;
            model.PlatformType = (PlatformType)submitModel.PlatformType;
            model.OrderShops = submitModel.OrderShops;
            if (!string.IsNullOrWhiteSpace(submitModel.CartItemIds))
                model.CartItemIds = submitModel.CartItemIds.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(item => long.Parse(item)).ToArray();
            model.GroupId = submitModel.GroupId;
            model.ActiveId = submitModel.groupActionId;
            model.IsShopbranchOrder = submitModel.IsShopbranchOrder;
            model.IsVirtual = submitModel.ProductType == 1;
            var orders = orderService.CreateOrder(model);

            var result = new OrderReturnModel();
            //不计算货到付款的单
            decimal orderTotals = orders.Where(d => d.PaymentType != OrderInfo.PaymentTypes.CashOnDelivery).Sum(item => item.OrderTotalAmount - item.CapitalAmount);
            result = new OrderReturnModel();
            result.Success = true;
            result.OrderIds = orders.Select(item => item.Id).ToArray();
            result.OrderTotal = Math.Round(orderTotals, 2);//原数据库内是保留两位小数存储的，此处用作判断是否全预存款支付
            return result;
        }

        /// <summary>
        /// 获提交订单页面数据
        /// </summary>
        /// <param name="cartItemIds">提交的购物车物品集合</param>
        /// <param name="regionId">客户送货区域标识</param>
        /// <param name="userid">用户标识</param>
        /// <param name="cartInfo">cookie的购物车物品集合</param>
        /// <returns>页面数据</returns>
        public static OrderSubmitModel Submit(string cartItemIds, long? regionId, long userid, string cartInfo, IEnumerable<string[]> CouponIdsStr = null)
        {
            var integralExchange = _iMemberIntegralService.GetIntegralChangeRule();
            int MoneyPerIntegral = integralExchange == null ? 0 : integralExchange.MoneyPerIntegral;
            OrderSubmitModel submitModel = new OrderSubmitModel();
            submitModel.IntegralPerMoney = integralExchange == null ? 0 : integralExchange.IntegralPerMoney;
            submitModel.Integral = MemberIntegralApplication.GetAvailableIntegral(userid);
            submitModel.Capital = MemberCapitalApplication.GetBalanceByUserId(userid);
            //设置会员信息
            submitModel.Member = MemberApplication.GetMember(userid);
            submitModel.cartItemIds = cartItemIds;
            //获取订单商品信息
            GetOrderProductsInfo(submitModel, cartItemIds, regionId, userid, cartInfo, CouponIdsStr);
            submitModel.TotalIntegral = MoneyPerIntegral == 0 ? 0 : Convert.ToInt64(Math.Floor(submitModel.totalAmount / MoneyPerIntegral));
            submitModel.MoneyPerIntegral = MoneyPerIntegral;
            //获取收货地址
            var address = GetShippingAddress(regionId, userid);
            submitModel.address = address;
            if (address != null)
            {
                bool hasRegion = PaymentConfigApplication.IsCashOnDelivery(address.RegionId);
                submitModel.IsCashOnDelivery = hasRegion;
            }
            else
            {
                submitModel.IsCashOnDelivery = false;
            }
            submitModel.IsLimitBuy = false;
            //发票信息
            SetOrderInvoiceInfo(submitModel, userid);
            //submitModel.InvoiceTitle = Service.GetInvoiceTitles(userid);
            //submitModel.InvoiceContext = Service.GetInvoiceContexts();
            return submitModel;
        }

        /// <summary>
        /// 根据订单ID获取订单信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static Order GetOrder(long orderId)
        {
            return Service.GetOrder(orderId).Map<DTO.Order>();
        }

        public static List<OrderInfo> CreateOrder(OrderCreateModel model)
        {
            return Service.CreateOrder(model);
        }
        /// <summary>
        /// 根据订单ID获取订单信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static OrderInfo GetOrderInfo(long orderId)
        {
            var order = Service.GetOrder(orderId);
            if (order != null)
            {
                //统一显示支付方式名称
                order.PaymentTypeName = PaymentApplication.GetPaymentTypeDescById(order.PaymentTypeGateway) ?? order.PaymentTypeName;
                return order;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 根据订单Id获取FullOrder
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static FullOrder GetFullOrder(long orderId)
        {
            var order = Service.GetOrder(orderId).Map<DTO.FullOrder>();
            order.OrderItems = GetOrderItemsByOrderId(order.Id);
            return order;
        }

        /// <summary>
        /// 根据提货码取订单
        /// </summary>
        /// <param name="pickCode"></param>
        /// <param name="fullOrderItems">是否填充OrderItems属性</param>
        /// <returns></returns>
        public static Order GetOrderByPickCode(string pickCode)
        {
            var order = Service.GetOrderByPickCode(pickCode);
            if (order != null)
            {
                //统一显示支付方式名称
                order.PaymentTypeName = PaymentApplication.GetPaymentTypeDescById(order.PaymentTypeGateway) ?? order.PaymentTypeName;
                return order.Map<DTO.Order>();
            }
            return null;
        }
        /// <summary>
        /// 根据提货码获取FullOrder
        /// </summary>
        /// <param name="pickCode"></param>
        /// <returns></returns>
        public static FullOrder GetFullOrderByPickCode(string pickCode)
        {
            var order = Service.GetOrderByPickCode(pickCode).Map<DTO.FullOrder>();
            order.OrderItems = GetOrderItemsByOrderId(order.Id);
            return order;
        }

        /// <summary>
        /// 获取商品已购数(过滤拼团、限时购购买数)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public static Dictionary<long, long> GetProductBuyCount(long userId, IEnumerable<long> productIds)
        {
            var fightBuyCounts = FightGroupApplication.GetMarketSaleCountForProductIdAndUserId(productIds, userId);
            var buyCounts = Service.GetProductBuyCountNotLimitBuy(userId, productIds).ToDictionary(e => e.Key, e => e.Value - (fightBuyCounts.ContainsKey(e.Key) ? fightBuyCounts[e.Key] : 0));
            return buyCounts;
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<Order> GetOrders(OrderQuery query)
        {
            var data = Service.GetOrders(query);
            var models = data.Models.Map<List<DTO.Order>>();
            foreach (var item in data.Models)
            {
                if (item.OrderStatus >= OrderInfo.OrderOperateStatus.WaitDelivery)
                {
                    Service.CalculateOrderItemRefund(item.Id);
                }
            }
            return new QueryPageModel<Order>
            {
                Models = models,
                Total = data.Total
            };
        }



        /// <summary>
        /// 根据每个店铺的购物列表获取每个店铺的满额减优惠金额
        /// </summary>
        /// <param name="cartItems"></param>
        /// <returns></returns>
        private static decimal GetShopFullDiscount(List<CartItemModel> cartItems, bool isShopBranchOrder = false)
        {
            decimal shopFullDiscount = 0;
            List<CartItemModel> fulldiscountP = new List<CartItemModel>();
            foreach (var p in cartItems)
            {
                var canJoin = true;
                if (!isShopBranchOrder)
                {
                    //限时购不参与满额减（bug需求34735）
                    var ltmbuy = ServiceApplication.Create<ILimitTimeBuyService>().GetLimitTimeMarketItemByProductId(p.id);
                    if (ltmbuy != null)
                    {
                        canJoin = false;
                    }
                }
                if (canJoin)
                    fulldiscountP.Add(p);
            }
            if (fulldiscountP.Count() <= 0)
                return shopFullDiscount;
            fulldiscountP = fulldiscountP.OrderBy(d => d.skuId).ToList();

            var productIds = fulldiscountP.Select(a => a.id).Distinct();
            var shopId = fulldiscountP.FirstOrDefault().shopId;
            var actives = FullDiscountApplication.GetOngoingActiveByProductIds(productIds, shopId);

            foreach (var active in actives)
            {
                var pids = active.Products.Select(a => a.ProductId);
                List<CartItemModel> items = fulldiscountP;
                if (!active.IsAllProduct)
                {
                    items = items.Where(a => pids.Contains(a.id)).ToList();
                }
                var realTotal = items.Sum(a => a.price * a.count);  //满额减的总金额
                var rule = active.Rules.Where(a => a.Quota <= realTotal).OrderByDescending(a => a.Quota).FirstOrDefault();
                decimal fullDiscount = 0;
                if (rule != null)//找不到就是不满足金额
                {
                    fullDiscount = rule.Discount;
                    decimal itemFullDiscount = 0;
                    for (var i = 0; i < items.Count(); i++)
                    {
                        var item = items[i];
                        if (i < items.Count() - 1)
                        {
                            item.fullDiscount = Math.Round(fullDiscount * (item.price * item.count) / realTotal, 2);
                            itemFullDiscount += item.fullDiscount;
                        }
                        else
                        {
                            item.fullDiscount = fullDiscount - itemFullDiscount;
                        }
                    }
                    shopFullDiscount += fullDiscount; //店铺总优惠金额
                }
            }
            return shopFullDiscount;
        }


        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<FullOrder> GetFullOrders(OrderQuery query)
        {
            var data = Service.GetOrders(query);
            if (data.Models.Count <= 0)
                return new QueryPageModel<FullOrder>()
                {
                    Models = new List<FullOrder>(),
                    Total = data.Total
                };
            var models = data.Models.Map<List<DTO.FullOrder>>();
            var orderids = models.Select(p => p.Id).ToList();
            var orderItems = GetOrderItemsByOrderId(models.Select(p => p.Id));
            //补充商品单位
            var products = ProductManagerApplication.GetAllStatusProductByIds(orderItems.Select(e => e.ProductId).Distinct());
            //补充门店名称
            var branchIds = models.Where(e =>  e.ShopBranchId>0).Select(e => e.ShopBranchId).Distinct().ToList();
            var branchModels = ShopBranchApplication.GetShopBranchByIds(branchIds);


            var refunds = _iRefundService.GetOrderRefundsByOrder(orderids);


            foreach (var order in models)
            {
                order.Refunds = refunds.Where(p => p.OrderId == order.Id).ToList();
                order.OrderItems = orderItems.Where(p => p.OrderId == order.Id).ToList();
                order.OrderProductQuantity = order.OrderItems.Sum(a=>a.Quantity);
                if (order.ShopBranchId>0)
                {//补充门店名称
                    var branch = branchModels.FirstOrDefault(e => e.Id == order.ShopBranchId);
                    if (branch != null)
                    {
                        order.ShopBranchName = branch.ShopBranchName;
                    }
                }
                else
                {
                    order.ShopBranchName = order.ShopName;
                }
                //订单售后
                var ordref = refunds.FirstOrDefault(d => d.OrderId == order.Id && d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund);
                if (ordref != null && order.OrderStatus < OrderInfo.OrderOperateStatus.WaitReceiving)
                {
                    order.RefundStats = ordref.RefundStatusValue;
                    order.ShowRefundStats = ordref.RefundStatus;
                    if (order.ShopBranchId > 0)
                    {
                        order.ShowRefundStats = order.ShowRefundStats.Replace("商家", "门店");
                    }
                }
                foreach (var item in order.OrderItems)
                {
                    var p = products.FirstOrDefault(e => e.Id == item.ProductId);
                    if (p != null)
                    {
                        item.Unit = p.MeasureUnit;
                        item.ThumbnailsUrl = MallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Mall.CommonModel.ImageSize.Size_100);
                    }
                    Entities.TypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetTypeByProductId(item.ProductId);
                    item.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    item.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    item.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                    if (p != null)
                    {
                        item.ColorAlias = !string.IsNullOrWhiteSpace(p.ColorAlias) ? p.ColorAlias : item.ColorAlias;
                        item.SizeAlias = !string.IsNullOrWhiteSpace(p.SizeAlias) ? p.SizeAlias : item.SizeAlias;
                        item.VersionAlias = !string.IsNullOrWhiteSpace(p.VersionAlias) ? p.VersionAlias : item.VersionAlias;
                    }

                    //订单项售后
                    var orditemref = refunds.FirstOrDefault(d => d.OrderId == order.Id && d.OrderItemId == item.Id && d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                    if (orditemref != null)
                    {
                        item.Refund = orditemref;
                        item.RefundStats = orditemref.RefundStatusValue;
                        item.ShowRefundStats = orditemref.RefundStatus;
                        if (order.ShopBranchId > 0)
                        {
                            item.ShowRefundStats = item.ShowRefundStats.Replace("商家", "门店");
                        }
                    }
                }
            }
            return new QueryPageModel<FullOrder>
            {
                Models = models,
                Total = data.Total
            };
        }


        /// <summary>
        /// 获取订单列表(忽略分页)
        /// </summary>
        /// <param name="orderQuery"></param>
        /// <param name="fullOrderItems">是否填充OrderItems属性</param>
        /// <returns></returns>
        public static List<FullOrder> GetAllFullOrders(OrderQuery orderQuery)
        {
            var data = Service.GetAllOrders(orderQuery);
            var list = data.Select(item => new FullOrder
            {
                ActualPayAmount = item.ActualPayAmount,
                Address = item.Address,
                CapitalAmount = item.CapitalAmount,
                CellPhone = item.CellPhone,
                CloseReason = item.CloseReason,
                CommisTotalAmount = item.CommisTotalAmount,
                DadaStatus = item.DadaStatus,
                DeliveryType = item.DeliveryType,
                DiscountAmount = item.DiscountAmount,
                ExpressCompanyName = item.ExpressCompanyName,
                FightGroupCanRefund = item.FightGroupCanRefund,
                FightGroupOrderJoinStatus = item.FightGroupOrderJoinStatus,
                FinishDate = item.FinishDate,
                Freight = item.Freight,
                FullDiscount = item.FullDiscount,
                GatewayOrderId = item.GatewayOrderId,
                Id = item.Id,
                IntegralDiscount = item.IntegralDiscount,
                LastModifyTime = item.LastModifyTime,
                OrderAmount = item.OrderAmount,
                OrderDate = item.OrderDate,
                OrderEnabledRefundAmount = item.OrderEnabledRefundAmount,
                OrderRemarks = item.OrderRemarks,
                OrderStatus = item.OrderStatus,
                OrderTotalAmount = item.OrderTotalAmount,
                OrderType = item.OrderType,
                PayDate = item.PayDate,
                PaymentType = item.PaymentType,
                PaymentTypeGateway = item.PaymentTypeGateway,
                PaymentTypeName = item.PaymentTypeName,
                PayRemark = item.PayRemark,
                PickupCode = item.PickupCode,
                Platform = item.Platform,
                ProductTotal = item.ProductTotal,
                ProductTotalAmount = item.ProductTotalAmount,
                ReceiveLatitude = item.ReceiveLatitude,
                ReceiveLongitude = item.ReceiveLongitude,
                RefundCommisAmount = item.RefundCommisAmount,
                RefundTotalAmount = item.RefundTotalAmount,
                RegionId = item.RegionId,
                RegionFullName = item.RegionFullName,
                SellerAddress = item.SellerAddress,
                SellerPhone = item.SellerPhone,
                SellerRemark = item.SellerRemark,
                SellerRemarkFlag = item.SellerRemarkFlag,
                ShipOrderNumber = item.ShipOrderNumber,
                ShippingDate = item.ShippingDate,
                ShipTo = item.ShipTo,
                ShopBranchId = item.ShopBranchId,
                ShopId = item.ShopId,
                ShopName = item.ShopName,
                Tax = item.Tax,
                TopRegionId = item.TopRegionId,
                TotalAmount = item.TotalAmount,
                UserId = item.UserId,
                UserName = item.UserName,
                UserRemark = item.UserRemark,
                OrderItems = new List<OrderItem>(),
            }).ToList();

            IEnumerable<long> orderIds = list.Select(p => p.Id);//补充门店名称

            var branchIds = list.Where(e =>  e.ShopBranchId>0).Select(e => e.ShopBranchId).Distinct().ToList();
            var branchModels = ShopBranchApplication.GetShopBranchByIds(branchIds);

            var orderItems = GetOrderItemsByOrderId(orderIds);
            var psoItems = Service.GetPendingSettlementOrdersByOrderId(orderIds);//待结算
            if (psoItems == null)
            {
                psoItems = new List<PendingSettlementOrderInfo>();
            }
            var finishPsoItems = Service.GetAccountDetailByOrderId(orderIds);//已结算订单
            if (finishPsoItems == null)
            {
                finishPsoItems = new List<AccountDetailInfo>();
            }

            var virItems = Service.GeVirtualOrderItemsByOrderId(orderIds);//虚拟订单
            if (virItems == null)
            {
                virItems = new List<VirtualOrderItemInfo>();
            }

            var invoiceItems = Service.GetOrderInvoicesByOrderId(orderIds);//发票订单
            if (invoiceItems == null)
            {
                invoiceItems = new List<OrderInvoiceInfo>();
            }

            list.ForEach(order =>
            {
                order.OrderItems = orderItems.Where(p => p.OrderId == order.Id).ToList();//子订单
                order.OrderItems.ForEach(item =>
                {
                    item.VirtualOrderItem = virItems.Where(p => p.OrderItemId == item.Id).ToList();//订单里虚拟商品信息
                });

                order.OrderProductQuantity = order.OrderItems.Sum(p => p.Quantity);
                order.OrderReturnQuantity = order.OrderItems.Sum(p => p.ReturnQuantity);
                order.OrderInvoice = invoiceItems.Where(p => p.OrderId == order.Id).FirstOrDefault();//发票订单

                #region 平台佣金和分销员佣金
                var noSettlement = psoItems.Where(p => p.OrderId == order.Id).FirstOrDefault();//待结算订单
                if (noSettlement == null)
                {
                    //已结算的订单它会删除待结算订单表记录，则待结算没数据是读取已结算订单数据
                    var yesSettlement = finishPsoItems.Where(p => p.OrderId == order.Id).FirstOrDefault();//已结算订单
                    if (yesSettlement != null)
                    {
                        order.PlatCommission = yesSettlement.CommissionAmount;//平台佣金
                        order.DistributorCommission = yesSettlement.BrokerageAmount;//分销员佣金
                    }
                }
                else
                {
                    order.PlatCommission = noSettlement.PlatCommission;//平台佣金
                    order.DistributorCommission = noSettlement.DistributorCommission;//分销员佣金
                }
                #endregion

                #region //补充门店名称
                if ( order.ShopBranchId>0)
                {
                    var branch = branchModels.FirstOrDefault(e => e.Id == order.ShopBranchId);
                    if (branch != null)
                    {
                        order.ShopBranchName = branch.ShopBranchName;
                    }
                }
                else
                {
                    order.ShopBranchName = order.ShopName;
                }
                #endregion
            });
            return list;
        }

        /// <summary>
        /// 分页查询平台会员购买记录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<Order> GetUserBuyRecord(long userId, OrderQuery query)
        {
            query.UserId = userId;
            query.IsBuyRecord = true;
            var order = Service.GetOrders<OrderInfo>(query);
            var models = order.Models.ToList().Map<List<Order>>();

            return new QueryPageModel<Order>()
            {
                Total = order.Total,
                Models = models
            };
        }

        /// <summary>
        /// 分页查询平台会员购买记录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<FullOrder> GetFullOrdersUserBuyRecord(long userId, OrderQuery query)
        {
            query.UserId = userId;
            query.IsBuyRecord = true;
            var order = Service.GetOrders(query);
            var models = order.Models.Map<List<FullOrder>>();

            var orderItems = GetOrderItemsByOrderId(models.Select(p => p.Id));
            foreach (var item in models)
            {
                item.OrderItems = orderItems.Where(p => p.OrderId == item.Id).ToList();
            }

            return new QueryPageModel<FullOrder>()
            {
                Total = order.Total,
                Models = models
            };
        }

        public static QueryPageModel<Order> GetOrders<TOrderBy>(OrderQuery query, System.Linq.Expressions.Expression<Func<OrderInfo, TOrderBy>> orderBy = null)
        {
            var data = Service.GetOrders<TOrderBy>(query, orderBy);
            var models = data.Models.ToList().Map<List<DTO.Order>>();

            return new QueryPageModel<Order>
            {
                Models = models,
                Total = data.Total
            };
        }

        public static QueryPageModel<OrderInfo> GetOrderInfos<TOrderBy>(OrderQuery query, System.Linq.Expressions.Expression<Func<OrderInfo, TOrderBy>> orderBy = null)
        {
            return Service.GetOrders<TOrderBy>(query, orderBy);
        }

        public static QueryPageModel<FullOrder> GetFullOrders<TOrderBy>(OrderQuery query, System.Linq.Expressions.Expression<Func<OrderInfo, TOrderBy>> orderBy)
        {
            var data = Service.GetOrders<TOrderBy>(query, orderBy);
            var models = data.Models.ToList().Map<List<DTO.FullOrder>>();

            var orderItems = GetOrderItemsByOrderId(models.Select(p => p.Id));
            foreach (var item in models)
            {
                item.OrderItems = orderItems.Where(p => p.OrderId == item.Id).ToList();
            }

            return new QueryPageModel<FullOrder>
            {
                Models = models,
                Total = data.Total
            };
        }

        /// <summary>
        /// 根据订单id获取订单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<Order> GetOrders(IEnumerable<long> ids)
        {
            var orderInfoList = Service.GetOrders(ids);
            return orderInfoList.Map<List<Order>>();
        }

        /// <summary>
        /// 根据订单id获取订单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<FullOrder> GetFullOrders(IEnumerable<long> ids)
        {
            var list = Service.GetOrders(ids).Map<List<FullOrder>>();

            var orderItems = GetOrderItemsByOrderId(list.Select(p => p.Id));
            foreach (var item in list)
            {
                item.OrderItems = orderItems.Where(p => p.OrderId == item.Id).ToList();
            }

            return list;
        }

        /// <summary>
        /// 获得立即购买数据
        /// </summary>
        /// <param name="userid">用户ID</param>
        /// <param name="skuIds">库存Id集合</param>
        /// <param name="counts">每个库存购买数量</param>
        /// <param name="regionId">客户收货地区的标识</param>
        /// <param name="collpids">组合购Id集合</param>
        /// <returns>返回订单提交页面数据</returns>
        public static OrderSubmitModel SubmitByProductId(long userid, string skuIds, string counts, long? regionId, string collpids = null, IEnumerable<string[]> CouponIdsStr = null, sbyte? productType = 0)
        {
            OrderSubmitModel submitModel = new OrderSubmitModel();
            //获取订单商品信息
            SetOrderProductsInfo(submitModel, skuIds, counts, userid, collpids, CouponIdsStr, productType);

            //获取收货地址
            var address = GetShippingAddress(regionId, userid);

            submitModel.address = address;
            if (address != null)
            {
                bool hasRegion = PaymentConfigApplication.IsCashOnDelivery(address.RegionId);
                var isEnable = PaymentConfigApplication.IsEnable();
                if (hasRegion && isEnable)
                {
                    submitModel.IsCashOnDelivery = true;
                }
                else
                {
                    submitModel.IsCashOnDelivery = false;
                }
            }
            else
            {
                submitModel.IsCashOnDelivery = false;
            }

            var integralExchange = _iMemberIntegralService.GetIntegralChangeRule();
            submitModel.IntegralPerMoney = integralExchange == null ? 0 : integralExchange.IntegralPerMoney;
            submitModel.MoneyPerIntegral = integralExchange == null ? 0 : integralExchange.MoneyPerIntegral;
            submitModel.TotalIntegral = submitModel.MoneyPerIntegral == 0 ? 0 : Convert.ToDecimal(Math.Floor(submitModel.totalAmount / submitModel.MoneyPerIntegral));
            submitModel.Integral = MemberIntegralApplication.GetAvailableIntegral(userid);
            submitModel.Capital = MemberCapitalApplication.GetBalanceByUserId(userid);
            var sku = _iProductService.GetSku(skuIds.Split(',')[0]);
            submitModel.IsLimitBuy = _iProductService.IsLimitBuy(sku.ProductId);

            submitModel.collIds = collpids;//组合购商品ID
            submitModel.skuIds = skuIds;//sku集合
            submitModel.counts = counts;//数量集合
            //发票信息
            SetOrderInvoiceInfo(submitModel, userid);
            //submitModel.InvoiceTitle = Service.GetInvoiceTitles(userid);
            //submitModel.InvoiceContext = Service.GetInvoiceContexts();
            return submitModel;
        }

        /// <summary>
        /// 拼团订单信息
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="GroupActionId"></param>
        /// <param name="GroupId"></param>
        /// <returns></returns>
        public static MobileOrderDetailConfirmModel SubmitByGroupId(long userid, string skuId, int count, long GroupActionId, long? GroupId = null)
        {

            MobileOrderDetailConfirmModel result = new MobileOrderDetailConfirmModel();
            //发票信息
            SetMobileOrderInvoiceInfo(result, userid);
            //result.InvoiceContext = Service.GetInvoiceContexts();
            //result.InvoiceTitle = Service.GetInvoiceTitles(userid, InvoiceType.OrdinaryInvoices);

            if (GroupActionId <= 0)
            {
                throw new InvalidPropertyException("无效的拼团活动");
            }
            if (GroupId > 0)
            {
                var gpobj = FightGroupApplication.GetGroup(GroupActionId, GroupId.Value);
                if (gpobj == null)
                {
                    throw new InvalidPropertyException("无效的团信息");
                }
            }

            //获取购买商品信息
            GetOrderProductsInfoOnGroup(skuId, count, userid, GroupActionId, result, GroupId);
            result.IsCashOnDelivery = false; //不支持货到付款
            result.Sku = skuId;
            result.Count = count.ToString();
            return result;
        }

        /// <summary>
        /// 判断用户是否有支付密码
        /// </summary>
        /// <param name="userid">用户标识</param>
        /// <returns>是否</returns>
        public static bool GetPayPwd(long userid)
        {
            string paypwd = MemberApplication.GetMember(userid).PayPwd;
            if (string.IsNullOrWhiteSpace(paypwd))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 根据用户ID获取用户收获地址列表
        /// </summary>
        /// <param name="userid">用户ID</param>
        /// <returns>收获地址列表</returns>
        public static List<ShipAddressInfo> GetUserShippingAddresses(long userid)
        {
            var addresses = ShippingAddressApplication.GetUserShippingAddressByUserId(userid).ToArray();
            List<ShipAddressInfo> result = new List<ShipAddressInfo>();
            foreach (var item in addresses)
            {
                ShipAddressInfo addr = new ShipAddressInfo();
                addr.id = item.Id;
                addr.fullRegionName = item.RegionFullName;
                addr.address = item.Address;
                addr.addressDetail = item.AddressDetail;
                addr.phone = item.Phone;
                addr.shipTo = item.ShipTo;
                addr.fullRegionIdPath = item.RegionIdPath;
                addr.regionId = item.RegionId;
                addr.latitude = item.Latitude;
                addr.longitude = item.Longitude;
                addr.NeedUpdate = item.NeedUpdate;
                result.Add(addr);
            }
            return result;
        }

        /// <summary>
        /// 确认订单(零元订单或积分支付订单)
        /// </summary>
        /// <param name="userid">用户ID</param>
        /// <param name="orderIds">订单ID集合</param>
        public static void ConfirmOrder(long userid, string orderIds)
        {
            var orderIdArr = orderIds.Split(',').Select(item => long.Parse(item));
            Service.ConfirmZeroOrder(orderIdArr, userid);
        }

        /// <summary>
        /// 保存发票抬头
        /// </summary>
        /// <param name="userid">用户ID</param>
        /// <param name="name">抬头名称</param>
        /// <returns>返回发票抬头ID</returns>
        public static long SaveInvoiceTitle(long userid, string name, string code, long id = 0)
        {
            InvoiceTitleInfo info = new InvoiceTitleInfo
            {
                Name = name,
                Code = code,
                UserId = userid,
                IsDefault = 0,
                InvoiceType = InvoiceType.OrdinaryInvoices
            };
            long result = -1;
            if (string.IsNullOrWhiteSpace(info.Name) || string.IsNullOrWhiteSpace(info.Code)) return result;
            if (id > 0)
            {
                info.Id = id;
                result = Service.EditInvoiceTitle(info);
            }
            else
            {
                result = Service.SaveInvoiceTitle(info);
            }
            return result;
        }

        /// <summary>
        /// 保存发票信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static void SaveInvoiceTitleNew(InvoiceTitleInfo info)
        {
            if (info.InvoiceType == InvoiceType.ElectronicInvoice)
            {
                if (string.IsNullOrEmpty(info.CellPhone))
                {
                    throw new MallException("收票人手机号不能为空");
                }
                if (!Core.Helper.ValidateHelper.IsMobile(info.CellPhone))
                    throw new MallException("收票人手机号格式不正确");

                if (string.IsNullOrEmpty(info.Email))
                    throw new MallException("收票人邮箱不能为空");

                if (!Core.Helper.ValidateHelper.IsEmail(info.Email))
                    throw new MallException("收票人邮箱格式不正确");
            }
            else if (info.InvoiceType == InvoiceType.VATInvoice)
            {
                if (string.IsNullOrEmpty(info.Name))
                {
                    throw new MallException("单位名称不能为空");
                }
                if (string.IsNullOrEmpty(info.Code))
                {
                    throw new MallException("纳税人识别号不能为空");
                }
                if (string.IsNullOrEmpty(info.RegisterAddress))
                {
                    throw new MallException("注册地址不能为空");
                }
                if (string.IsNullOrEmpty(info.RegisterPhone))
                {
                    throw new MallException("收票人手机号不能为空");
                }
                if (string.IsNullOrEmpty(info.CellPhone))
                {
                    throw new MallException("注册电话不能为空");
                }
                if (string.IsNullOrEmpty(info.BankName))
                {
                    throw new MallException("开户银行不能为空");
                }
                if (string.IsNullOrEmpty(info.BankNo))
                {
                    throw new MallException("银行账户不能为空");
                }
                if (string.IsNullOrEmpty(info.RealName))
                {
                    throw new MallException("收票人姓名不能为空");
                }
                if (string.IsNullOrEmpty(info.CellPhone))
                {
                    throw new MallException("收票人手机号不能为空");
                }
                if (!Core.Helper.ValidateHelper.IsMobile(info.CellPhone))
                    throw new MallException("收票人手机号格式不正确");
                if (info.RegionID <= 0)
                {
                    throw new MallException("请选择收票人地区");
                }
                if (string.IsNullOrEmpty(info.Address))
                {
                    throw new MallException("收票人详细地址不能为空");
                }
            }
            info.IsDefault = 1;
            Service.SaveInvoiceTitleNew(info);
        }

        /// <summary>
        /// 删除发票抬头
        /// </summary>
        /// <param name="id">发票抬头标识</param>
        public static void DeleteInvoiceTitle(long id, long userId = 0)
        {
            Service.DeleteInvoiceTitle(id, userId);
        }

        /// <summary>
        /// 获取运费
        /// </summary>
        /// <param name="addressId"></param>
        /// <param name="counts">门店，商品id和数量的集合</param>
        /// <returns></returns>
        public static Dictionary<long, decimal> CalcFreight(int addressId, Dictionary<long, Dictionary<long, string>> counts)
        {
            var result = new Dictionary<long, decimal>();

            foreach (var shopId in counts.Keys)
            {
                List<long> excludeIds = new List<long>();//排除掉包邮的商品

                var productInfos = ProductManagerApplication.GetProductsByIds(counts[shopId].Keys);//商家下所有的商品集合
                if (productInfos != null && productInfos.Count > 0)
                {
                    var templateIds = productInfos.Select(a => a.FreightTemplateId).ToList();
                    if (templateIds != null && templateIds.Count > 0)
                    {
                        templateIds.ForEach(tid =>
                        {
                            var ids = productInfos.Where(a => a.FreightTemplateId == tid).Select(b => b.Id).ToList();//属于当前模板的商品ID集合
                            bool isFree = false;
                            var freeRegions = ServiceProvider.Instance<IFreightTemplateService>.Create.GetShippingFreeRegions(tid);
                            freeRegions.ForEach(c =>
                            {
                                c.RegionSubList = ServiceProvider.Instance<IRegionService>.Create.GetSubsNew(c.RegionId, true).Select(a => a.Id).ToList();
                            });
                            var regions = freeRegions.Where(d => d.RegionSubList.Contains(addressId)).ToList();//根据模板设置的包邮地区过滤出当前配送地址所在地址
                            if (regions != null && regions.Count > 0)
                            {
                                var groupIds = regions.Select(e => e.GroupId).ToList();
                                var freeGroups = ServiceProvider.Instance<IFreightTemplateService>.Create.GetShippingFreeGroupInfos(tid, groupIds);

                                //只要有一个符合包邮条件，则退出
                                long count = counts[shopId].Where(p => ids.Contains(p.Key)).Sum(a => int.Parse(a.Value.Split('$')[0]));
                                decimal amount = counts[shopId].Where(p => ids.Contains(p.Key)).Sum(a => decimal.Parse(a.Value.Split('$')[1]));
                                freeGroups.ForEach(f =>
                                {
                                    if (f.ConditionType == 1)//购买件数
                                    {
                                        if (count >= int.Parse(f.ConditionNumber))
                                        {
                                            isFree = true;
                                            return;
                                        }
                                    }
                                    else if (f.ConditionType == 2)//金额
                                    {
                                        if (amount >= decimal.Parse(f.ConditionNumber))
                                        {
                                            isFree = true;
                                            return;
                                        }
                                    }
                                    else if (f.ConditionType == 3)//件数+金额
                                    {
                                        var condition1 = int.Parse(f.ConditionNumber.Split('$')[0]);
                                        var condition2 = decimal.Parse(f.ConditionNumber.Split('$')[1]);
                                        if (count >= condition1 && amount >= condition2)
                                        {
                                            isFree = true;
                                            return;
                                        }
                                    }
                                });
                            }
                            if (isFree)
                            {
                                excludeIds.AddRange(ids);
                            }

                        });
                    }
                }
                //要排除掉指定地区包邮的商品ID
                IEnumerable<long> pIds = counts[shopId].Where(a => !excludeIds.Contains(a.Key)).Select(b => b.Key);
                IEnumerable<int> pCounts = counts[shopId].Where(a => !excludeIds.Contains(a.Key)).Select(b => int.Parse(b.Value.Split('$')[0]));
                decimal freight = 0;
                if (pIds != null && pIds.Count() > 0 && pCounts != null && pCounts.Count() > 0)
                {
                    freight = _iProductService.GetFreight(pIds, pCounts, addressId);
                }
                result.Add(shopId, freight);
            }

            return result;
        }

        /// <summary>
        /// 预付款支付
        /// </summary>
        /// <param name="userid">用户ID</param>
        /// <param name="orderIds">订单ID集合</param>
        /// <param name="pwd">密码</param>
        /// <param name="hostUrl">网站地址</param>
        /// <returns>支付是否成功</returns>
        public static bool PayByCapital(long userid, string orderIds, string pwd, string hostUrl)
        {
            if (string.IsNullOrWhiteSpace(orderIds))
            {
                throw new MallException("错误的订单编号");
            }
            var success = MemberApplication.VerificationPayPwd(userid, pwd);
            if (!success)
            {
                throw new MallException("支付密码不对");
            }
            IEnumerable<long> ids = orderIds.Split(',').Select(e => long.Parse(e));
            //获取待支付的所有订单
            var orders = Service.GetOrders(ids).Where(item => item.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay && item.UserId == userid).ToList();

            if (orders == null || orders.Count() == 0) //订单状态不正确
            {
                throw new MallException("错误的订单编号");
            }
            /* 积分支付的订单金额，可能为0
            decimal total = orders.Sum(a => a.OrderTotalAmount);
            if (total == 0)
            {
                throw new MallException("错误的订单总价");
            }*/

            foreach (var item in orders)
            {
                if (item.OrderType == OrderInfo.OrderTypes.FightGroup)
                {
                    if (!FightGroupApplication.OrderCanPay(item.Id))
                    {
                        throw new MallException("有拼团订单为不可付款状态");
                    }
                }
            }

            #region 支付流水获取
            var orderPayModel = orders.Select(item => new OrderPayInfo
            {
                PayId = 0,
                OrderId = item.Id
            });
            //保存支付订单
            long payid = Service.SaveOrderPayInfo(orderPayModel, PlatformType.PC);
            #endregion

            Service.PayCapital(ids, payId: payid);

            //限时购
            IncreaseSaleCount(ids.ToList());
            //红包
            GenerateBonus(ids, hostUrl);
            return true;
        }
        public static bool PayByCapitalIsOk(long userid, string orderIds)
        {
            IEnumerable<long> ids = orderIds.Split(',').Select(e => long.Parse(e));
            return Service.PayByCapitalIsOk(userid, ids);
        }
        /// <summary>
        /// 获取支付页面数据
        /// </summary>
        /// <param name="userid">用户ID</param>
        /// <param name="orderIds">订单ID集合</param>
        /// <param name="webRoot">站点地址</param>
        /// <returns>数据</returns>
        public static PaymentViewModel GetPay(long userid, string orderIds, string webRoot)
        {
            PaymentViewModel result = new PaymentViewModel();
            result.IsSuccess = true;
            if (string.IsNullOrEmpty(orderIds))
            {
                result.IsSuccess = false;
                result.Msg = "订单号错误，不能进行支付。";
                return result;
            }
            var orderIdArr = orderIds.Split(',').Select(item => long.Parse(item));
            var orders = Service.GetOrders(orderIdArr).Where(p => p.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay && p.UserId == userid).ToList();
            if (orders.Count <= 0)//订单已经支付，则跳转至订单页面
            {

                var errorOrder = Service.GetOrders(orderIdArr).Where(p => p.OrderStatus == OrderInfo.OrderOperateStatus.Close && p.UserId == userid).Count();
                result.IsSuccess = false;
                if (errorOrder > 0)
                    result.Msg = "订单已关闭，不能进行支付。";
                else
                    result.Msg = "没有钱要付";
                return result;
            }
            else
            {

                //获取待支付的所有订单
                var orderser = Service;

                foreach (var item in orders)
                {
                    if (item.OrderType == OrderInfo.OrderTypes.FightGroup)
                    {
                        if (!FightGroupApplication.OrderCanPay(item.Id))
                        {
                            throw new MallException("有拼团订单为不可付款状态");
                        }
                    }
                }

                #region 数据补偿
                //EDIT DZY [150703]
                //是否有已删商品
                bool isHaveNoSaleProOrd = false;   //是否有非销售中的商品
                List<OrderInfo> delOrders = new List<OrderInfo>();
                foreach (var order in orders)
                {
                    if (order.OrderStatus == OrderInfo.OrderOperateStatus.Close)
                    {
                        delOrders.Add(order);
                        isHaveNoSaleProOrd = true;
                    }
                }
                if (isHaveNoSaleProOrd)
                {
                    foreach (var _item in delOrders)
                    {
                        orders.Remove(_item);  //执行清理
                    }
                    throw new MallException("有订单商品处于非销售状态，请手动处理。");
                }
                result.HaveNoSalePro = isHaveNoSaleProOrd;
                #endregion

                if (orders == null || orders.Count == 0) //订单状态不正确
                {
                    result.IsSuccess = false;
                    result.Msg = "系统错误，您可以到 “我的订单” 查看付款操作是否成功。";
                }

                result.Orders = orders;

                decimal total = orders.Sum(a => a.OrderTotalAmount - a.CapitalAmount);

                result.TotalAmount = total;

                //获取所有订单中的商品名称
                //var productInfos = GetProductNameDescriptionFromOrders(orders);

                //获取同步返回地址
                string returnUrl = webRoot + "/Pay/Return/{0}";

                //获取异步通知地址
                string payNotify = webRoot + "/Pay/Notify/{0}";

                var payments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(PlatformType.PC));

                const string RELATEIVE_PATH = "/Plugins/Payment/";

                var models = payments.Select(item =>
                {
                    string requestUrl = string.Empty;

                    #region 适应改价(注释)
                    //TODO:DZY[160428] 适应改价需求，支付过程分离
                    //try
                    //{
                    //    requestUrl = item.Biz.GetRequestUrl(string.Format(returnUrl, EncodePaymentId(item.PluginInfo.PluginId)), string.Format(payNotify, EncodePaymentId(item.PluginInfo.PluginId)), ids, total, productInfos);
                    //}
                    //catch (Exception ex)
                    //{
                    //    Core.Log.Error("支付页面加载支付插件出错", ex);
                    //}
                    #endregion

                    return new PaymentModel()
                    {
                        Logo = RELATEIVE_PATH + item.PluginInfo.ClassFullName.Split(',')[1] + "/" + item.Biz.Logo,
                        RequestUrl = requestUrl,
                        UrlType = item.Biz.RequestUrlType,
                        Id = item.PluginInfo.PluginId
                    };
                });
                result.Models = models.OrderByDescending(d => d.Id);
                //models = models.Where( item => !string.IsNullOrEmpty( item.RequestUrl ) );//只选择正常加载的插件
                //TODO:【2015-08-31】支付页面增加预付款
                //var capital = MemberCapitalApplication.GetCapitalInfo(userid);
                //if (capital == null)
                //{
                //    result.Capital = 0;
                //}
                //else
                //{
                //    result.Capital = capital.Balance != null ? capital.Balance.Value : 0;
                //}
                return result;
            }
        }

        /// <summary>
        /// 获取支付相关信息
        /// </summary>
        /// <param name="userid">用户id</param>
        /// <param name="orderIds">订单id</param>
        /// <param name="webRoot">网站根目录</param>
        /// <returns>支付相信息</returns>
        public static ChargePayModel ChargePay(long userid, string orderIds, string webRoot)
        {

            ChargePayModel viewmodel = new ChargePayModel();
            var model = MemberCapitalApplication.GetChargeDetail(long.Parse(orderIds));
            if (model == null || model.MemId != userid || model.ChargeStatus == Mall.Entities.ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess)//订单已经支付，则跳转至订单页面
            {
                Log.Error("调用ChargePay方法时未找到充值申请记录：" + orderIds);
                //return RedirectToAction("index", "userCenter", new { url = "/UserCapital", tar = "UserCapital" });
                return null;
            }
            else
            {

                //ViewBag.Orders = model;
                viewmodel.Orders = model;
                //string webRoot = Request.Url.Scheme + "://" + HttpContext.Request.Url.Host + (HttpContext.Request.Url.Port == 80 ? "" : (":" + HttpContext.Request.Url.Port.ToString()));

                //获取同步返回地址
                string returnUrl = webRoot + "/Pay/CapitalChargeReturn/{0}";

                //获取异步通知地址
                string payNotify = webRoot + "/Pay/CapitalChargeNotify/{0}/";

                var payments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(PlatformType.PC));

                const string RELATEIVE_PATH = "/Plugins/Payment/";

                var models = payments.Select(item =>
                {
                    string requestUrl = string.Empty;
                    try
                    {
                        requestUrl = item.Biz.GetRequestUrl(string.Format(returnUrl, EncodePaymentId(item.PluginInfo.PluginId)), string.Format(payNotify, EncodePaymentId(item.PluginInfo.PluginId)), orderIds, model.ChargeAmount, "预存款充值");
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Error("支付页面加载支付插件出错", ex);
                    }
                    return new PaymentModel()
                    {
                        Logo = RELATEIVE_PATH + item.PluginInfo.ClassFullName.Split(',')[1] + "/" + item.Biz.Logo,
                        RequestUrl = requestUrl,
                        UrlType = item.Biz.RequestUrlType,
                        Id = item.PluginInfo.PluginId
                    };
                });
                models = models.Where(item => !string.IsNullOrEmpty(item.RequestUrl));//只选择正常加载的插件
                viewmodel.OrderIds = orderIds;
                viewmodel.TotalAmount = model.ChargeAmount;
                viewmodel.Step = 1;
                viewmodel.UnpaidTimeout = SiteSettingApplication.SiteSettings.UnpaidTimeout;
                viewmodel.models = models.OrderByDescending(d => d.Id).ToList();
                //return View(viewmodel);
                return viewmodel;
            }
        }

        /// <summary>
        /// 获得限时购订单提交数据对像
        /// </summary>
        /// <returns></returns>
        public static OrderCreateModel GetLimitOrder(CommonModel.OrderPostModel model)
        {
            var skuIdsArr = model.OrderShops.SelectMany(p => p.OrderSkus.Select(pp => pp.SkuId));
            var pCountsArr = model.OrderShops.SelectMany(p => p.OrderSkus.Select(pp => pp.Count));
            var productService = _iProductService;
            var orderService = Service;
            if (model.Integral < 0)
            {
                throw new MallException("兑换积分数量不正确");
            }
            IEnumerable<long> collocationPidArr = null;
            if (!string.IsNullOrEmpty(model.CollpIds))
            {
                collocationPidArr = model.CollpIds.Split(',').Select(item => long.Parse(item));
            }

            var result = new OrderCreateModel();
            result.SkuIds = skuIdsArr;
            result.Counts = pCountsArr;
            result.CurrentUser = (Entities.MemberInfo)model.CurrentUser;
            result.Integral = model.Integral;
            result.Capital = model.Capital;
            result.IsCashOnDelivery = model.IsCashOnDelivery;
            result.OrderRemarks = model.OrderShops.Select(p => p.Remark);
            result.CouponIdsStr = ConvertUsedCoupon(model.CouponIds);
            //result.Invoice = (InvoiceType)model.InvoiceType;
            //result.InvoiceTitle = model.InvoiceTitle;
            //result.InvoiceCode = model.InvoiceCode;
            //result.InvoiceContext = model.InvoiceContext;
            result.CollPids = collocationPidArr;
            result.ReceiveAddressId = model.RecieveAddressId;
            result.IslimitBuy = true;
            result.OrderShops = model.OrderShops;
            if (result.Counts.Count() == 0)
                throw new InvalidPropertyException("待提交订单的商品数量不能这空");
            else if (result.Counts.Count(item => item <= 0) > 0)
                throw new InvalidPropertyException("待提交订单的商品数量必须都大于0");
            else if (result.SkuIds.Count() != result.Counts.Count())
                throw new InvalidPropertyException("商品数量不一致");
            else if (model.RecieveAddressId <= 0)
                throw new InvalidPropertyException("收货地址无效");
            else
                return result;
        }

        /// <summary>
        /// 获得拼团订单提交数据对像
        /// </summary>
        /// <returns></returns>
        public static OrderCreateModel GetGroupOrder(
            long userid,
            string skuIds,
            string counts,
            long recieveAddressId,
            int invoiceType,
            string invoiceTitle,
            string invoiceCode,
            string invoiceContext,
            long activeId,
            PlatformType platformType,
            long groupId = 0,
            bool isCashOnDelivery = false,
            string orderRemarks = "",
            decimal capitalAmount = 0
            )
        {
            var skuIdsArr = skuIds.Split(',');
            var pCountsArr = counts.TrimEnd(',').Split(',').Select(t => int.Parse(t));
            var productService = _iProductService;
            var orderService = Service;
            IEnumerable<long> collocationPidArr = null;
            if (string.IsNullOrWhiteSpace(skuIds) || string.IsNullOrWhiteSpace(counts))
                throw new Mall.Core.MallException("创建订单的时候，SKU为空，或者数量为0");
            if (userid <= 0)
                throw new InvalidPropertyException("会员Id无效");
            OrderCreateModel model = new OrderCreateModel();
            model.SkuIds = skuIdsArr;
            model.Counts = pCountsArr;
            model.CurrentUser = MemberApplication.GetMember(userid);
            model.Integral = 0;
            model.IsCashOnDelivery = isCashOnDelivery;
            model.OrderRemarks = orderRemarks.Split(',');
            model.Invoice = (InvoiceType)invoiceType;
            model.InvoiceTitle = invoiceTitle;
            model.InvoiceCode = invoiceCode;
            model.InvoiceContext = invoiceContext;
            model.CollPids = collocationPidArr;
            model.ReceiveAddressId = recieveAddressId;
            model.IslimitBuy = false;
            model.ActiveId = activeId;
            model.GroupId = groupId;
            model.PlatformType = platformType;
            model.Capital = capitalAmount;
            if (model.Counts.Count() == 0)
            {
                throw new InvalidPropertyException("待提交订单的商品数量不能为空");
            }

            if (model.Counts.Count(item => item <= 0) > 0)
            {
                throw new InvalidPropertyException("待提交订单的商品数量必须都大于0");
            }

            if (model.SkuIds.Count() != model.Counts.Count())
            {
                throw new InvalidPropertyException("商品数量不一致");
            }

            if (recieveAddressId <= 0)
            {
                throw new InvalidPropertyException("收货地址无效");
            }

            if (activeId <= 0)
            {
                throw new InvalidPropertyException("无效的拼团ID");
            }

            if (groupId > 0)
            {
                var gpobj = FightGroupApplication.GetGroup(activeId, groupId);
                if (gpobj == null)
                {
                    throw new InvalidPropertyException("无效的团信息");
                }
                if (gpobj.BuildStatus != CommonModel.FightGroupBuildStatus.Ongoing)
                {
                    throw new InvalidPropertyException("拼团当前状态无法参团");
                }
            }
            return model;

        }



        /// <summary>
        /// 限时购缓存提交订单
        /// </summary>
        public static string LimitRedisSubmit(OrderCreateModel model)
        {
            //string id = "";
            //SubmitOrderResult r = LimitOrderHelper.SubmitOrder(model, out id);
            //if (r == SubmitOrderResult.SoldOut)
            //    throw new MallException("已售空");
            //else if (r == SubmitOrderResult.NoSkuId)
            //    throw new Mall.Core.InvalidPropertyException("创建订单的时候，SKU为空，或者数量为0");
            //else if (r == SubmitOrderResult.NoData)
            //    throw new Mall.Core.InvalidPropertyException("参数错误");
            //else if (string.IsNullOrEmpty(id))
            //    throw new Mall.Core.InvalidPropertyException("参数错误");
            //else
            //    return id;
            throw new NotImplementedException();
        }

        /// <summary>
        /// 数据库直接提交订单
        /// </summary>
        public static OrderReturnModel OrderSubmit(OrderCreateModel model, string payPwd = "")
        {
            if (model.Capital > 0 && !string.IsNullOrEmpty(payPwd))
            {
                var flag = MemberApplication.VerificationPayPwd(model.CurrentUser.Id, payPwd);
                if (!flag)
                {
                    throw new MallException("预存款支付密码错误");
                }
            }
            var orders = Service.CreateOrder(model);
            decimal orderTotals = orders.Sum(item => item.OrderTotalAmount - item.CapitalAmount);
            var result = new OrderReturnModel();
            result.Success = true;
            result.OrderIds = orders.Select(item => item.Id).ToArray();
            result.OrderTotal = Math.Round(orderTotals, 2);//原数据库内是保留两位小数存储的，此处用作判断是否全预存款支付
            return result;
        }

        #endregion

        #region mobile公共方法
        /// <summary>
        /// 获得立即购买提交页面数据
        /// </summary>
        /// <param name="skuIds">库存ID集合</param>
        /// <param name="counts">库存ID对应的数量</param>
        /// <returns>数据</returns>
        public static MobileOrderDetailConfirmModel GetMobileSubmit(long userid, string skuIds, string counts, long shippingAddressId = 0, IEnumerable<string[]> CouponIdsStr = null, long shopBranchId = 0)
        {
            if (string.IsNullOrEmpty(skuIds))
                throw new InvalidPropertyException("待提交订单的商品ID不能为空");
            if (string.IsNullOrEmpty(counts))
                throw new InvalidPropertyException("待提交订单的商品数量不能为空");
            MobileOrderDetailConfirmModel result = new MobileOrderDetailConfirmModel();

            //获取发票信息
            SetMobileOrderInvoiceInfo(result, userid);
            //result.InvoiceContext = Service.GetInvoiceContexts();
            //result.InvoiceTitle = Service.GetInvoiceTitles(userid, InvoiceType.OrdinaryInvoices);

            //获取收货地址
            GetShippingAddress(userid, result);

            //获取购买商品信息
            var sku = skuIds.Split(',').Select(item => item);
            var count = counts.Split(',').Select(item => int.Parse(item));
            GetOrderProductsInfo(userid, sku, count, result, shippingAddressId, CouponIdsStr, shopBranchId);
            result.Sku = skuIds;
            result.Count = counts;
            return result;
        }

        /// <summary>
        /// 进入购物车提交页面
        /// </summary>
        /// <param name="cartItemIds">购物车旬</param>
        /// <returns></returns>
        public static MobileOrderDetailConfirmModel GetMobileSubmiteByCart(long userid, string cartItemIds, long shippingAddressId = 0, IEnumerable<string[]> CouponIdsStr = null)
        {
            MobileOrderDetailConfirmModel result = new MobileOrderDetailConfirmModel();
            //获取发票信息
            SetMobileOrderInvoiceInfo(result, userid);
            //result.InvoiceContext = Service.GetInvoiceContexts();
            //result.InvoiceTitle = Service.GetInvoiceTitles(userid, InvoiceType.OrdinaryInvoices);
            GetOrderProductsInfo(userid, cartItemIds, result, shippingAddressId, CouponIdsStr);
            GetShippingAddress(userid, result, shippingAddressId);
            return result;
        }

        /// <summary>
        /// 组合购提交页面
        /// </summary>
        /// <param name="cartItemIds">购物车旬</param>
        /// <returns></returns>
        public static MobileOrderDetailConfirmModel GetMobileCollocationBuy(long userid, string skuIds, string counts, long? regionId, string collpids = null, long shippingAddressId = 0, IEnumerable<string[]> CouponIdsStr = null)
        {
            if (string.IsNullOrEmpty(collpids))
                throw new InvalidPropertyException("组合构ID不能为空");
            MobileOrderDetailConfirmModel result = new MobileOrderDetailConfirmModel();
            result.InvoiceContext = Service.GetInvoiceContexts();
            result.InvoiceTitle = Service.GetInvoiceTitles(userid, InvoiceType.OrdinaryInvoices);
            GetShippingAddress(userid, result);
            string[] skus = skuIds.Split(',');
            string[] countarr = counts.Split(',');
            int[] cs = new int[countarr.Length];
            for (int i = 0; i < countarr.Length; i++)
            {
                cs[i] = int.Parse(countarr.ElementAt(i));
            }

            string[] colarr = collpids.Split(',');
            if (colarr.Count() == 0)
                throw new InvalidPropertyException("组合构ID不能为空");
            int[] ps = new int[colarr.Length];
            for (int i = 0; i < colarr.Length; i++)
            {
                ps[i] = int.Parse(colarr[i]);
            }


            GetOrderProductInfoColl(userid, skus, cs, ps, result, shippingAddressId, CouponIdsStr);
            return result;
        }

        /// <summary>
        /// 使用积分支付的订单取消
        /// </summary>
        /// <param name="orderIds">订单id</param>
        /// <param name="userid">用户id</param>
        public static void CancelOrder(string orderIds, long userid)
        {
            var orderIdArr = orderIds.Split(',').Select(item => long.Parse(item));
            Service.CancelOrders(orderIdArr, userid);

        }

        /// <summary>
        /// 是否全部抵扣
        /// </summary>
        /// <param name="integral">积分</param>
        /// <param name="total">总共需要积分</param>
        /// <param name="userid">用户标识</param>
        /// <returns>抵扣是否成功</returns>
        public static bool IsAllDeductible(int integral, decimal total, long userid)
        {
            if (integral == 0) //没使用积分时的0元订单
                return false;
            var result = Service.GetIntegralDiscountAmount(integral, userid);
            if (result < total)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 添加微店购买数量
        /// </summary>
        /// <param name="orderIds">订单ID</param>
        public static void AddVshopBuyNumber(IEnumerable<long> orderIds)
        {
            var shopIds = Service.GetOrders(orderIds).Select(item => item.ShopId).ToList();//从订单信息获取店铺id
            List<long> vshopIds = new List<long>();
            foreach (var item in shopIds)
            {
                var vshop = VshopApplication.GetVShopByShopId(item);
                if (vshop != null)
                {
                    vshopIds.Add(vshop.Id);
                }
            }
            foreach (var vshopId in vshopIds)
            {
                VshopApplication.AddBuyNumber(vshopId);
            }
        }

        /// <summary>
        /// 根据用户获收获地址列表
        /// </summary>
        /// <param name="userid">用户id</param>
        /// <returns>收获地址列表</returns>
        public static List<Entities.ShippingAddressInfo> GetUserAddresses(long userid, long shopBranchId = 0)
        {
            var addresss = ShippingAddressApplication.GetUserShippingAddressByUserId(userid).ToList();
            if (shopBranchId > 0)
            {
                var shopBranchInfo = _iShopBranchService.GetShopBranchById(shopBranchId);
                if (shopBranchInfo == null)
                    return addresss;
                foreach (var item in addresss)
                {
                    if (item.NeedUpdate) continue;
                    string form = string.Format("{0},{1}", item.Latitude, item.Longitude);//收货地址的经纬度
                    if (form.Length <= 1)
                        continue;//地址不含经纬度的不可配送
                    double Distances = _iShopBranchService.GetLatLngDistancesFromAPI(form, string.Format("{0},{1}", shopBranchInfo.Latitude, shopBranchInfo.Longitude));
                    if (Distances > shopBranchInfo.ServeRadius)
                        continue;//距离超过配送距离的不可配送,距离计算失败不可配送
                    item.CanDelive = true;
                }
            }
            return addresss;
        }

        /// <summary>
        /// 设置用户默认收货地址
        /// </summary>
        /// <param name="addrId">地址Id</param>
        /// <param name="userid">用户Id</param>
        public static void SetDefaultUserShippingAddress(long addrId, long userid)
        {
            ShippingAddressApplication.SetDefaultShippingAddress(addrId, userid);
        }

        /// <summary>
        /// 获取指定收获地址的信息
        /// </summary>
        /// <param name="addressId">收获地址Id</param>
        /// <returns>收获地址信息</returns>
        public static Entities.ShippingAddressInfo GetUserAddress(long addressId)
        {
            var ShipngInfo = new Entities.ShippingAddressInfo();
            if (addressId != 0)
            {
                ShipngInfo = ShippingAddressApplication.GetUserShippingAddress(addressId);
            }
            return ShipngInfo;
        }

        /// <summary>
        /// 删除指定的收获地址信息
        /// </summary>
        /// <param name="addressId">收获地址Id</param>
        public static void DeleteShippingAddress(long addressId, long userid)
        {
            ShippingAddressApplication.DeleteShippingAddress(addressId, userid);
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orderId">订单Id</param>
        /// <param name="userid">用户Id</param>
        /// <param name="username">用户名</param>
        /// <returns>是否成功</returns>
        public static bool CloseOrder(long orderId, long userid, string username)
        {
            var order = Service.GetOrder(orderId, userid);
            if (order != null)
            {
                Service.MemberCloseOrder(orderId, username);
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 确认订单收货
        /// </summary>
        public static int ConfirmOrder(long orderId, long userId, string username)
        {
            var order = Service.GetOrder(orderId, userId);
            if (order.OrderStatus == OrderInfo.OrderOperateStatus.Finish)
            {
                return 1;
                //throw new MallException("该订单已经确认过!");
            }
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitReceiving && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
            {
                return 2;
                //throw new MallException("订单状态发生改变，请重新刷页面操作!");
            }
            Service.MembeConfirmOrder(orderId, username);
            if (order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery)
            {//货到付款的订单，在会员确认收货时
                MemberApplication.UpdateNetAmount(order.UserId, order.OrderTotalAmount);
                MemberApplication.IncreaseMemberOrderNumber(order.UserId);
            }
            return 0;
        }
        /// <summary>
        /// 门店核销订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="managerName"></param>
        public static void ShopBranchConfirmOrder(long orderId, long shopBranchId, string managerName)
        {
            Service.ShopBranchConfirmOrder(orderId, shopBranchId, managerName);
        }
        /// <summary>
        /// 获取订单详细信息
        /// </summary>
        /// <param name="id">订单Id</param>
        /// <param name="userid">用户Id</param>
        /// <param name="type">平台类型</param>
        /// <param name="host">网站host地址</param>
        /// <returns>订单详细信息</returns>
        public static OrderDetailView Detail(long id, long userid, PlatformType type, string host)
        {
            OrderInfo order = Service.GetOrder(id, userid);
            var shopinfo = ShopApplication.GetShopInfo(order.ShopId);
            var vshop = VshopApplication.GetVShopByShopId(shopinfo.Id)?.Id ?? 0;
            bool IsRefundTimeOut = false;
            var _ordrefobj = RefundApplication.GetOrderRefundByOrderId(id) ?? new OrderRefundInfo { Id = 0 };
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
            {
                _ordrefobj = new OrderRefundInfo { Id = 0 };
            }
            int? ordrefstate = (_ordrefobj == null ? null : (int?)_ordrefobj.SellerAuditStatus);
            ordrefstate = (ordrefstate > 4 ? (int?)_ordrefobj.ManagerConfirmStatus : ordrefstate);
            var orderItems = Service.GetOrderItemsByOrderId(id);
            var refunds = RefundApplication.GetOrderRefundsByOrder(id);
            var products = _iProductService.GetProducts(orderItems.Select(p => p.ProductId).ToList());
            //获取订单商品项数据
            var orderDetail = new OrderDetail()
            {
                ShopName = shopinfo.ShopName,
                ShopId = order.ShopId,
                VShopId = vshop,
                RefundStats = ordrefstate,
                OrderRefundId = _ordrefobj.Id,
                OrderItems = orderItems.Select(item =>
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    var refund = refunds.FirstOrDefault(p => p.OrderItemId == item.Id && p.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                    int? refundState = (refund == null ? null : (int?)refund.SellerAuditStatus);
                    refundState = (refundState > 4 ? (int?)refund.ManagerConfirmStatus : refundState);
                    return new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Count = item.Quantity,
                        Price = item.SalePrice,
                        ProductImage = product.GetImage(ImageSize.Size_100),
                        Id = item.Id,
                        Unit = product.MeasureUnit,
                        IsCanRefund = CanRefund(order, itemId: item.Id),
                        Color = item.Color,
                        Size = item.Size,
                        Version = item.Version,
                        RefundStats = refundState,
                        OrderRefundId = (refund == null ? 0 : refund.Id),
                        EnabledRefundAmount = item.EnabledRefundAmount
                    };
                })
            };
            OrderDetailView view = new OrderDetailView();
            IsRefundTimeOut = Service.IsRefundTimeOut(id);
            view.Detail = orderDetail;
            view.Bonus = null;
            if (type == Core.PlatformType.WeiXin)
            {
                var bonusmodel = ShopBonusApplication.GetGrantByUserOrder(id, userid);
                if (bonusmodel != null)
                {
                    view.Bonus = bonusmodel;
                    view.ShareHref = Core.Helper.WebHelper.GetScheme() + "://" + host + "/m-weixin/shopbonus/index/" + ShopBonusApplication.GetGrantIdByOrderId(id);
                }
            }
            view.Order = order;

            view.FightGroupCanRefund = true;
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)  //拼团状态补充
            {
                var fgord = FightGroupApplication.GetFightGroupOrderStatusByOrderId(order.Id);
                view.FightGroupJoinStatus = CommonModel.FightGroupOrderJoinStatus.JoinFailed;
                if (fgord != null)
                {
                    view.FightGroupJoinStatus = fgord.GetJoinStatus;
                    view.FightGroupCanRefund = fgord.CanRefund;
                }
            }

            view.IsRefundTimeOut = IsRefundTimeOut;
            return view;
        }

        /// <summary>
        /// 是否超过售后期
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static bool IsRefundTimeOut(DTO.Order order)
        {
            return Service.IsRefundTimeOut(order.Map<OrderInfo>());
        }

        /// <summary>
        /// 获取快递信息
        /// </summary>
        /// <param name="orderId">订单Id</param>
        /// <param name="userid">用户Id</param>
        /// <returns>快递信息 [0]:快递公司 [1]:单号</returns>
        public static string[] GetExpressInfo(long orderId)
        {
            OrderInfo order = Service.GetOrder(orderId);
            string[] result = new string[2];
            if (order != null)
            {
                result[0] = order.ExpressCompanyName;
                result[1] = order.ShipOrderNumber;
            }
            return result;
        }
        #endregion
        /// <summary>
        /// 计算会员折扣价
        /// </summary>
        /// <param name="discount"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        static void CalculateDiscountPrice(decimal discount, List<CartItemModel> cartItems)
        {
            var siteInfo = Application.SiteSettingApplication.SiteSettings;
            if (siteInfo != null)
            {
                if (!(siteInfo.IsOpenPC || siteInfo.IsOpenH5 || siteInfo.IsOpenMallSmallProg || siteInfo.IsOpenApp))//授权模块影响会员折扣功能
                {
                    return;
                }
            }
            foreach (var item in cartItems)
            {
                if (item.IsLimit) continue;//如果是限时购，则不处理折扣价
                item.price = Math.Round(decimal.Round(item.price, 2, MidpointRounding.AwayFromZero) * discount, 2, MidpointRounding.AwayFromZero);//折扣价为四舍五入保留两位小数
            }
        }

        #region 张宇枫

        /// <summary>
        /// 根据SKUID获取SKU
        /// </summary>
        /// <param name="skuid"></param>
        /// <returns></returns>
        public static SKUInfo GetSkuByID(string skuid)
        {
            return Service.GetSkuByID(skuid);
        }


        #endregion
        #region mobile私有方法
        /// <summary>
        /// 获取订单相关的产品信息
        /// </summary>
        /// <param name="userid">用户id</param>
        /// <param name="skuIds">库存id</param>
        /// <param name="counts">数量</param>
        /// <param name="confirmModel">保存数据的实体</param>
        static void GetOrderProductsInfo(long userid, IEnumerable<string> skuIds, IEnumerable<int> counts, MobileOrderDetailConfirmModel confirmModel, long shippingAddressId = 0, IEnumerable<string[]> CouponIdsStr = null, long shopBranchId = 0)
        {
            int cityId = 0;
            if (shippingAddressId > 0)
            {
                var address = ShippingAddressApplication.GetUserShippingAddress(shippingAddressId);
                if (address != null)
                {
                    cityId = address.RegionId;
                }
            }
            else
            {
                var address = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
                if (address != null)
                {
                    // cityId = RegionApplication.GetRegion(address.RegionId, Region.RegionLevel.City).Id;
                    cityId = address.RegionId;
                }
            }
            sbyte productType = 0; long productId = 0;
            var products = GenerateCartItem(skuIds, counts, shopBranchId);
            if (products != null && products.Count == 1)
            {
                productType = products.FirstOrDefault().ProductType;
                productId = products.FirstOrDefault().id;
            }
            var shopList = products.GroupBy(a => a.shopId);
            var member = MemberApplication.GetMember(userid);
            List<MobileShopCartItemModel> list = new List<MobileShopCartItemModel>();
            var baseCoupon = Service.GetOrdersCoupons(userid, CouponIdsStr);
            foreach (var shopcartItem in shopList)
            {
                IEnumerable<long> productIds = shopcartItem.Select(r => r.id);
                IEnumerable<int> productCounts = shopcartItem.Select(r => r.count);
                MobileShopCartItemModel item = new MobileShopCartItemModel();
                item.shopId = shopcartItem.Key;
                if (products != null && products.Count > 0)
                {
                    item.ShopBranchId = products[0].ShopBranchId;
                    item.ShopBranchName = products[0].ShopBranchName;
                }
                item.CartItemModels = products.Where(a => a.shopId == item.shopId).ToList();
                if (cityId > 0 && productType == 0)
                {
                    #region 指定地区包邮
                    IEnumerable<long> pIds;
                    IEnumerable<int> pCounts;
                    FreeShipping(cityId, shopcartItem, out pIds, out pCounts);
                    #endregion
                    if (pIds != null && pIds.Count() > 0 && pCounts != null && pCounts.Count() > 0)
                    {
                        item.Freight = _iProductService.GetFreight(pIds, pCounts, cityId);
                    }
                }

                var shop = ShopApplication.GetShopInfo(item.shopId);
                if (shop.IsSelf)
                {
                    //只有官方自营店商品，才有会员折扣，会员折扣优先级高于满减、优惠券
                    //计算会员折扣
                    CalculateDiscountPrice(member.MemberDiscount, item.CartItemModels);
                }
                item.IsSelf = shop.IsSelf;
                //计算满额减的金额
                item.FullDiscount = GetShopFullDiscount(item.CartItemModels,  item.ShopBranchId > 0);
                //满足优惠券(商品总金额除去满额减金额)
                if (baseCoupon != null)
                {
                    var couponAmount = item.ShopTotalWithoutFreight;
                    var coupon = baseCoupon.Where(a => a.ShopId == item.shopId).FirstOrDefault();
                    if (coupon != null && coupon.Type == 0)
                    {
                        var uc = (coupon.Coupon as Entities.CouponRecordInfo);
                        var bc = CouponApplication.Get(uc.CouponId);
                        if (bc.UseArea == 1)
                        {
                            var couponProducts = CouponApplication.GetCouponProductsByCouponId(uc.CouponId).Select(p => p.ProductId).ToList();
                            decimal coupontotal = 0;
                            foreach (var p in item.CartItemModels)
                            {
                                if (couponProducts.Contains(p.id))
                                    coupontotal += p.price * p.count - p.fullDiscount;
                            }
                            couponAmount = coupontotal;
                        }
                    }
                    item.OneCoupons = GetSelectedCoupon(item.ShopTotalWithoutFreight == couponAmount ? item.ShopTotalWithoutFreight : couponAmount, userid, item.shopId, baseCoupon);
                }
                else
                {
                    item.OneCoupons = GetDefaultCoupon(item.shopId, userid, item.ShopTotalWithoutFreight, item.CartItemModels);
                }
                item.BaseCoupons = GetBaseCoupon(item.shopId, userid, item.ShopTotalWithoutFreight, item.CartItemModels);
                decimal ordPaidPrice = CalculatePaidPrice(item);
                item.ShopName = shop.ShopName;
                //满额免
                SetFullFree(ordPaidPrice, shop.FreeFreight, item);

                item.VshopId = VshopApplication.GetVShopByShopId(shop.Id)?.Id ?? 0;
                //item.ShopBranchId=products[0].
                item.IsOpenLadder = item.CartItemModels.Any(c => c.IsOpenLadder);
                //发票
                item.IsInvoice = ShopApplication.GetShopInvoiceConfig(item.shopId)?.IsInvoice ?? false;
                item.invoiceTpyes = ShopApplication.GetInvoiceTypes(item.shopId);
                item.InvoiceDay = GetInvoiceDays(item.shopId);
                list.Add(item);
            }
            var totalUserCoupons = list.Where(a => a.OneCoupons != null).Sum(b => b.OneCoupons.BasePrice);
            confirmModel.products = list;
            //  confirmModel.totalAmount = products.Sum(item => item.price * item.count);

            confirmModel.totalAmount = products.Sum(item => item.price * item.count) - list.Sum(a => a.FullDiscount);
            confirmModel.Freight = list.Sum(a => a.Freight);
            confirmModel.orderAmount = confirmModel.totalAmount + confirmModel.Freight - totalUserCoupons;
            confirmModel.capitalAmount = MemberCapitalApplication.GetBalanceByUserId(userid);
            var memberIntegralInfo = MemberIntegralApplication.GetMemberIntegral(userid);
            var memberIntegral = memberIntegralInfo.AvailableIntegrals;
            confirmModel.memberIntegralInfo = memberIntegralInfo;
            OrderIntegralModel integral = GetAvailableIntegral(confirmModel.totalAmount, totalUserCoupons, memberIntegral);
            confirmModel.integralPerMoney = integral.IntegralPerMoney;
            confirmModel.userIntegralMaxDeductible = integral.userIntegralMaxDeductible;
            confirmModel.integralPerMoneyRate = integral.integralPerMoneyRate;
            confirmModel.userIntegrals = integral.UserIntegrals;
            confirmModel.ProductType = productType;
            confirmModel.ProductId = productId;
        }

        private static void FreeShipping(int cityId, IGrouping<long, CartItemModel> shopcartItem, out IEnumerable<long> pIds, out IEnumerable<int> pCounts)
        {
            List<long> excludeIds = new List<long>();//排除掉包邮的商品
            var templateIds = shopcartItem.Select(p => p.FreightTemplateId).Distinct().ToList();//当前商家下所有商品模板ID集合
            templateIds.ForEach(p =>
            {
                var ids = shopcartItem.Where(a => a.FreightTemplateId == p).Select(a => a.id).ToList();//属于当前模板的商品ID集合
                bool isFree = false;
                var freeRegions = ServiceProvider.Instance<IFreightTemplateService>.Create.GetShippingFreeRegions(p);
                freeRegions.ForEach(c =>
                {
                    c.RegionSubList = ServiceProvider.Instance<IRegionService>.Create.GetSubsNew(c.RegionId, true).Select(a => a.Id).ToList();
                });
                var regions = freeRegions.Where(d => d.RegionSubList.Contains(cityId)).ToList();//根据模板设置的包邮地区过滤出当前配送地址所在地址
                if (regions != null && regions.Count > 0)
                {
                    var groupIds = regions.Select(e => e.GroupId).ToList();
                    var freeGroups = ServiceProvider.Instance<IFreightTemplateService>.Create.GetShippingFreeGroupInfos(p, groupIds);

                    //只要有一个符合包邮条件，则退出
                    long count = shopcartItem.Where(a => ids.Contains(a.id)).Sum(b => b.count);//总数量
                    decimal amount = shopcartItem.Where(a => ids.Contains(a.id)).Sum(b => b.price * b.count);//总金额
                    freeGroups.ForEach(f =>
                    {
                        if (f.ConditionType == 1)//购买件数
                        {
                            if (count >= int.Parse(f.ConditionNumber))
                            {
                                isFree = true;
                                return;
                            }
                        }
                        else if (f.ConditionType == 2)//金额
                        {
                            if (amount >= decimal.Parse(f.ConditionNumber))
                            {
                                isFree = true;
                                return;
                            }
                        }
                        else if (f.ConditionType == 3)//件数+金额
                        {
                            var condition1 = int.Parse(f.ConditionNumber.Split('$')[0]);
                            var condition2 = decimal.Parse(f.ConditionNumber.Split('$')[1]);
                            if (count >= condition1 && amount >= condition2)
                            {
                                isFree = true;
                                return;
                            }
                        }
                    });
                }
                if (isFree)
                {
                    excludeIds.AddRange(ids);
                }
            });
            pIds = shopcartItem.Where(a => !excludeIds.Contains(a.id)).Select(b => b.id);
            pCounts = shopcartItem.Where(a => !excludeIds.Contains(a.id)).Select(b => b.count);
        }

        static void GetOrderProductInfoColl(long userid, IEnumerable<string> skuIds, int[] counts, int[] collpids, MobileOrderDetailConfirmModel confirmModel, long shippingAddressId = 0, IEnumerable<string[]> CouponIdsStr = null)
        {
            int cityId = 0;
            if (shippingAddressId > 0)
            {
                var address = ShippingAddressApplication.GetUserShippingAddress(shippingAddressId);
                if (address != null)
                {
                    cityId = address.RegionId;
                }
            }
            else
            {
                var address = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
                if (address != null)
                {
                    // cityId = RegionApplication.GetRegion(address.RegionId, Region.RegionLevel.City).Id;
                    cityId = address.RegionId;
                }
            }
            var products = GenerateCartItem(skuIds, counts);
            var shopList = products.GroupBy(a => a.shopId);
            List<MobileShopCartItemModel> list = new List<MobileShopCartItemModel>();
            foreach (var shopcartItem in shopList)
            {
                IEnumerable<long> productIds = shopcartItem.Select(r => r.id);
                IEnumerable<int> productCounts = shopcartItem.Select(r => r.count);
                MobileShopCartItemModel item = new MobileShopCartItemModel();
                item.shopId = shopcartItem.Key;
                item.CartItemModels = products.Where(a => a.shopId == item.shopId).ToList();
                var baseCoupon = Service.GetOrdersCoupons(userid, CouponIdsStr);
                foreach (CartItemModel cartitemmodel in item.CartItemModels)
                {
                    var sku = _iProductService.GetSku(cartitemmodel.skuId);
                    if (sku == null)
                        throw new MallException("未找到库存!");
                    long collpid = 0;
                    for (int i = 0; i < skuIds.Count(); i++)
                    {
                        if (skuIds.ElementAt(i) == cartitemmodel.skuId)
                        {
                            collpid = collpids.ElementAt(i);
                            break;
                        }
                    }

                    cartitemmodel.price = GetSalePrice(cartitemmodel.id, sku, collpid, skuIds.Count(), cartitemmodel.count, userid);
                }
                if (cityId > 0)
                {
                    #region 指定地区包邮
                    IEnumerable<long> pIds;
                    IEnumerable<int> pCounts;
                    FreeShipping(cityId, shopcartItem, out pIds, out pCounts);
                    #endregion
                    if (pIds != null && pIds.Count() > 0 && pCounts != null && pCounts.Count() > 0)
                    {
                        item.Freight = _iProductService.GetFreight(pIds, pCounts, cityId);
                    }
                }

                //计算满额减的金额
                item.FullDiscount = GetShopFullDiscount(item.CartItemModels,  item.ShopBranchId > 0);
                if (baseCoupon != null)
                {
                    var couponAmount = item.ShopTotalWithoutFreight;
                    var coupon = baseCoupon.Where(a => a.ShopId == item.shopId).FirstOrDefault();
                    if (coupon != null && coupon.Type == 0)
                    {
                        var uc = (coupon.Coupon as Entities.CouponRecordInfo);
                        var bc = CouponApplication.Get(uc.CouponId);
                        if (bc.UseArea == 1)
                        {
                            var couponProducts = CouponApplication.GetCouponProductsByCouponId(uc.CouponId).Select(p => p.ProductId).ToList();
                            decimal coupontotal = 0;
                            foreach (var p in item.CartItemModels)
                            {
                                if (couponProducts.Contains(p.id))
                                    coupontotal += p.price * p.count - p.fullDiscount;
                            }
                            couponAmount = coupontotal;
                        }
                    }
                    item.OneCoupons = GetSelectedCoupon(item.ShopTotalWithoutFreight == couponAmount ? item.ShopTotalWithoutFreight : couponAmount, userid, item.shopId, baseCoupon);
                }
                else
                {
                    item.OneCoupons = GetDefaultCoupon(item.shopId, userid, item.ShopTotalWithoutFreight, item.CartItemModels);
                }
                item.BaseCoupons = GetBaseCoupon(item.shopId, userid, item.ShopTotalWithoutFreight, item.CartItemModels);
                decimal ordPaidPrice = CalculatePaidPrice(item);
                var shop = ShopApplication.GetShopInfo(item.shopId);
                item.ShopName = shop.ShopName;

                //满额免
                SetFullFree(ordPaidPrice, shop.FreeFreight, item);
                item.VshopId = item.VshopId = VshopApplication.GetVShopByShopId(shop.Id)?.Id ?? 0;
                //发票
                item.IsInvoice = ShopApplication.GetShopInvoiceConfig(item.shopId)?.IsInvoice ?? false;
                item.invoiceTpyes = ShopApplication.GetInvoiceTypes(item.shopId);
                item.InvoiceDay = GetInvoiceDays(item.shopId);
                list.Add(item);
            }
            var totalUserCoupons = list.Where(a => a.OneCoupons != null).Sum(b => b.OneCoupons.BasePrice);
            confirmModel.products = list;
            confirmModel.totalAmount = products.Sum(item => item.price * item.count) - list.Sum(a => a.FullDiscount);
            confirmModel.Freight = list.Sum(a => a.Freight);
            confirmModel.orderAmount = confirmModel.totalAmount + confirmModel.Freight - totalUserCoupons;
            var memberIntegral = MemberIntegralApplication.GetAvailableIntegral(userid);
            OrderIntegralModel integral = GetAvailableIntegral(confirmModel.totalAmount, totalUserCoupons, memberIntegral);
            confirmModel.integralPerMoney = integral.IntegralPerMoney;
            confirmModel.userIntegralMaxDeductible = integral.userIntegralMaxDeductible;
            confirmModel.integralPerMoneyRate = integral.integralPerMoneyRate;
            confirmModel.userIntegrals = integral.UserIntegrals;

            confirmModel.capitalAmount = MemberCapitalApplication.GetBalanceByUserId(userid);
        }

        /// <summary>
        /// 获取订单相关的产品信息-拼团
        /// </summary>
        /// <param name="model"></param>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="userid">用户id</param>
        /// <param name="GroupActionId">拼团活动编号</param>
        static void GetOrderProductsInfoOnGroup(string skuId, int count, long userid, long GroupActionId, MobileOrderDetailConfirmModel confirmModel, long? groupId = null)
        {
            int cityId = 0;
            var address = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
            if (address != null)
            {
                address.RegionFullName = RegionApplication.GetFullName(address.RegionId);
                //  cityId = RegionApplication.GetRegion(address.RegionId, Region.RegionLevel.City).Id;
                cityId = address.RegionId;
            }
            confirmModel.Address = address;
            var products = GenerateGroupItem(GroupActionId, skuId, count, groupId);

            var shopList = products.GroupBy(a => a.shopId);
            List<MobileShopCartItemModel> list = new List<MobileShopCartItemModel>();
            foreach (var shopcartItem in shopList)
            {
                IEnumerable<long> productIds = shopcartItem.Select(r => r.id);
                IEnumerable<int> productCounts = shopcartItem.Select(r => r.count);
                MobileShopCartItemModel item = new MobileShopCartItemModel();
                item.shopId = shopcartItem.Key;
                item.CartItemModels = products.Where(a => a.shopId == item.shopId).ToList();
                foreach (var product in item.CartItemModels)
                {
                    product.imgUrl = Core.MallIO.GetRomoteImagePath(product.imgUrl);
                }
                if (cityId > 0)
                {
                    #region 指定地区包邮
                    IEnumerable<long> pIds;
                    IEnumerable<int> pCounts;
                    FreeShipping(cityId, shopcartItem, out pIds, out pCounts);
                    #endregion
                    if (pIds != null && pIds.Count() > 0 && pCounts != null && pCounts.Count() > 0)
                    {
                        item.Freight = _iProductService.GetFreight(pIds, pCounts, cityId);
                    }
                }

                item.OneCoupons = null; //不可以使用优惠券
                decimal ordPaidPrice = CalculatePaidPrice(item);
                var shop = ShopApplication.GetShopInfo(item.shopId);
                item.ShopName = shop.ShopName;
                item.IsSelf = shop.IsSelf;
                //计算满额减的金额
                if (GroupActionId <= 0)
                    item.FullDiscount = GetShopFullDiscount(item.CartItemModels, item.ShopBranchId > 0);
                //满额免
                SetFullFree(ordPaidPrice, shop.FreeFreight, item);
                item.VshopId = item.VshopId = VshopApplication.GetVShopByShopId(shop.Id)?.Id ?? 0;
                item.IsOpenLadder = item.CartItemModels.Any(c => c.IsOpenLadder);
                //发票
                item.IsInvoice = ShopApplication.GetShopInvoiceConfig(item.shopId)?.IsInvoice ?? false;
                item.invoiceTpyes = ShopApplication.GetInvoiceTypes(item.shopId);
                item.InvoiceDay = GetInvoiceDays(item.shopId);
                list.Add(item);
            }
            var totalUserCoupons = 0; //不可以使用优惠券
            confirmModel.products = list;
            confirmModel.totalAmount = products.Sum(item => item.price * item.count) - list.Sum(e => e.FullDiscount);
            confirmModel.Freight = list.Sum(a => a.Freight);
            confirmModel.orderAmount = confirmModel.totalAmount + confirmModel.Freight - totalUserCoupons;

            confirmModel.capitalAmount = MemberCapitalApplication.GetBalanceByUserId(userid);

            //不可以使用积分
            confirmModel.integralPerMoney = 0;
            confirmModel.userIntegrals = 0;
        }

        /// <summary>
        /// 获取订单相关的产品信息
        /// </summary>
        /// <param name="userid">用户id</param>
        /// <param name="cartItemIds">购物车的物品id</param>
        /// <param name="confirmModel">保存数据的实体</param>
        static void GetOrderProductsInfo(long userid, string cartItemIds, MobileOrderDetailConfirmModel confirmModel, long shippingAddressId = 0, IEnumerable<string[]> CouponIdsStr = null)
        {
            IEnumerable<Mall.Entities.ShoppingCartItem> cartItems = null;
            var siteconfig = SiteSettingApplication.SiteSettings;
            if (string.IsNullOrWhiteSpace(cartItemIds))
                cartItems = GetCart(userid, "").Items;
            else
            {
                var cartItemIdsArr = cartItemIds.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(t => long.Parse(t));
                cartItems = CartApplication.GetCartItems(cartItemIdsArr);
            }
            int cityId = 0;
            if (shippingAddressId > 0)
            {
                var address = ShippingAddressApplication.GetUserShippingAddress(shippingAddressId);
                if (address != null)
                {
                    cityId = address.RegionId;
                }
            }
            else
            {
                var address = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
                if (address != null)
                {
                    var reg = ServiceProvider.Instance<IRegionService>.Create.GetRegion(address.RegionId);
                    cityId = (reg != null) ? reg.Id : cityId;
                }
            }
            var member = MemberApplication.GetMember(userid);
            var products = GenerateCartItem(cartItems);
            var shopList = products.GroupBy(a => a.shopId);
            List<MobileShopCartItemModel> list = new List<MobileShopCartItemModel>();
            var baseCoupon = Service.GetOrdersCoupons(userid, CouponIdsStr);
            foreach (var shopcartItem in shopList)
            {
                IEnumerable<long> productIds = shopcartItem.Select(r => r.id);
                IEnumerable<int> productCounts = shopcartItem.Select(r => r.count);
                MobileShopCartItemModel item = new MobileShopCartItemModel();
                item.shopId = shopcartItem.Key;
                item.VshopId = VshopApplication.GetVShopByShopId(item.shopId)?.Id ?? 0;
                if (products != null && products.Count > 0)
                {
                    item.ShopBranchId = products[0].ShopBranchId;
                    item.ShopBranchName = products[0].ShopBranchName;
                }
                item.CartItemModels = products.Where(a => a.shopId == item.shopId).ToList();

                var shop = ShopApplication.GetShopInfo(item.shopId);
                if (shop.IsSelf)
                {//只有官方自营店商品，才有会员折扣，会员折扣优先级高于满减、优惠券
                    //计算会员折扣
                    CalculateDiscountPrice(member.MemberDiscount, item.CartItemModels);
                }
                item.IsSelf = shop.IsSelf;
                //计算满额减的金额
                item.FullDiscount = GetShopFullDiscount(item.CartItemModels, item.ShopBranchId > 0);
                //满足优惠券(商品总金额除去满额减金额)

                if (baseCoupon != null)
                {
                    var couponAmount = item.ShopTotalWithoutFreight;
                    var coupon = baseCoupon.Where(a => a.ShopId == item.shopId).FirstOrDefault();
                    if (coupon != null && coupon.Type == 0)
                    {
                        var uc = (coupon.Coupon as Entities.CouponRecordInfo);
                        var bc = CouponApplication.Get(uc.CouponId);
                        if (bc.UseArea == 1)
                        {
                            var couponProducts = CouponApplication.GetCouponProductsByCouponId(uc.CouponId).Select(p => p.ProductId).ToList();
                            decimal coupontotal = 0;
                            foreach (var p in item.CartItemModels)
                            {
                                if (couponProducts.Contains(p.id))
                                    coupontotal += p.price * p.count - p.fullDiscount;
                            }
                            couponAmount = coupontotal;
                        }
                    }
                    item.OneCoupons = GetSelectedCoupon(item.ShopTotalWithoutFreight == couponAmount ? item.ShopTotalWithoutFreight : couponAmount, userid, item.shopId, baseCoupon);
                }
                else
                {
                    item.OneCoupons = GetDefaultCoupon(item.shopId, userid, item.ShopTotalWithoutFreight, item.CartItemModels);
                }
                if (item.ShopBranchId > 0)
                {
                    var shopBranchInfo = _iShopBranchService.GetShopBranchById(item.ShopBranchId);
                    if (shopBranchInfo != null)
                    {
                        if ((item.CartItemModels.Sum(a => a.price * a.count) - item.FullDiscount - (item.OneCoupons != null ? item.OneCoupons.BasePrice : 0)) >= shopBranchInfo.FreeMailFee && shopBranchInfo.IsFreeMail)
                            item.Freight = 0;
                        else
                            item.Freight = shopBranchInfo.DeliveFee;
                    }
                }
                else
                {
                    if (cityId > 0)
                    {
                        #region 指定地区包邮
                        IEnumerable<long> pIds;
                        IEnumerable<int> pCounts;
                        FreeShipping(cityId, shopcartItem, out pIds, out pCounts);
                        #endregion
                        if (pIds != null && pIds.Count() > 0 && pCounts != null && pCounts.Count() > 0)
                        {
                            item.Freight = _iProductService.GetFreight(pIds, pCounts, cityId);
                        }
                    }
                }
                item.BaseCoupons = GetBaseCoupon(item.shopId, userid, item.ShopTotalWithoutFreight, item.CartItemModels);
                decimal ordPaidPrice = CalculatePaidPrice(item);
                item.ShopName = shop.ShopName;
                //满额免
                if (item.ShopBranchId <= 0)
                    SetFullFree(ordPaidPrice, shop.FreeFreight, item);
                item.IsOpenLadder = item.CartItemModels.Any(c => c.IsOpenLadder);
                //发票
                item.IsInvoice = ShopApplication.GetShopInvoiceConfig(item.shopId)?.IsInvoice ?? false;
                item.invoiceTpyes = ShopApplication.GetInvoiceTypes(item.shopId);
                item.InvoiceDay = GetInvoiceDays(item.shopId);
                list.Add(item);
            }
            var totalUserCoupons = list.Where(a => a.OneCoupons != null).Sum(b => b.OneCoupons.BasePrice);


            if (list != null && list.Count > 0)
            {
                if ( list[0].ShopBranchId> 0)
                {
                    confirmModel.shopBranchInfo = _iShopBranchService.GetShopBranchById(list[0].ShopBranchId);
                }
            }
            confirmModel.products = list;
            confirmModel.totalAmount = products.Sum(item => item.price * item.count) - list.Sum(a => a.FullDiscount);
            if (confirmModel.shopBranchInfo != null)
            {
                if (confirmModel.shopBranchInfo.DeliveTotalFee > products.Sum(item => item.price * item.count))
                {
                    throw new MallException("订单未达到起送费用");
                }
                if ((confirmModel.totalAmount - totalUserCoupons) >= confirmModel.shopBranchInfo.FreeMailFee && confirmModel.shopBranchInfo.IsFreeMail)
                    confirmModel.Freight = 0;
                else
                    confirmModel.Freight = confirmModel.shopBranchInfo.DeliveFee;
            }
            else
                confirmModel.Freight = list.Sum(a => a.Freight);
            confirmModel.orderAmount = confirmModel.totalAmount + confirmModel.Freight - totalUserCoupons;

            confirmModel.capitalAmount = MemberCapitalApplication.GetBalanceByUserId(userid);
            var memberIntegralInfo = MemberIntegralApplication.GetMemberIntegral(userid);
            var memberIntegral = memberIntegralInfo == null ? 0 : memberIntegralInfo.AvailableIntegrals;

            confirmModel.memberIntegralInfo = memberIntegralInfo;
            OrderIntegralModel integral = GetAvailableIntegral(confirmModel.totalAmount, totalUserCoupons, memberIntegral);
            confirmModel.integralPerMoney = integral.IntegralPerMoney;
            confirmModel.userIntegralMaxDeductible = integral.userIntegralMaxDeductible;
            confirmModel.IntegralDeductibleRate = siteconfig.IntegralDeductibleRate;
            confirmModel.integralPerMoneyRate = integral.integralPerMoneyRate;
            confirmModel.userIntegrals = integral.UserIntegrals;
        }

        /// <summary>
        /// 获取收获地址
        /// </summary>
        /// <param name="userid">用户id</param>
        /// <param name="confirm">保存数据的实体</param>
        static void GetShippingAddress(long userid, MobileOrderDetailConfirmModel confirm, long shippingAddressId = 0)
        {
            if (confirm.shopBranchInfo == null)
            {
                var address = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
                if (address != null)
                {
                    bool hasRegion = PaymentConfigApplication.IsCashOnDelivery(address.RegionId);
                    var isEnable = PaymentConfigApplication.IsEnable();
                    if (hasRegion && isEnable)
                    {
                        confirm.IsCashOnDelivery = true;
                    }
                    else
                    {
                        confirm.IsCashOnDelivery = false;
                    }
                }
                else
                {
                    confirm.IsCashOnDelivery = false;
                }
                confirm.Address = address;
            }
            else
            {
                var addressList = ShippingAddressApplication.GetUserShippingAddressByUserId(userid).OrderByDescending(n => n.IsDefault);
                Entities.ShippingAddressInfo address = null;
                bool hasget = false;
                if (addressList != null && addressList.Count() > 0)
                {
                    foreach (var item in addressList)
                    {
                        if (confirm.shopBranchInfo.ServeRadius == 0)//门店无配送范围不可配送，
                            continue;
                        string form = string.Format("{0},{1}", item.Latitude, item.Longitude);//收货地址的经纬度
                        if (form.Length <= 1)
                            continue;//地址不含经纬度的不可配送
                        double Distances = _iShopBranchService.GetLatLngDistancesFromAPI(form, string.Format("{0},{1}", confirm.shopBranchInfo.Latitude, confirm.shopBranchInfo.Longitude));
                        if (Distances > confirm.shopBranchInfo.ServeRadius)
                            continue;//距离超过配送距离的不可配送,距离计算失误无法配送

                        bool hasRegion = PaymentConfigApplication.IsCashOnDelivery(item.RegionId);
                        var isEnable = PaymentConfigApplication.IsEnable();
                        if (hasRegion && isEnable)
                        {
                            confirm.IsCashOnDelivery = true;
                        }
                        else
                        {
                            confirm.IsCashOnDelivery = false;
                        }
                        if (shippingAddressId > 0)
                        {
                            if (item.Id == shippingAddressId)
                            {
                                address = item;
                                hasget = true;
                                break;
                            }
                            else
                            {
                                if (address == null)
                                    address = item;
                            }
                        }
                        else
                        {
                            if (!hasget)
                            {
                                address = item;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    confirm.IsCashOnDelivery = false;
                }
                confirm.Address = address;
            }
        }

        public static BaseCoupon GetSelectedCoupon(decimal totalPrice, long userid, long shopId, IEnumerable<BaseAdditionalCoupon> baseCoupons)
        {
            BaseCoupon c;
            if (baseCoupons != null)
            {
                var coupon = baseCoupons.Where(a => a.ShopId == shopId).FirstOrDefault();
                if (coupon != null)//存在使用优惠券的情况
                {
                    if (coupon.Type == 0)//优惠券
                    {
                        var uc = (coupon.Coupon as Entities.CouponRecordInfo);
                        var info = CouponApplication.GetCouponInfo(uc.CouponId);
                        c = new BaseCoupon();
                        c.BaseEndTime = info.EndTime;
                        c.BaseId = uc.Id;
                        c.BaseName = info.CouponName;
                        c.BasePrice = info.Price;
                        c.BaseShopId = uc.ShopId;
                        c.BaseShopName = uc.ShopName;
                        c.BaseType = CommonModel.CouponType.Coupon;
                        c.OrderAmount = info.OrderAmount;
                        if (c.BasePrice >= totalPrice)
                            c.BasePrice = totalPrice;
                        return c;
                    }
                    else if (coupon.Type == 1)//代金红包
                    {
                        var sb = (coupon.Coupon as ShopBonusReceiveInfo);

                        var service = GetService<IShopBonusService>();
                        var grant = service.GetGrant(sb.BonusGrantId);
                        var bonus = service.GetShopBonus(grant.ShopBonusId);
                        var shop = ShopApplication.GetShop(bonus.ShopId);

                        c = new BaseCoupon();
                        c.BaseEndTime = bonus.BonusDateEnd;
                        c.BaseId = sb.Id;
                        c.BaseName = bonus.Name;
                        c.BasePrice = sb.Price;
                        c.BaseShopId = shop.Id;
                        c.BaseShopName = shop.ShopName;
                        c.BaseType = CommonModel.CouponType.ShopBonus;
                        c.OrderAmount = bonus.UsrStatePrice;
                        //超过优惠券金额，使用优惠券最大金额
                        if (c.BasePrice >= totalPrice)
                            c.BasePrice = totalPrice;
                        return c;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 在无法手动选择优惠券的场景下，自动选择合适的优惠券
        /// </summary>
        public static BaseCoupon GetDefaultCoupon(long shopid, long userid, decimal totalPrice, List<CartItemModel> cartItems = null)
        {
            var shopBonus = ShopBonusApplication.GetDetailToUse(shopid, userid, totalPrice);
            var userCouponsAll = CouponApplication.GetUserCoupon(shopid, userid, totalPrice);
            List<CouponRecordInfo> list = new List<CouponRecordInfo>();
            var coupons = CouponApplication.GetCouponInfo(userCouponsAll.Select(p => p.CouponId));
            foreach (var coupon in userCouponsAll)
            {
                var cou = coupons.FirstOrDefault(p => p.Id == coupon.CouponId);
                coupon.CouponInfo = cou;
                if (cou.UseArea == 1)
                {
                    var pids = CouponApplication.GetCouponProductsByCouponId(coupon.CouponId).Select(p => p.ProductId).ToList();
                    decimal totalAmount = 0;
                    var canUse = false;
                    foreach (var cartitem in cartItems)
                    {
                        if (pids.Contains(cartitem.id))
                        {
                            totalAmount += cartitem.count * cartitem.price - cartitem.fullDiscount;
                            canUse = true;
                        }
                    }
                    if (canUse && totalAmount >= cou.OrderAmount)
                    {
                        if (cou.Price > totalAmount)
                        {
                            cou.Price = totalAmount;
                        }
                        list.Add(coupon);
                    }
                }
                else
                {
                    list.Add(coupon);
                }
            }
            var userCoupons = list.OrderByDescending(p => p.CouponInfo.Price).ToList();
            BaseCoupon c;
            if (shopBonus.Count() > 0 && userCoupons.Count() > 0)
            {
                var sb = shopBonus.FirstOrDefault();      //商家红包
                var uc = userCoupons.FirstOrDefault();  //优惠卷
                var info = CouponApplication.GetCouponInfo(uc.CouponId);
                if (sb.Price > info.Price)
                {
                    c = new BaseCoupon();
                    var service = GetService<IShopBonusService>();
                    var grant = service.GetGrant(sb.BonusGrantId);
                    var bonus = service.GetShopBonus(grant.ShopBonusId);
                    var shop = ShopApplication.GetShop(bonus.ShopId);

                    c.BaseEndTime = bonus.BonusDateEnd;
                    c.BaseId = sb.Id;
                    c.BaseName = bonus.Name;
                    c.BasePrice = sb.Price;
                    c.BaseShopId = shop.Id;
                    c.BaseShopName = shop.ShopName;
                    c.BaseType = CommonModel.CouponType.ShopBonus;
                    c.OrderAmount = bonus.UsrStatePrice;
                    //超过优惠券金额，使用优惠券最大金额
                    if (c.BasePrice >= totalPrice)
                        c.BasePrice = totalPrice;

                    return c;
                }
                else
                {
                    c = new BaseCoupon();

                    c.BaseEndTime = info.EndTime;
                    c.BaseId = uc.Id;
                    c.BaseName = info.CouponName;
                    c.BasePrice = info.Price;
                    c.BaseShopId = info.ShopId;
                    c.BaseShopName = info.ShopName;
                    c.BaseType = CommonModel.CouponType.Coupon;
                    c.OrderAmount = info.OrderAmount;

                    var totalAmount = totalPrice;
                    if (info.UseArea == 1)
                    {
                        var couponProducts = CouponApplication.GetCouponProductsByCouponId(uc.CouponId).Select(p => p.ProductId).ToList();
                        decimal coupontotal = 0;
                        foreach (var p in cartItems)
                        {
                            if (couponProducts.Contains(p.id))
                                coupontotal += p.price * p.count - p.fullDiscount;
                        }
                        totalAmount = coupontotal;
                    }
                    if (c.BasePrice >= totalAmount)
                        c.BasePrice = totalAmount;
                    return c;
                }
            }
            else if (shopBonus.Count() <= 0 && userCoupons.Count() <= 0)
            {
                return null;
            }
            else if (shopBonus.Count() <= 0 && userCoupons.Count() > 0)
            {
                var coupon = userCoupons.FirstOrDefault();
                c = new BaseCoupon();
                var info = CouponApplication.GetCouponInfo(coupon.CouponId);
                c.BaseEndTime = info.EndTime;
                c.BaseId = coupon.Id;
                c.BaseName = info.CouponName;
                c.BasePrice = info.Price;
                c.BaseShopId = info.ShopId;
                c.BaseShopName = info.ShopName;
                c.BaseType = CommonModel.CouponType.Coupon;
                c.OrderAmount = info.OrderAmount;
                var totalAmount = totalPrice;
                if (info.UseArea == 1)
                {
                    var couponProducts = CouponApplication.GetCouponProductsByCouponId(coupon.CouponId).Select(p => p.ProductId).ToList();
                    decimal coupontotal = 0;
                    foreach (var p in cartItems)
                    {
                        if (couponProducts.Contains(p.id))
                            coupontotal += p.price * p.count - p.fullDiscount;
                    }
                    totalAmount = coupontotal;
                }
                if (c.BasePrice >= totalAmount)
                    c.BasePrice = totalAmount;
                return c;
            }
            else if (shopBonus.Count() > 0 && userCoupons.Count() <= 0)
            {
                var coupon = shopBonus.FirstOrDefault();
                c = new BaseCoupon();


                var service = GetService<IShopBonusService>();
                var grant = service.GetGrant(coupon.BonusGrantId);
                var bonus = service.GetShopBonus(grant.ShopBonusId);
                var shop = ShopApplication.GetShop(bonus.ShopId);

                c.BaseEndTime = bonus.BonusDateEnd;
                c.BaseId = coupon.Id;
                c.BaseName = bonus.Name;
                c.BasePrice = coupon.Price;
                c.BaseShopId = shop.Id;
                c.BaseShopName = shop.ShopName;
                c.BaseType = CommonModel.CouponType.ShopBonus;
                c.OrderAmount = bonus.UsrStatePrice;
                if (c.BasePrice >= totalPrice)
                    c.BasePrice = totalPrice;
                return c;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 满额免运费
        /// </summary>
        static void SetFullFree(decimal ordPaidPrice, decimal freeFreight, MobileShopCartItemModel item)
        {
            item.IsFreeFreight = false;
            if (freeFreight > 0)
            {
                item.FreeFreight = freeFreight;
                if (ordPaidPrice >= freeFreight)
                {
                    item.Freight = 0;
                    item.IsFreeFreight = true;
                }
            }
        }

        /// <summary>
        /// 计算积分
        /// </summary>
        static OrderIntegralModel GetAvailableIntegral(decimal totalAmount, decimal totalUserCoupons, decimal memberIntegral)
        {
            var integralPerMoney = _iMemberIntegralService.GetIntegralChangeRule();
            var siteset = SiteSettingApplication.SiteSettings;
            decimal maxDeductible = decimal.Round((siteset.IntegralDeductibleRate * (totalAmount - totalUserCoupons)) / (decimal)100, 2, MidpointRounding.AwayFromZero);
            if (maxDeductible < 0)
            {
                maxDeductible = 0;
            }
            if (totalAmount - totalUserCoupons < maxDeductible)
            {
                maxDeductible = totalAmount - totalUserCoupons;
            }

            OrderIntegralModel result = new OrderIntegralModel();
            if (integralPerMoney != null && integralPerMoney.IntegralPerMoney > 0 && maxDeductible > 0)
            {
                var inteMoney = memberIntegral / integralPerMoney.IntegralPerMoney;
                inteMoney = Math.Floor(inteMoney * (decimal)Math.Pow(10, 2)) / (decimal)Math.Pow(10, 2);
                if (maxDeductible - inteMoney > 0)
                {
                    result.IntegralPerMoney = inteMoney;
                    result.userIntegralMaxDeductible = result.IntegralPerMoney;
                    result.UserIntegrals = memberIntegral;
                }
                else
                {
                    //result.IntegralPerMoney = Math.Round(totalAmount - totalUserCoupons, 2);
                    result.UserIntegrals = Math.Ceiling(maxDeductible * integralPerMoney.IntegralPerMoney);
                    var userinteMoney = result.UserIntegrals / integralPerMoney.IntegralPerMoney;
                    userinteMoney = Math.Floor(userinteMoney * (decimal)Math.Pow(10, 2)) / (decimal)Math.Pow(10, 2);
                    result.IntegralPerMoney = userinteMoney;
                    result.userIntegralMaxDeductible = result.IntegralPerMoney;
                }
                if (result.IntegralPerMoney <= 0)
                {
                    result.IntegralPerMoney = 0;
                    result.userIntegralMaxDeductible = 0;
                    result.UserIntegrals = 0;
                }
            }
            else
            {
                result.IntegralPerMoney = 0;
                result.userIntegralMaxDeductible = 0;
                result.UserIntegrals = 0;
            }
            if (result.IntegralPerMoney > maxDeductible)
            {
                result.IntegralPerMoney = maxDeductible;
            }
            if (result.userIntegralMaxDeductible > maxDeductible)
            {
                result.userIntegralMaxDeductible = maxDeductible;
            }
            if (integralPerMoney != null)
            {
                result.integralPerMoneyRate = integralPerMoney.IntegralPerMoney;
            }
            return result;
        }

        /// <summary>
        /// 计算需付款
        /// </summary>
        static decimal CalculatePaidPrice(MobileShopCartItemModel cart)
        {
            decimal ordTotalPrice = cart.ShopTotalWithoutFreight;
            decimal ordDisPrice = cart.OneCoupons == null ? 0 : cart.OneCoupons.BasePrice;
            return ordTotalPrice - ordDisPrice;
        }
        /// <summary>
        /// 获取销量
        /// </summary>
        /// <returns></returns>
        public static long GetSaleCount(DateTime? startDate = null, DateTime? endDate = null, long? shopBranchId = null, long? shopId = null, long? productId = null, bool hasReturnCount = true, bool hasWaitPay = false)
        {
            return Service.GetSaleCount(startDate, endDate, shopBranchId, shopId, productId, hasReturnCount, hasWaitPay);
        }

        #endregion

        #region 公共方法
        public static List<InvoiceTitleInfo> GetInvoiceTitles(long userid)
        {
            return Service.GetInvoiceTitles(userid, InvoiceType.OrdinaryInvoices);
        }

        /// <summary>
        /// 订单完成订单数据写入待结算表
        /// </summary>
        /// <param name="o"></param>
        public static void WritePendingSettlnment(OrderInfo o)
        {
            Service.WritePendingSettlnment(o);
        }
        /// <summary>
        /// 获取订单统计
        /// </summary>
        /// <param name="shop">门店ID,为0表示不筛选</param>
        /// <param name="begin">开始日期</param>
        /// <param name="end">结束日期</param>
        /// <returns></returns>
        public static OrderDayStatistics GetDayStatistics(long shop, DateTime begin, DateTime end)
        {
            return Service.GetOrderDayStatistics(shop, begin, end);
        }

        /// <summary>
        /// 获取订单统计
        /// </summary>
        /// <param name="shop"></param>
        /// <returns></returns>
        public static OrderDayStatistics GetYesterDayStatistics(long shop)
        {
            var today = DateTime.Now.Date;
            var yesterday = today.AddDays(-1);
            var key = CacheKeyCollection.YesterDayStatistics(shop);
            var result = Cache.Get<OrderDayStatistics>(key);
            if (result == null)
            {
                result = GetDayStatistics(shop, yesterday, today);
                Cache.Insert(key, result);
            }
            return result;
        }


        /// <summary>
        /// 商家给订单备注
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="remark"></param>
        /// <param name="shopId">店铺ID</param>
        /// <param name="flag">紧急标识</param>
        public static void UpdateSellerRemark(long orderId, long shopId, string remark, int flag)
        {
            var order = Service.GetOrder(orderId);
            if (order == null)
                throw new MessageException(ExceptionMessages.NoFound, "订单");
            if (order.ShopId != shopId)
                throw new MessageException(ExceptionMessages.UnauthorizedOperation);
            Service.UpdateSellerRemark(orderId, remark, flag);
        }

        /// <summary>
        /// 根据订单id获取OrderPayInfo
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public static List<OrderPay> GetOrderPays(IEnumerable<long> orderIds)
        {
            var list = Service.GetOrderPays(orderIds);
            // return AutoMapper.Mapper.Map<List<OrderPay>>(list);
            return list.Map<List<OrderPay>>();

        }

        /// <summary>
        /// 根据订单项id获取订单项
        /// </summary>
        /// <param name="orderItemIds"></param>
        /// <returns></returns>
        public static List<OrderItem> GetOrderItems(IEnumerable<long> orderItemIds)
        {
            var list = Service.GetOrderItemsByOrderItemId(orderItemIds);
            return list.Map<List<OrderItem>>();
        }

        public static List<OrderItemInfo> GetOrderItems(long order)
        {
            return Service.GetOrderItemsByOrderId(order);
        }

        /// <summary>
        /// 根据订单id获取订单项
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static List<OrderItem> GetOrderItemsByOrderId(long orderId)
        {
            var list = Service.GetOrderItemsByOrderId(orderId);
            return list.Map<List<OrderItem>>();
        }

        /// <summary>
        /// 根据订单id获取订单项
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public static List<OrderItem> GetOrderItemsByOrderId(IEnumerable<long> orderIds)
        {
            var list = Service.GetOrderItemsByOrderId(orderIds);
            return list.Map<List<DTO.OrderItem>>();
        }

        /// <summary>
        /// 获取订单的评论数
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public static Dictionary<long, long> GetOrderCommentCount(IEnumerable<long> orderIds)
        {
            return Service.GetOrderCommentCount(orderIds);
        }

        public static long GetOrderCommentCount(long order)
        {
            var result = GetOrderCommentCount(new List<long> { order });
            return result.ContainsKey(order) ? result[order] : 0;
        }

        /// <summary>
        /// 根据订单项id获取售后记录
        /// </summary>
        /// <param name="orderItemIds"></param>
        /// <returns></returns>
        public static List<DTO.OrderRefund> GetOrderRefunds(IEnumerable<long> orderItemIds)
        {
            var result = Service.GetOrderRefunds(orderItemIds).Map<List<DTO.OrderRefund>>();
            return result;
        }
        public static List<OrderRefundInfo> GetOrderRefundsByOrder(long order)
        {
            return GetService<IRefundService>().GetOrderRefundsByOrder(order);
        }
        /// <summary>
        /// 商家发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="companyName"></param>
        /// <param name="shipOrderNumber"></param>
        /// <param name="kuaidi100ReturnUrl"></param>
        public static void SellerSendGood(long orderId, string sellerName, string companyName, string shipOrderNumber, string kuaidi100ReturnUrl = "")
        {
            var order = Service.SellerSendGood(orderId, sellerName, companyName, shipOrderNumber);
            var siteSetting = SiteSettingApplication.SiteSettings;
            var key = siteSetting.Kuaidi100Key;
            if (!string.IsNullOrEmpty(key))
            {
                Task.Factory.StartNew(() => ServiceProvider.Instance<IExpressService>.Create.SubscribeExpress100(order.ExpressCompanyName, order.ShipOrderNumber, key, order.RegionFullName, kuaidi100ReturnUrl));
            }
            if (siteSetting.KuaidiType != 0)
            {//快递鸟物流轨迹，部分物流公司需要先订阅，目前通过调用获取接口实现
                Task.Factory.StartNew(() => ServiceProvider.Instance<IExpressService>.Create.GetExpressData(order.ExpressCompanyName, order.ShipOrderNumber));
            }
            var orderitems = OrderApplication.GetOrderItems(order.Id);
            //发送通知消息
            var orderMessage = new MessageOrderInfo();
            orderMessage.OrderTime = order.OrderDate;
            orderMessage.OrderId = order.Id.ToString();
            orderMessage.ShopId = order.ShopId;
            orderMessage.UserName = order.UserName;
            orderMessage.ShopName = order.ShopName;
            orderMessage.SiteName = siteSetting.SiteName;
            orderMessage.TotalMoney = order.OrderTotalAmount;
            orderMessage.ShippingCompany = string.IsNullOrEmpty(companyName) ? "商家自有物流" : companyName;
            orderMessage.ShippingNumber = string.IsNullOrEmpty(shipOrderNumber) ? "无" : shipOrderNumber;
            orderMessage.ShipTo = (order.Platform == PlatformType.WeiXinSmallProg) ? ((DateTime)order.ShippingDate).ToString("yyyy-MM-dd HH:mm:ss") : (order.ShipTo + " " + order.RegionFullName + " " + order.Address);
            orderMessage.ProductName = orderitems.FirstOrDefault().ProductName;
            if (order.Platform == PlatformType.WeiXinSmallProg)
            {
                orderMessage.MsgOrderType = MessageOrderType.Applet;
            }
            Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnOrderShipping(order.UserId, orderMessage));
        }

        /// <summary>
        /// 门店发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="companyName"></param>
        /// <param name="shipOrderNumber"></param>
        /// <param name="kuaidi100ReturnUrl"></param>
        public static void ShopSendGood(long orderId, int deliveryType, string shopkeeperName, string companyName, string shipOrderNumber, string kuaidi100ReturnUrl = "")
        {
            var order = Service.ShopSendGood(orderId, deliveryType, shopkeeperName, companyName, shipOrderNumber);
            if (deliveryType != 2 && deliveryType != DeliveryType.CityExpress.GetHashCode())
            {
                var siteSetting = SiteSettingApplication.SiteSettings;

                var key = siteSetting.Kuaidi100Key;
                if (!string.IsNullOrEmpty(key))
                {
                    Task.Factory.StartNew(() => ServiceProvider.Instance<IExpressService>.Create.SubscribeExpress100(order.ExpressCompanyName, order.ShipOrderNumber, key, order.RegionFullName, kuaidi100ReturnUrl));
                }
                if (siteSetting.KuaidiType != 0)
                {//快递鸟物流轨迹，部分物流公司需要先订阅，目前通过调用获取接口实现
                    Task.Factory.StartNew(() => ServiceProvider.Instance<IExpressService>.Create.GetExpressData(order.ExpressCompanyName, order.ShipOrderNumber));
                }
            }
            //发送通知消息
            if (deliveryType != DeliveryType.CityExpress.GetHashCode())  //达达物流在回调中发送消息
            {
                SendMessageOnOrderShipping(orderId);
            }
        }
        public static void SendMessageOnOrderShipping(long orderId)
        {
            var order = Service.GetOrder(orderId);
#if DEBUG
            Log.Debug("[SGM]" + orderId + "_" + order.ExpressCompanyName + "_" + order.ShipOrderNumber);
#endif
            var orderitems = OrderApplication.GetOrderItems(order.Id);
            //发送通知消息
            var orderMessage = new MessageOrderInfo();
            orderMessage.OrderTime = order.OrderDate;
            orderMessage.OrderId = order.Id.ToString();
            orderMessage.ShopId = order.ShopId;
            orderMessage.UserName = order.UserName;
            orderMessage.ShopName = order.ShopName;
            orderMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;
            orderMessage.TotalMoney = order.OrderTotalAmount;
            orderMessage.ShippingCompany = string.IsNullOrEmpty(order.ExpressCompanyName) ? "商家自有物流" : order.ExpressCompanyName;
            orderMessage.ShippingNumber = string.IsNullOrEmpty(order.ShipOrderNumber) ? "无" : order.ShipOrderNumber;
            orderMessage.ShipTo = (order.Platform == PlatformType.WeiXinSmallProg) ? ((DateTime)order.ShippingDate).ToString("yyyy-MM-dd HH:mm:ss") : (order.ShipTo + " " + order.RegionFullName + " " + order.Address);
            orderMessage.ProductName = orderitems.FirstOrDefault().ProductName;
            if (order.Platform == PlatformType.WeiXinSmallProg)
            {
                orderMessage.MsgOrderType = MessageOrderType.Applet;
            }
            Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnOrderShipping(order.UserId, orderMessage));
        }
        /// <summary>
        /// 判断订单是否正在申请售后
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static bool IsOrderAfterService(long orderId)
        {
            return Service.IsOrderAfterService(orderId);
        }

        public static DTO.ExpressData GetExpressData(string expressCompanyName, string shipOrderNumber)
        {
            return _iExpressService.GetExpressData(expressCompanyName, shipOrderNumber);
        }

        /// <summary>
        /// 修改快递信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="companyName"></param>
        /// <param name="shipOrderNumber"></param>
        /// <returns></returns>
        public static void UpdateExpress(long orderId, string companyName, string shipOrderNumber, string kuaidi100ReturnUrl = "")
        {
            var order = Service.UpdateExpress(orderId, companyName, shipOrderNumber);

            var key = SiteSettingApplication.SiteSettings.Kuaidi100Key;
            if (!string.IsNullOrEmpty(key))
            {
                Task.Factory.StartNew(() => ServiceProvider.Instance<IExpressService>.Create.SubscribeExpress100(order.ExpressCompanyName, order.ShipOrderNumber, key, order.RegionFullName, kuaidi100ReturnUrl));
            }
        }
        /// <summary>
        /// 所有订单是否都支付
        /// </summary>
        /// <param name="orderids"></param>
        /// <returns></returns>
        public static bool AllOrderIsPaied(string orderids)
        {
            var orders = Service.GetOrders(orderids.Split(',').Select(t => long.Parse(t)));
            IEnumerable<OrderInfo> waitPayOrders = orders.Where(p => p.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay);
            if (waitPayOrders.Count() > 0)
            {//有待付款的订单，则未支付完成
                return false;
            }
            return true;
        }
        #endregion

        #region web私有方法

        /// <summary>
        /// 对PaymentId进行加密（因为PaymentId中包含小数点"."，因此进行编码替换）
        /// </summary>
        static string EncodePaymentId(string paymentId)
        {
            return paymentId.Replace(".", "-");
        }
        /// <summary>
        /// 取得商品描述字符串
        /// </summary>
        /// <param name="orders">商品对象集合</param>
        /// <returns>描述字符串</returns>
        static string GetProductNameDescriptionFromOrders(IEnumerable<OrderInfo> orders)
        {
            var orderitems = OrderApplication.GetOrderItemsByOrderId(orders.Select(p => p.Id));
            var productNames = orderitems.Select(p => p.ProductName).ToList();

            string productInfos = "";
            if (productNames.Count > 0)
                productInfos = productNames.Count() > 1 ? (productNames.ElementAt(0) + " 等" + productNames.Count() + "种商品") : productNames.ElementAt(0);
            return productInfos;
        }

        /// <summary>
        /// 将前端传入参数转换成适合操作的格式
        /// </summary>
        static IEnumerable<string[]> ConvertUsedCoupon(string couponIds)
        {
            //couponIds格式  "id_type,id_type,id_type"
            IEnumerable<string> couponArr = null;
            if (!string.IsNullOrEmpty(couponIds))
            {
                couponArr = couponIds.Split(',');
            }

            //返回格式  string[0] = id , string[1] = type
            return couponArr == null ? null : couponArr.Select(p => p.Split('_'));
        }


        static void GetOrderProductsInfo(OrderSubmitModel model, string cartItemIds, long? regionId, long userid, string cartInfo, IEnumerable<string[]> CouponIdsStr = null)
        {
            Entities.ShippingAddressInfo address = new Entities.ShippingAddressInfo();
            if (regionId != null)
            {
                address = ShippingAddressApplication.GetUserShippingAddress((long)regionId);
            }
            else
            {
                address = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
            }
            int cityId = 0;
            if (address != null)
            {
                cityId = address.RegionId;
            }

            IEnumerable<Mall.Entities.ShoppingCartItem> cartItems = null;
            if (string.IsNullOrWhiteSpace(cartItemIds))
                cartItems = GetCart(userid, cartInfo).Items;
            else
            {
                var cartItemIdsArr = cartItemIds.Split(',').Select(t => long.Parse(t));
                cartItems = CartApplication.GetCartItems(cartItemIdsArr);
            }

            var products = GenerateCartItem(cartItems);
            var shopList = products.GroupBy(a => a.shopId);
            List<ShopCartItemModel> list = new List<ShopCartItemModel>();
            var orderService = Service;

            decimal discount = model.Member.MemberDiscount;
            var baseCoupon = Service.GetOrdersCoupons(userid, CouponIdsStr);
            foreach (var shopcartItem in shopList)
            {
                IEnumerable<long> productIds = shopcartItem.Select(r => r.id);
                IEnumerable<int> counts = shopcartItem.Select(r => r.count);

                ShopCartItemModel item = new ShopCartItemModel();
                item.shopId = shopcartItem.Key;
                var shopInfo = ShopApplication.GetShop(item.shopId);
                item.CartItemModels = products.Where(a => a.shopId == item.shopId).ToList();
                var shop = ShopApplication.GetShopInfo(item.shopId);
                item.ShopName = shop.ShopName;
                item.FreeFreight = shop.FreeFreight;
                if (cityId > 0)
                {
                    #region 指定地区包邮
                    IEnumerable<long> pIds;
                    IEnumerable<int> pCounts;
                    FreeShipping(cityId, shopcartItem, out pIds, out pCounts);
                    #endregion
                    if (pIds != null && pIds.Count() > 0 && pCounts != null && pCounts.Count() > 0)
                    {
                        item.Freight = _iProductService.GetFreight(pIds, pCounts, cityId);
                    }
                }
                //会员折扣
                foreach (var c in item.CartItemModels)
                {
                    var price = c.price * discount;
                    c.price = shopInfo.IsSelf ? price : c.price;
                }

                //计算满额减的金额
                item.FullDiscount = GetShopFullDiscount(item.CartItemModels);
                //默认优惠券
                if (baseCoupon != null)
                {
                    var couponAmount = item.ShopTotalWithoutFreight;
                    var coupon = baseCoupon.Where(a => a.ShopId == item.shopId).FirstOrDefault();
                    if (coupon != null && coupon.Type == 0)
                    {
                        var uc = (coupon.Coupon as Entities.CouponRecordInfo);
                        var bc = CouponApplication.Get(uc.CouponId);
                        if (bc.UseArea == 1)
                        {
                            var couponProducts = CouponApplication.GetCouponProductsByCouponId(uc.CouponId).Select(p => p.ProductId).ToList();
                            decimal coupontotal = 0;
                            foreach (var p in item.CartItemModels)
                            {
                                if (couponProducts.Contains(p.id))
                                    coupontotal += p.price * p.count - p.fullDiscount;
                            }
                            couponAmount = coupontotal;
                        }
                    }
                    item.OneCoupons = GetSelectedCoupon(item.ShopTotalWithoutFreight == couponAmount ? item.ShopTotalWithoutFreight : couponAmount, userid, item.shopId, baseCoupon);
                }
                else
                {
                    item.OneCoupons = GetDefaultCoupon(item.shopId, userid, item.ShopTotalWithoutFreight, item.CartItemModels);
                }
                //优惠券
                item.BaseCoupons = GetBaseCoupon(item.shopId, userid, item.ShopTotalWithoutFreight, item.CartItemModels, true);

                var OrderItems = GetOrderItems(item);
                item.freightProductGroup = OrderItems.OrderBy(e => e.FreightTemplateId).ToList();
                //发票
                item.IsInvoice = ShopApplication.GetShopInvoiceConfig(item.shopId)?.IsInvoice ?? false;
                item.invoiceTpyes = ShopApplication.GetInvoiceTypes(item.shopId);
                item.InvoiceDay = GetInvoiceDays(item.shopId);
                list.Add(item);
            }
            model.products = list;
            model.totalAmount = products.Sum(item => decimal.Round(item.price, 2, MidpointRounding.AwayFromZero) * item.count) - list.Sum(a => a.FullDiscount);
            model.Freight = list.Sum(a => a.Freight);
        }

        /// <summary>
        /// 设置发票信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userid"></param>
        static void SetOrderInvoiceInfo(OrderSubmitModel model, long userid)
        {
            model.InvoiceTitle = Service.GetInvoiceTitles(userid, InvoiceType.OrdinaryInvoices);
            model.InvoiceContext = Service.GetInvoiceContexts();
            //默认台头
            string invoiceName = string.Empty;
            string invoiceCode = string.Empty;
            //默认电子发票信息
            string cellPhone = string.Empty;
            string email = string.Empty;
            model.vatInvoice = GetDefaultInvoiceInfo(userid, ref cellPhone, ref email, ref invoiceName, ref invoiceCode);
            model.cellPhone = cellPhone;
            model.email = email;
            model.invoiceName = invoiceName;
            model.invoiceCode = invoiceCode;
        }

        static void SetMobileOrderInvoiceInfo(MobileOrderDetailConfirmModel model, long userid)
        {
            model.InvoiceContext = Service.GetInvoiceContexts();
            model.InvoiceTitle = Service.GetInvoiceTitles(userid, InvoiceType.OrdinaryInvoices);
            //默认台头
            string invoiceName = string.Empty;
            string invoiceCode = string.Empty;
            //默认电子发票信息
            string cellPhone = string.Empty;
            string email = string.Empty;
            model.vatInvoice = GetDefaultInvoiceInfo(userid, ref cellPhone, ref email, ref invoiceName, ref invoiceCode);
            model.cellPhone = cellPhone;
            model.email = email;
            model.invoiceName = invoiceName;
            model.invoiceCode = invoiceCode;
        }
        /// <summary>
        /// 获取默认发票信息
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="cellPhone"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        static InvoiceTitleInfo GetDefaultInvoiceInfo(long userid, ref string cellPhone, ref string email,ref string invoiceName,ref string invoiceCode)
        {
            //默认电子发票信息
            cellPhone = string.Empty;
            email = string.Empty;
            invoiceName = string.Empty;
            invoiceCode = string.Empty;
            var invoice = ShopApplication.GetInvoiceTitleInfo(userid, InvoiceType.OrdinaryInvoices);
            if(invoice != null)
            {
                invoiceName = invoice.Name;
                invoiceCode = invoice.Code;
            }

            var invoiceTitle = ShopApplication.GetInvoiceTitleInfo(userid, InvoiceType.ElectronicInvoice);
            if (invoiceTitle != null)
            {
                cellPhone = invoiceTitle.CellPhone;
                email = invoiceTitle.Email;
            }
            else
            {
                var bindPhone = MessageApplication.GetDestination(userid, "Mall.Plugin.Message.SMS", Entities.MemberContactInfo.UserTypes.General);
                if (!string.IsNullOrEmpty(bindPhone))
                    cellPhone = bindPhone;
                var bindEmail = MessageApplication.GetDestination(userid, "Mall.Plugin.Message.Email", Entities.MemberContactInfo.UserTypes.General);
                if (!string.IsNullOrEmpty(bindEmail))
                    email = bindEmail;
            }
            //默认增值税发票信息
            var vatInvoice = ShopApplication.GetInvoiceTitleInfo(userid, InvoiceType.VATInvoice);
            if (vatInvoice == null)
            {
                vatInvoice = new InvoiceTitleInfo();
                var defaultAddress = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
                if(defaultAddress != null)
                {
                    vatInvoice.RealName = defaultAddress.ShipTo;
                    vatInvoice.CellPhone = defaultAddress.Phone;
                    vatInvoice.RegionID = defaultAddress.RegionId;
                    vatInvoice.Address = defaultAddress.Address + " " + defaultAddress.AddressDetail;
                }
            }                
            vatInvoice.RegionFullName = RegionApplication.GetFullName(vatInvoice.RegionID);
            return vatInvoice;
        }

        /// <summary>
        /// 计算增值税发票发货时间
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        static string GetInvoiceDays(long shopid)
        {
            string days = "0";
            var shopConfig = ShopApplication.GetShopInvoiceConfig(shopid);
            if(shopConfig!= null && shopConfig.IsVatInvoice)
            {
                var platDay = SiteSettingApplication.SiteSettings.SalesReturnTimeout;
                var shopDay = shopConfig.VatInvoiceDay + platDay;

                if (platDay < shopDay)
                    days = platDay + "-" + shopDay;
                else if (platDay == shopDay)
                    days = shopDay + "";
            }
            return days;
        }

        static void SetOrderProductsInfo(OrderSubmitModel model, string skuIds, string counts, long userid, string collIds = null, IEnumerable<string[]> CouponIdsStr = null, sbyte? productType = 0)
        {
            var address = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
            int cityId = 0;
            if (address != null)
            {
                // cityId = RegionApplication.GetRegion(address.RegionId, Region.RegionLevel.City).Id;
                cityId = address.RegionId;
            }

            if (cityId <= 0)
            {
                //跳转填写收货地址
            }
            IEnumerable<long> CollPidArr = null;
            if (string.IsNullOrEmpty(skuIds))
                throw new MallException("sku不能为空");
            var skuIdsArr = skuIds.Split(',');
            var pCountsArr = counts.TrimEnd(',').Split(',').Select(t => int.Parse(t));
            if (!string.IsNullOrEmpty(collIds))
            {
                CollPidArr = collIds.TrimEnd(',').Split(',').Select(t => long.Parse(t));
            }
            var productService = _iProductService;
            int index = 0;
            var skuCount = skuIdsArr.Length;//有多少个SKU就是多少个商品
            var skus = ProductManagerApplication.GetSKUInfos(skuIdsArr);
            var products = ProductManagerApplication.GetProductByIds(skus.Select(p => p.ProductId));
            var listProducts = skuIdsArr.Select(sku =>
            {
                var skuitem = skus.Where(p => p.Id == sku).FirstOrDefault();
                var product = products.FirstOrDefault(p => p.Id == skuitem.ProductId);
                var count = pCountsArr.ElementAt(index);
                var collpid = CollPidArr != null ? CollPidArr.ElementAt(index) : 0;
                index++;
                var skuprice = GetSalePrice(skuitem.ProductId, skuitem, collpid, skuCount, count, userid);
                return new CartItemModel()
                {
                    skuId = skuitem.Id,
                    id = product.Id,
                    imgUrl = product.GetImage(ImageSize.Size_50),
                    name = product.ProductName,
                    shopId = product.ShopId,
                    price = skuprice,
                    count = count,
                    productCode = product.ProductCode,
                    collpid = collpid,
                    FreightTemplateId = product.FreightTemplateId,
                    IsOpenLadder = product.IsOpenLadder
                };
            }).ToList();
            var shopList = listProducts.GroupBy(a => a.shopId);
            List<ShopCartItemModel> list = new List<ShopCartItemModel>();
            var baseCoupon = Service.GetOrdersCoupons(userid, CouponIdsStr);
            foreach (var shopcartItem in shopList)
            {
                IEnumerable<long> productIds = shopcartItem.Select(r => r.id);
                IEnumerable<int> productCounts = shopcartItem.Select(r => r.count);

                ShopCartItemModel item = new ShopCartItemModel();
                item.shopId = shopcartItem.Key;
                item.CartItemModels = listProducts.Where(a => a.shopId == item.shopId).ToList();
                var shop = ShopApplication.GetShopInfo(item.shopId);
                item.ShopName = shop.ShopName;
                item.FreeFreight = shop.FreeFreight;
                if (productType == 0 && cityId > 0)
                {
                    #region 指定地区包邮
                    IEnumerable<long> pIds;
                    IEnumerable<int> pCounts;
                    FreeShipping(cityId, shopcartItem, out pIds, out pCounts);
                    #endregion
                    if (pIds != null && pIds.Count() > 0 && pCounts != null && pCounts.Count() > 0)
                    {
                        item.Freight = _iProductService.GetFreight(pIds, pCounts, cityId);
                    }
                }
                //计算满额减的金额
                item.FullDiscount = GetShopFullDiscount(item.CartItemModels);
                //默认优惠券
                if (baseCoupon != null)
                {
                    var couponAmount = item.ShopTotalWithoutFreight;
                    var coupon = baseCoupon.Where(a => a.ShopId == item.shopId).FirstOrDefault();
                    if (coupon != null && coupon.Type == 0)
                    {
                        var uc = (coupon.Coupon as Entities.CouponRecordInfo);
                        var bc = CouponApplication.Get(uc.CouponId);
                        if (bc.UseArea == 1)
                        {
                            var couponProducts = CouponApplication.GetCouponProductsByCouponId(uc.CouponId).Select(p => p.ProductId).ToList();
                            decimal coupontotal = 0;
                            foreach (var p in item.CartItemModels)
                            {
                                if (couponProducts.Contains(p.id))
                                    coupontotal += p.price * p.count - p.fullDiscount;
                            }
                            couponAmount = coupontotal;
                        }
                    }
                    item.OneCoupons = GetSelectedCoupon(item.ShopTotalWithoutFreight == couponAmount ? item.ShopTotalWithoutFreight : couponAmount, userid, item.shopId, baseCoupon);
                }
                else
                {
                    item.OneCoupons = GetDefaultCoupon(item.shopId, userid, item.ShopTotalWithoutFreight, item.CartItemModels);
                }
                //优惠券
                item.BaseCoupons = GetBaseCoupon(item.shopId, userid, item.ShopTotalWithoutFreight, item.CartItemModels);
                var orderItems = GetOrderItems(item);

                item.freightProductGroup = orderItems.OrderBy(e => e.FreightTemplateId).ToList();
                //发票
                item.IsInvoice = ShopApplication.GetShopInvoiceConfig(item.shopId)?.IsInvoice ?? false;
                item.invoiceTpyes = ShopApplication.GetInvoiceTypes(item.shopId);
                item.InvoiceDay = GetInvoiceDays(item.shopId);
                list.Add(item);
            }
            model.products = list;
            model.totalAmount = listProducts.Sum(item => decimal.Round(item.price, 2, MidpointRounding.AwayFromZero) * item.count) - list.Sum(a => a.FullDiscount);
            model.Freight = list.Sum(a => a.Freight);
        }
        /// <summary>
        /// 获取售价
        /// <para>已计算会员折</para>
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="sku"></param>
        /// <param name="collid"></param>
        /// <param name="Count"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        static decimal GetSalePrice(long productId, SKUInfo sku, long? collid, int Count, int quantity, long? userId = null)
        {
            var price = sku.SalePrice;
            var product = ProductManagerApplication.GetProduct(sku.ProductId);
            #region 阶梯价--张宇枫
            if (product.IsOpenLadder)
            {
                //商品批量销售价
                price = ProductManagerApplication.GetProductLadderPrice(product.Id, quantity);
            }
            #endregion

            #region 会员折
            decimal discount = 1;  //默认无折扣
            if (userId.HasValue && userId > 0)
            {
                var user = MemberApplication.GetMember(userId.Value);
                var shopInfo = ShopApplication.GetShop(product.ShopId);
                if (shopInfo != null && shopInfo.IsSelf)
                {
                    discount = user.MemberDiscount;
                }
            }
            price = discount * price; //折扣价
            #endregion

            if (collid.HasValue && collid.Value != 0 && Count > 1)//组合购大于一个商品
            {
                var collsku = CollocationApplication.GetColloSku(collid.Value, sku.Id);
                if (collsku != null)
                {
                    price = collsku.Price;
                }
                //获取组合购的价格
            }
            else if (Count == 1) //只有一个商品可能是限时购
            {
                var limit = _iLimitTimeBuyService.GetDetail(sku.Id);

                if (limit != null)
                {
                    price = (decimal)limit.Price;
                }
            }
            return price;
        }

        static IEnumerable<OrderSubmitItemModel> GetOrderItems(ShopCartItemModel item)
        {
            var productService = _iProductService;
            var iTypeService = _iTypeService;
            var orderItems = item.CartItemModels.Select(r =>
            {
                var productcode = r.productCode;
                var skuinfo = productService.GetSku(r.skuId);
                if (skuinfo != null)
                {
                    if (!string.IsNullOrWhiteSpace(skuinfo.Sku))
                    {
                        productcode = skuinfo.Sku;
                    }
                }
                var product = productService.GetProduct(skuinfo.ProductId);
                //杨振国加的保证金标识，这里请重构
                var cashDeposit = CashDepositsApplication.GetCashDepositsObligation(skuinfo.ProductId);
                Entities.TypeInfo typeInfo = iTypeService.GetTypeByProductId(skuinfo.ProductId);
                string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

                string skuDetails = "";
                if (!string.IsNullOrWhiteSpace(skuinfo.Size))
                {
                    if (!string.IsNullOrWhiteSpace(skuDetails))
                    {
                        skuDetails += "、";
                    }
                    skuDetails += skuinfo.Size;
                }
                if (!string.IsNullOrWhiteSpace(skuinfo.Color))
                {
                    if (!string.IsNullOrWhiteSpace(skuDetails))
                    {
                        skuDetails += "、";
                    }
                    skuDetails += skuinfo.Color;
                }
                if (!string.IsNullOrWhiteSpace(skuinfo.Version))
                {
                    if (!string.IsNullOrWhiteSpace(skuDetails))
                    {
                        skuDetails += "、";
                    }
                    skuDetails += skuinfo.Version;
                }
                return new OrderSubmitItemModel
                {
                    id = r.id,
                    ProductId = product.Id,
                    FreightTemplateId = product != null ? product.FreightTemplateId : 0,
                    price = r.price,
                    count = r.count,
                    skuId = r.skuId,
                    name = r.name,
                    productCode = productcode,
                    imgUrl = r.imgUrl,
                    //杨振国加的保证金标识，这里请重构
                    sevenDayNoReasonReturn = cashDeposit.IsSevenDayNoReasonReturn,
                    timelyShip = cashDeposit.IsTimelyShip,
                    customerSecurity = cashDeposit.IsCustomerSecurity,
                    skuColor = skuinfo.Color,
                    skuSize = skuinfo.Size,
                    skuVersion = skuinfo.Version,
                    colorAlias = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias,
                    sizeAlias = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias,
                    versionAlias = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.VersionAlias : versionAlias,
                    skuDetails = skuDetails,
                    collpid = r.collpid,
                    isOpenLadder = product.IsOpenLadder
                };
            });

            return orderItems;
        }

        static Entities.ShippingAddressInfo GetShippingAddress(long? regionId, long userid)
        {
            if (regionId != null)
            {
                return ShippingAddressApplication.GetUserShippingAddress((long)regionId);
            }
            else
                return ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(userid);
        }

        /// <summary>
        /// 订单提交页面，需要展示的数据
        /// </summary>
        static List<CartItemModel> GenerateCartItem(IEnumerable<ShoppingCartItem> cartItems)
        {
            var productService = _iProductService;
            var groupCartByProduct = cartItems.GroupBy(i => i.ProductId).ToList();//按照商品分组
            var products = ProductManagerApplication.GetProducts(cartItems.Select(p => p.ProductId));
            var skus = ProductManagerApplication.GetSKUByProducts(products.Select(p => p.Id).ToList());
            var types = TypeApplication.GetTypes(products.Select(p => p.TypeId).ToList());
            var shops = ShopApplication.GetShops(products.Select(p => p.ShopId));
            return cartItems.Select(item =>
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                var sku = skus.FirstOrDefault(p => p.Id == item.SkuId);
                var typeInfo = types.FirstOrDefault(p => p.Id == product.TypeId);
                var shop = shops.FirstOrDefault(p => p.Id == product.ShopId);

                string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                string shopBranchName = "";
                if ( item.ShopBranchId > 0)
                {
                    var shopBranchInfo = _iShopBranchService.GetShopBranchById(item.ShopBranchId);
                    if (shopBranchInfo != null)
                        shopBranchName = shopBranchInfo.ShopBranchName;
                }
                #region 阶梯价--张宇枫
                //默认SKU销售价
                var price = sku.SalePrice;
                if (product.IsOpenLadder)
                {
                    //获取商品总数量，不分规格
                    var quantity =
                        groupCartByProduct.Where(i => i.Key == item.ProductId)
                            .ToList()
                            .Sum(cartitem => cartitem.Sum(i => i.Quantity));
                    //商品批量销售价
                    price = ProductManagerApplication.GetProductLadderPrice(item.ProductId, quantity);
                }
                #endregion
                string skuDetails = "";
                if (!string.IsNullOrWhiteSpace(sku.Size))
                {
                    if (!string.IsNullOrWhiteSpace(skuDetails))
                    {
                        skuDetails += "、";
                    }
                    skuDetails += sku.Size;
                }
                if (!string.IsNullOrWhiteSpace(sku.Color))
                {
                    if (!string.IsNullOrWhiteSpace(skuDetails))
                    {
                        skuDetails += "、";
                    }
                    skuDetails += sku.Color;
                }
                if (!string.IsNullOrWhiteSpace(sku.Version))
                {
                    if (!string.IsNullOrWhiteSpace(skuDetails))
                    {
                        skuDetails += "、";
                    }
                    skuDetails += sku.Version;
                }
                return new CartItemModel()
                {
                    skuId = item.SkuId,
                    id = product.Id,
                    imgUrl = Core.MallIO.GetRomoteProductSizeImage(product.RelativePath, 1, (int)ImageSize.Size_100),
                    name = product.ProductName,
                    price = price,
                    shopId = product.ShopId,
                    count = item.Quantity,
                    productCode = product.ProductCode,
                    color = sku.Color,
                    size = sku.Size,
                    version = sku.Version,
                    skuDetails = skuDetails,
                    IsSelf = shop.IsSelf,
                    ColorAlias = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias,
                    SizeAlias = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias,
                    VersionAlias = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.SizeAlias : versionAlias,
                    ShopBranchId = item.ShopBranchId,
                    ShopBranchName = shopBranchName,
                    IsOpenLadder = product.IsOpenLadder,
                    FreightTemplateId = product.FreightTemplateId
                };
            }).ToList();

        }


        /// <summary>
        /// 获取购物车中的商品
        /// </summary>
        /// <returns></returns>
        static Mall.Entities.ShoppingCartInfo GetCart(long memberId, string cartInfo)
        {
            Mall.Entities.ShoppingCartInfo shoppingCartInfo;
            if (memberId > 0)//已经登录，系统从服务器读取购物车信息，否则从Cookie获取购物车信息
                shoppingCartInfo = CartApplication.GetCart(memberId);
            else
            {
                shoppingCartInfo = new Mall.Entities.ShoppingCartInfo();

                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    var cartInfoItems = new List<Mall.Entities.ShoppingCartItem>();
                    int i = 0;
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        cartInfoItems.Add(new Mall.Entities.ShoppingCartItem() { ProductId = long.Parse(cartItemParts[0].Split('_')[0]), SkuId = cartItemParts[0], Quantity = int.Parse(cartItemParts[1]) });
                    }
                    shoppingCartInfo.Items = cartInfoItems;
                }
            }
            return shoppingCartInfo;
        }
        /// <summary>
        /// 订单提交页面，需要展示的数据
        /// </summary>
        static List<CartItemModel> GenerateCartItem(IEnumerable<string> skuIds, IEnumerable<int> counts, long shopBranchId = 0)
        {
            int i = 0;
            var skus = ProductManagerApplication.GetSKUs(skuIds);
            var products = ProductManagerApplication.GetProducts(skus.Select(p => p.ProductId));
            var types = TypeApplication.GetTypes(products.Select(p => p.TypeId).ToList());
            var shops = ShopApplication.GetShops(products.Select(p => p.ShopId));
            var shopBranchInfo = ShopBranchApplication.GetShopBranchInfoById(shopBranchId);
            return skuIds.Select(item =>
            {
                var sku = skus.FirstOrDefault(p => p.Id == item);
                var product = products.FirstOrDefault(p => p.Id == sku.ProductId);
                var shop = shops.FirstOrDefault(p => p.Id == product.ShopId);
                var count = counts.ElementAt(i++);
                var ltmbuy = _iLimitTimeBuyService.GetLimitTimeMarketItemByProductId(product.Id);
                //默认SKU销售价
                var price = sku.SalePrice;
                if (ltmbuy != null)
                {
                    if (count > ltmbuy.LimitCountOfThePeople)
                        throw new MallException("超过最大限购数量：" + ltmbuy.LimitCountOfThePeople.ToString() + "");
                }
                else
                {
                    #region 阶梯价--张宇枫
                    if (product.IsOpenLadder)
                    {
                        //商品批量销售价
                        price = ProductManagerApplication.GetProductLadderPrice(product.Id, count);
                    }
                    #endregion
                }
                if (sku.Stock < count)
                {
                    //throw new MallException("库存不足");
                }
                var typeInfo = types.FirstOrDefault(p => p.Id == product.TypeId);
                string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

                return new CartItemModel()
                {
                    skuId = item,
                    id = product.Id,
                    ProductType = product.ProductType,
                    imgUrl = Core.MallIO.GetRomoteProductSizeImage(product.RelativePath, 1, (int)(ImageSize.Size_100)),
                    name = product.ProductName,
                    shopId = product.ShopId,
                    price = ltmbuy == null ? price : (decimal)_iLimitTimeBuyService.GetDetail(item).Price,
                    count = count,
                    productCode = product.ProductCode,
                    unit = product.MeasureUnit,
                    size = sku.Size,
                    color = sku.Color,
                    version = sku.Version,
                    IsSelf = shop.IsSelf,
                    ColorAlias = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias,
                    SizeAlias = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias,
                    VersionAlias = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.VersionAlias : versionAlias,
                    IsLimit = ltmbuy != null,
                    FreightTemplateId = product.FreightTemplateId,
                    IsOpenLadder = product.IsOpenLadder,
                    ShopBranchName = shopBranchInfo != null ? shopBranchInfo.ShopBranchName : "",
                    ShopBranchId = shopBranchInfo != null ? shopBranchInfo.Id : 0
                };
            }).ToList();
        }

        /// <summary>
        /// 订单提交页面，拼团数据组装
        /// </summary>
        /// <param name="actionId">活动编号</param>
        /// <param name="skuId">规格</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        static List<CartItemModel> GenerateGroupItem(long actionId, string skuId, int count, long? groupId = null)
        {
            bool isnewgroup = false;
            if (groupId > 0)
            {
                isnewgroup = true;
            }
            List<CartItemModel> result = new List<CartItemModel>();
            var actobj = FightGroupApplication.GetActive(actionId);

            var sku = actobj.ActiveItems.FirstOrDefault(d => d.SkuId == skuId);
            if (sku == null)
            {
                throw new MallException("错误的规格信息");
            }
            if (count > actobj.LimitQuantity)
            {
                throw new MallException("超过最大限购数量：" + actobj.LimitQuantity.ToString() + "");
            }
            if (sku.ActiveStock < count)
            {
                //throw new MallException("库存不足");
            }
            if (isnewgroup)
            {
                if (actobj.ActiveStatus != CommonModel.FightGroupActiveStatus.Ongoing)
                {
                    throw new MallException("拼团活动已结束，不可以开团");
                }
            }
            Entities.TypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetTypeByProductId(sku.ProductId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            var product = ProductManagerApplication.GetProduct(sku.ProductId);
            var data = new CartItemModel()
            {
                skuId = skuId,
                id = sku.ProductId,
                imgUrl = MallIO.GetRomoteProductSizeImage(actobj.ProductImgPath, 1, (int)ImageSize.Size_100),
                name = actobj.ProductName,
                shopId = actobj.ShopId,
                price = sku.ActivePrice,
                count = count,
                productCode = actobj.ProductCode,
                unit = actobj.MeasureUnit,
                size = sku.Size,
                color = sku.Color,
                version = sku.Version,
                IsSelf = ShopApplication.IsSelfShop(actobj.ShopId),
                ColorAlias = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias,
                SizeAlias = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias,
                VersionAlias = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.VersionAlias : versionAlias,
                FreightTemplateId = actobj.FreightTemplateId,
                IsOpenLadder = product.IsOpenLadder
            };
            result.Add(data);

            return result;
        }

        /// <summary>
        /// 获取用户所有可用的优惠券
        /// </summary>
        static List<BaseCoupon> GetBaseCoupon(long shopId, long userId, decimal totalPrice, List<CartItemModel> cartItems, bool isPc = false)
        {
            var userCoupons = CouponApplication.GetUserCoupon(shopId, userId, totalPrice);
            var userBonus = ShopBonusApplication.GetDetailToUse(shopId, userId, totalPrice);
            var cous = CouponApplication.GetCouponInfo(userCoupons.Select(p => p.CouponId));
            List<BaseCoupon> coupons = new List<BaseCoupon>();
            foreach (var coupon in userCoupons)
            {
                var cou = cous.FirstOrDefault(p => p.Id == coupon.CouponId);
                if (cou.UseArea == 1)
                {
                    var pids = CouponApplication.GetCouponProductsByCouponId(coupon.CouponId).Select(p => p.ProductId).ToList();
                    decimal totalAmount = 0;
                    var canUse = false;
                    foreach (var cartitem in cartItems)
                    {
                        if (pids.Contains(cartitem.id))
                        {
                            totalAmount += cartitem.count * cartitem.price;
                            canUse = true;
                        }
                    }

                    if (canUse && totalAmount >= cou.OrderAmount)
                    {
                        BaseCoupon c = new BaseCoupon();
                        var info = CouponApplication.GetCouponInfo(coupon.CouponId);
                        c.BaseEndTime = info.EndTime;
                        c.BaseId = coupon.Id;
                        c.BaseName = info.CouponName;

                        if (isPc)
                            c.BasePrice = info.Price > totalAmount ? totalAmount : info.Price;
                        else
                            c.BasePrice = info.Price;

                        c.BaseShopId = info.ShopId;
                        c.BaseShopName = info.ShopName;
                        c.BaseType = CommonModel.CouponType.Coupon;
                        c.OrderAmount = info.OrderAmount;
                        c.UseArea = info.UseArea;
                        c.Remark = info.Remark;
                        c.StartDateStr = info.StartTime.ToString("yyyy.MM.dd");
                        c.EndDateStr = info.EndTime.ToString("yyyy.MM.dd");
                        coupons.Add(c);
                    }
                }
                else
                {
                    BaseCoupon c = new BaseCoupon();
                    var info = CouponApplication.GetCouponInfo(coupon.CouponId);
                    c.BaseEndTime = info.EndTime;
                    c.BaseId = coupon.Id;
                    c.BaseName = info.CouponName;
                    c.BasePrice = info.Price;
                    c.BaseShopId = info.ShopId;
                    c.BaseShopName = info.ShopName;
                    c.BaseType = CommonModel.CouponType.Coupon;
                    c.OrderAmount = info.OrderAmount;
                    c.UseArea = info.UseArea;
                    c.Remark = info.Remark;
                    c.StartDateStr = info.StartTime.ToString("yyyy.MM.dd");
                    c.EndDateStr = info.EndTime.ToString("yyyy.MM.dd");
                    coupons.Add(c);
                }
            }
            foreach (var coupon in userBonus)
            {
                BaseCoupon c = new BaseCoupon();
                var service = GetService<IShopBonusService>();
                var grant = service.GetGrant(coupon.BonusGrantId);
                var bonus = service.GetShopBonus(grant.ShopBonusId);
                var shop = ShopApplication.GetShop(bonus.ShopId);

                c.BaseEndTime = bonus.BonusDateEnd;
                c.BaseId = coupon.Id;
                c.BaseName = bonus.Name;
                c.BasePrice = coupon.Price;
                c.BaseShopId = shop.Id;
                c.BaseShopName = shop.ShopName;
                c.BaseType = CommonModel.CouponType.ShopBonus;
                c.OrderAmount = bonus.UsrStatePrice;
                c.StartDateStr = bonus.BonusDateStart.ToString("yyyy.MM.dd");
                c.EndDateStr = bonus.BonusDateEnd.ToString("yyyy.MM.dd");
                coupons.Add(c);
            }
            return coupons;
        }

        /// <summary>
        /// 支付完生成红包
        /// </summary>
        private static Dictionary<long, ShopBonusInfo> GenerateBonus(IEnumerable<long> orderIds, string urlHost)
        {
            Dictionary<long, ShopBonusInfo> bonusGrantIds = new Dictionary<long, ShopBonusInfo>();
            string url = Core.Helper.WebHelper.GetScheme() + "://" + urlHost + "/m-weixin/shopbonus/index/";
            var buyOrders = Service.GetOrders(orderIds);
            foreach (var o in buyOrders)
            {
                var shopBonus = ShopBonusApplication.GetByShopId(o.ShopId);
                if (shopBonus == null)
                {
                    continue;
                }
                if (shopBonus.GrantPrice <= o.OrderTotalAmount)
                {
                    long grantid = ShopBonusApplication.GenerateBonusDetail(shopBonus, o.Id, url);
                    bonusGrantIds.Add(grantid, shopBonus);
                }
            }
            return bonusGrantIds;
        }

        /// <summary>
        /// 更改限时购销售量
        /// </summary>
        private static void IncreaseSaleCount(List<long> orderid)
        {
            if (orderid.Count == 1)
            {
                _iLimitTimeBuyService.IncreaseSaleCount(orderid);
            }
        }

        // 平台确认订单支付
        public static void PlatformConfirmOrderPay(long orderId, string payRemark, string managerName)
        {
            Service.PlatformConfirmOrderPay(orderId, payRemark, managerName);
        }

        /// <summary>
        /// 处理会员订单类别
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        public static void DealDithOrderCategoryByUserId(long orderId, long userId)
        {
            var orderItem = GetOrderItemsByOrderId(orderId);
            var productIds = orderItem.Select(p => p.ProductId);
            var product = ProductManagerApplication.GetProductsByIds(productIds);
            foreach (var item in product)
            {
                var categoryId = long.Parse(item.CategoryPath.Split('|')[0]);
                OrderAndSaleStatisticsApplication.SynchronizeMemberBuyCategory(categoryId, userId);
            }
        }
        /// <summary>
        /// 根据订单ID获取订单商品明细，包括商品店铺信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<OrderDetailView> GetOrderDetailViews(IEnumerable<long> ids)
        {
            var list = Service.GetOrders(ids).Map<List<FullOrder>>();
            List<OrderDetailView> orderDetails = new List<OrderDetailView>();
            var orderItems = GetOrderItemsByOrderId(list.Select(p => p.Id));//订单明细
            var shops = ShopApplication.GetShops(orderItems.Select(e => e.ShopId).ToList());//店铺信息
            var shopbranchs = ShopBranchApplication.GetShopBranchByIds(list.Where(d =>  d.ShopBranchId > 0).Select(d => d.ShopBranchId).ToList());
            var vShops = VshopApplication.GetVShopsByShopIds(orderItems.Select(e => e.ShopId));//微店信息
            foreach (var orderItem in orderItems)
            {
                //完善图片地址
                string s_pimg = orderItem.ThumbnailsUrl;
                orderItem.ThumbnailsUrl = MallIO.GetRomoteProductSizeImage(s_pimg, 1, (int)ImageSize.Size_500);
                orderItem.ProductImage = MallIO.GetRomoteProductSizeImage(s_pimg, 1, (int)ImageSize.Size_100);
                orderItem.ShareImage = MallIO.GetRomoteProductSizeImage(s_pimg, 1, (int)ImageSize.Size_50);
            }
            foreach (var item in list)
            {
                OrderDetailView detail = new OrderDetailView() { };
                var vshop = vShops.FirstOrDefault(e => e.ShopId == item.ShopId);
                long vshopId = 0;
                if (vshop != null)
                {
                    vshopId = vshop.Id;
                }
                detail.Detail = new OrderDetail
                {
                    ShopId = item.ShopId,
                    ShopName = shops.FirstOrDefault(e => e.Id == item.ShopId).ShopName,
                    VShopId = vshopId,
                    OrderItems = orderItems.Where(p => p.OrderId == item.Id).ToList()
                };
                if (item.ShopBranchId > 0)
                {
                    var sb = shopbranchs.FirstOrDefault(d => d.Id == item.ShopBranchId);
                    if (sb != null)
                    {
                        detail.Detail.ShopBranchName = sb.ShopBranchName;
                        detail.Detail.ShopBranchId = sb.Id;
                    }
                }
                detail.Order = item.Map<OrderInfo>();
                orderDetails.Add(detail);
            }

            return orderDetails;
        }
        #endregion
        #region 商家手动分配门店
        /// <summary>
        /// 分配门店时更新商家、门店库存
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="quantity"></param>
        public static void AllotStoreUpdateStock(List<string> skuIds, List<int> counts, long shopBranchId)
        {
            Service.AllotStoreUpdateStock(skuIds, counts, shopBranchId);
        }
        /// <summary>
        /// 分配门店订单到新门店
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="newShopBranchId"></param>
        /// <param name="oldShopBranchId"></param>
        public static void AllotStoreUpdateStockToNewShopBranch(List<string> skuIds, List<int> counts, long newShopBranchId, long oldShopBranchId)
        {
            Service.AllotStoreUpdateStockToNewShopBranch(skuIds, counts, newShopBranchId, oldShopBranchId);
        }
        /// <summary>
        /// 分配门店订单回到商家
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="shopId"></param>
        public static void AllotStoreUpdateStockToShop(List<string> skuIds, List<int> counts, long shopBranchId)
        {
            Service.AllotStoreUpdateStockToShop(skuIds, counts, shopBranchId);
        }
        /// <summary>
        /// 更新订单所属门店
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopBranchId"></param>
        public static void UpdateOrderShopBranch(long orderId, long shopBranchId)
        {
            Service.UpdateOrderShopBranch(orderId, shopBranchId);
        }

        #endregion


        public static int GetFightGroupOrderByUser(long userId)
        {
            return Service.GetFightGroupOrderCountByUser(userId);
        }
        /// <summary>
        /// 获取待评价订单数量
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static int GetWaitingForComments(OrderQuery query)
        {
            return Service.GetWaitingForComments(query);
        }
        public static QueryPageModel<OrderInfo> GetOrdersByQuery(OrderQuery query)
        {
            return Service.GetOrders(query);
        }
        public static void CalculateOrderItemRefund(long orderId, bool isCompel = false)
        {
            Service.CalculateOrderItemRefund(orderId, isCompel);
        }
        public static OrderInfo GetOrder(long orderId, long userId)
        {
            return Service.GetOrder(orderId, userId);
        }
        public static bool IsRefundTimeOut(long orderId)
        {
            return Service.IsRefundTimeOut(orderId);
        }
        public static void MembeConfirmOrder(long orderId, string memberName)
        {
            Service.MembeConfirmOrder(orderId, memberName);
        }
        public static void MemberCloseOrder(long orderId, string memberName)
        {
            Service.MemberCloseOrder(orderId, memberName);
        }

        public static OrderItemInfo GetOrderItem(long orderItemId)
        {
            return Service.GetOrderItem(orderItemId);
        }
        /// <summary>
		/// 保存支付订单信息，生成支付订单
		/// </summary>
		/// <param name="model"></param>
		/// <param name="platform"></param>
		/// <returns></returns>
		public static long SaveOrderPayInfo(IEnumerable<OrderPay> model, PlatformType platform)
        {
            //  List<OrderPayInfo> ordpays = Mapper.Map<List<OrderPay>, List<OrderPayInfo>>(model.ToList());

            List<OrderPayInfo> ordpays = model.ToList().Map< List<OrderPayInfo>>();

            return Service.SaveOrderPayInfo(ordpays, platform);
        }
        /// <summary>
        /// 取最近time分钟内的满足打印的订单数据
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static List<long> GetOrderIdsByLatestTime(int time, long shopBranchId, long shopId)
        {
            return Service.GetOrderIdsByLatestTime(time, shopBranchId, shopId);
        }
        /// <summary>
        /// 是否可以售后
        /// </summary>
        /// <param name="data"></param>
        /// <param name="refundStatus">售后状态,null表示方法自查</param>
        /// <param name="itemId">订单项编号,null表示订单退款</param>
        /// <returns></returns>
        public static bool CanRefund(Order data, int? refundStatus = null, long? itemId = null)
        {
            bool result = false;
            if (itemId == null || itemId <= 0)
            {
                if (refundStatus == null)
                {
                    OrderRefundInfo _ordrefobj = _iRefundService.GetOrderRefundByOrderId(data.Id);
                    if (data.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery && data.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                    {
                        _ordrefobj = null;
                    }
                    refundStatus = (_ordrefobj == null ? null : (int?)_ordrefobj.SellerAuditStatus);
                    refundStatus = (refundStatus > 4 ? (int?)_ordrefobj.ManagerConfirmStatus : refundStatus);
                }

                result = (data.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || data.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                    && !data.RefundStats.HasValue && data.PaymentType != Entities.OrderInfo.PaymentTypes.CashOnDelivery && data.PaymentType != Entities.OrderInfo.PaymentTypes.None
                    && (data.FightGroupCanRefund == null || data.FightGroupCanRefund == true);
                result = result && (refundStatus.GetValueOrDefault().Equals(0) || refundStatus.GetValueOrDefault().Equals(4));
                result = result && (data.TotalAmount + data.IntegralDiscount > 0);
            }
            else
            {
                if (data.OrderType == OrderInfo.OrderTypes.Virtual)
                {
                    var itemInfo = GetOrderItems(data.Id).FirstOrDefault();
                    if (itemInfo != null)
                    {
                        var virtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(itemInfo.ProductId);
                        if (virtualProductInfo != null)
                        {
                            //如果该商品支持退款，而订单状态为待消费，则可退款
                            if (virtualProductInfo.SupportRefundType == (sbyte)ProductInfo.SupportVirtualRefundType.SupportAnyTime)
                            {
                                if (data.OrderStatus == OrderInfo.OrderOperateStatus.WaitVerification)
                                {
                                    result = true;
                                }
                            }
                            else if (virtualProductInfo.SupportRefundType == (sbyte)ProductInfo.SupportVirtualRefundType.SupportValidity)
                            {
                                if (virtualProductInfo.EndDate.Value > DateTime.Now)
                                {
                                    if (data.OrderStatus == OrderInfo.OrderOperateStatus.WaitVerification)
                                    {
                                        result = true;
                                    }
                                }
                            }
                            else if (virtualProductInfo.SupportRefundType == (sbyte)ProductInfo.SupportVirtualRefundType.NonSupport)
                            {
                                result = false;
                            }
                            if (result)
                            {
                                var orderVerificationCodes = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(new List<long>() { data.Id });
                                long num = orderVerificationCodes.Where(a => a.Status == OrderInfo.VerificationCodeStatus.WaitVerification).Count();
                                if (num > 0)
                                {
                                    result = true;
                                }
                                else
                                {
                                    result = false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    result = RefundApplication.CanApplyRefund(data.Id, itemId.Value, false);
                    result = result && !IsRefundTimeOut(data.Id);

                    if (data.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || data.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                    {
                        result = false;  //待收货 待自提只可以订单退款
                    }
                    if (data.PaymentType == Entities.OrderInfo.PaymentTypes.CashOnDelivery && data.OrderStatus != OrderInfo.OrderOperateStatus.Finish)
                    {
                        result = false;  //货到付款在订单未完成前不可以售后
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 是否可以售后
        /// </summary>
        /// <param name="data"></param>
        /// <param name="refundStatus">售后状态,null表示方法自查</param>
        /// <param name="itemId">订单项编号,null表示订单退款</param>
        /// <returns></returns>
        public static bool CanRefund(OrderInfo data, int? refundStatus = null, long? itemId = null)
        {
            //var cdata = AutoMapper.Mapper.Map<DTO.Order>(data);
            var cdata = data.Map<DTO.Order>();


            return CanRefund(cdata, refundStatus, itemId);
        }

        public static List<OrderInfo> GetUserOrders(long user, int top)
        {
            return Service.GetTopOrders(top, user);
        }


        public static List<OrderComplaintInfo> GetOrderComplaintByOrders(List<long> orders)
        {
            return Service.GetOrderComplaintByOrders(orders);
        }
        public static int GetOrderTotalProductCount(long order)
        {
            return Service.GetOrderTotalProductCount(order);
        }

        public static List<OrderCommentInfo> GetOrderComment(long order)
        {
            return GetService<ITradeCommentService>().GetOrderCommentsByOrder(order);
        }

        /// <summary>
        /// 虚拟订单用户信息项
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static List<VirtualOrderItemInfo> GetVirtualOrderItemInfosByOrderId(long orderId)
        {
            return Service.GetVirtualOrderItemInfosByOrderId(orderId);
        }

        /// <summary>
        /// 订单核销码
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static List<OrderVerificationCodeInfo> GetOrderVerificationCodeInfosByOrderIds(List<long> orderIds)
        {
            return Service.GetOrderVerificationCodeInfosByOrderIds(orderIds);
        }
        /// <summary>
        /// 根据核销码获取唯一条核销码信息
        /// </summary>
        /// <param name="verificationCode"></param>
        /// <returns></returns>
        public static OrderVerificationCodeInfo GetOrderVerificationCodeInfoByCode(string verificationCode)
        {
            return Service.GetOrderVerificationCodeInfoByCode(verificationCode);
        }
        public static List<OrderVerificationCodeInfo> GetOrderVerificationCodeInfoByCodes(List<string> verificationCodes)
        {
            return Service.GetOrderVerificationCodeInfoByCodes(verificationCodes);
        }
        /// <summary>
        /// 更新订单核销码状态
        /// </summary>
        /// <param name="verficationCodes"></param>
        /// <returns></returns>
        public static bool UpdateOrderVerificationCodeStatusByCodes(List<string> verficationCodes, long orderId, OrderInfo.VerificationCodeStatus status, DateTime? verificationTime, string verificationUser = "")
        {
            return Service.UpdateOrderVerificationCodeStatusByCodes(verficationCodes, orderId, status, verificationTime, verificationUser);
        }
        /// <summary>
        /// 新增核销记录
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool AddVerificationRecord(VerificationRecordInfo info)
        {
            return Service.AddVerificationRecord(info);
        }
        public static VerificationRecordInfo GetVerificationRecordInfoById(long id)
        {
            return Service.GetVerificationRecordInfoById(id);
        }

        /// <summary>
        /// 新增虚拟订单项
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool AddVirtualOrderItemInfo(List<VirtualProductItem> infos)
        {
            List<VirtualOrderItemInfo> vinfos = new List<VirtualOrderItemInfo>();
            infos.ForEach(a =>
            {
                vinfos.Add(new VirtualOrderItemInfo()
                {
                    Content = a.Content,
                    OrderId = a.OrderId,
                    OrderItemId = a.OrderItemId,
                    VirtualProductItemName = a.VirtualProductItemName,
                    VirtualProductItemType = (ProductInfo.VirtualProductItemType)a.VirtualProductItemType
                });
            });
            return Service.AddVirtualOrderItemInfo(vinfos);
        }

        /// <summary>
        /// 订单核销码
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        public static bool AddOrderVerificationCodeInfo(List<OrderVerificationCodeInfo> infos)
        {
            return Service.AddOrderVerificationCodeInfo(infos);
        }
        public static int GetWaitConsumptionOrderNumByUserId(long userId = 0, long shopId = 0, long shopBranchId = 0)
        {
            return Service.GetWaitConsumptionOrderNumByUserId(userId, shopId, shopBranchId);
        }
        public static QueryPageModel<VerificationRecordModel> GetVerificationRecords(VerificationRecordQuery query)
        {
            var data = Service.GetVerificationRecords(query);
            if (data.Models.Count <= 0)
                return new QueryPageModel<VerificationRecordModel>()
                {
                    Models = new List<VerificationRecordModel>(),
                    Total = data.Total
                };

            var models = data.Models.Map<List<DTO.VerificationRecordModel>>();
            var orderItems = OrderApplication.GetOrderItemsByOrderId(models.Select(a => a.OrderId));
            var products = ProductManagerApplication.GetAllProductByIds(orderItems.Select(a => a.ProductId));
            foreach (var a in models)
            {
                var orderItemInfo = orderItems.FirstOrDefault(p => p.OrderId == a.OrderId);
                if (orderItemInfo != null)
                {
                    var productInfo = products.FirstOrDefault(p => p.Id == orderItemInfo.ProductId);
                    if (productInfo != null)
                    {
                        a.ProductName = productInfo.ProductName;
                        a.ImagePath = Core.MallIO.GetRomoteProductSizeImage(productInfo.RelativePath, 1, (int)Mall.CommonModel.ImageSize.Size_350);
                        a.Specifications = string.Format("{0}{1}{2}", orderItemInfo.Color, orderItemInfo.Size, orderItemInfo.Version);
                        a.Quantity = a.VerificationCodeIds.Trim(',').Split(',').Count();
                        a.Price = orderItemInfo.SalePrice;
                        a.Time = a.VerificationTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
            }
            return new QueryPageModel<VerificationRecordModel>
            {
                Models = models,
                Total = data.Total
            };
        }

        public static QueryPageModel<OrderVerificationCodeModel> GetOrderVerificationCodeInfos(VerificationRecordQuery query)
        {
            var data = Service.GetOrderVerificationCodeInfos(query);
            if (data.Models.Count <= 0)
                return new QueryPageModel<OrderVerificationCodeModel>()
                {
                    Models = new List<OrderVerificationCodeModel>(),
                    Total = data.Total
                };

            var models = data.Models.Map<List<DTO.OrderVerificationCodeModel>>();
            var shopBranchs = ShopBranchApplication.GetShopBranchByIds(models.Select(a => a.ShopBranchId).ToList());
            var shops = ShopApplication.GetShops(models.Select(a => a.ShopId).ToList());
            foreach (var a in models)
            {
                var shopBranchInfo = shopBranchs.FirstOrDefault(p => p.Id == a.ShopBranchId);
                if (shopBranchInfo != null)
                {
                    a.Name = shopBranchInfo.ShopBranchName;
                }
                if (string.IsNullOrWhiteSpace(a.Name) && a.ShopBranchId == 0)
                {
                    var shop = shops.FirstOrDefault(p => p.Id == a.ShopId);
                    if (shop != null)
                        a.Name = shop.ShopName;
                }
                a.VerificationTimeText = a.VerificationTime.HasValue ? a.VerificationTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";
                a.StatusText = a.Status.ToDescription();
                a.PayDateText = a.PayDate.HasValue ? a.PayDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";
                if (a.Status == OrderInfo.VerificationCodeStatus.WaitVerification || a.Status == OrderInfo.VerificationCodeStatus.Refund)
                {
                    a.VerificationCode = Regex.Replace(a.VerificationCode, "(\\d{4})\\d{4}(\\d{4})", "$1 **** $2");
                }
                else
                {
                    a.VerificationCode = Regex.Replace(a.VerificationCode, @"(\d{4})", "$1 ");
                }
            }
            return new QueryPageModel<OrderVerificationCodeModel>
            {
                Models = models,
                Total = data.Total
            };
        }

        public static List<SearchShopAndShopbranchModel> GetShopOrShopBranch(string keyword, sbyte? type = null)
        {
            return Service.GetShopOrShopBranch(keyword, type);
        }

        /// <summary>
        /// 获取订单发票实体
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static OrderInvoiceInfo GetOrderInvoiceInfo(long orderId)
        {
            return Service.GetOrderInvoiceInfo(orderId);
        }
    }
}
