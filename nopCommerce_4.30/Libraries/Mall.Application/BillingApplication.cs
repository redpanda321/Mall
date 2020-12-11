using System;
using System.Collections.Generic;
using System.Linq;
using Mall.IServices;
using Mall.DTO;
using Mall.CommonModel;
using Mall.Entities;
using AutoMapper;
using Mall.Core;
using Mall.Core.Plugins.Payment;
using Mall.DTO.QueryModel;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    public class BillingApplication
    {
        private const string PLUGIN_PAYMENT_ALIPAY = "Mall.Plugin.Payment.Alipay";
     //   private static IBillingService _iBillingService =  EngineContext.Current.Resolve<IBillingService>();
      //  private static IMemberCapitalService _iMemberCapitalService =  EngineContext.Current.Resolve<IMemberCapitalService>();


        private static IBillingService _iBillingService =  EngineContext.Current.Resolve<IBillingService>();
        private static IMemberCapitalService _iMemberCapitalService =  EngineContext.Current.Resolve<IMemberCapitalService>();


        /// <summary>
        /// 根据ShopID获取该店铺的财务总览信息
        /// </summary>
        /// <param name="shopId">店铺ID</param>
        /// <returns></returns>
        public static ShopAccount GetShopAccount(long shopId)
        {
            if (shopId == 0)
            {
                throw new Core.MallException("错误的店铺ID");
            }
            var model = _iBillingService.GetShopAccount(shopId);
            var shopAccount = model.Map<ShopAccount>();
            shopAccount.FrozenAmount = _iBillingService.GetFrozenAmount(shopId);
            return shopAccount;
        }

        /// <summary>
        /// 获取平台帐户信息
        /// </summary>
        /// <returns></returns>
        public static PlatAccount GetPlatAccount()
        {
            var model = _iBillingService.GetPlatAccount();
            
            var platAccount = model.Map<PlatAccount>();
            return platAccount;
        }


        /// <summary>
        /// 获取首页交易额图表
        /// </summary>
        public static LineChartDataModel<decimal> GetTradeChart(DateTime start, DateTime end, long? shopId)
        {
            start = start.Date;
            end = end.Date;
            var model = _iBillingService.GetTradeChart(start, end, shopId);
            return model;
        }
        /// <summary>
        /// 获取本月交易额图表
        /// </summary>
        public static LineChartDataModel<decimal> GetTradeChartMonth(DateTime start, DateTime end, long? shopId)
        {
            start = start.Date;
            end = end.Date;
            var model = _iBillingService.GetTradeChartMonth(start, end, shopId);
            return model;
        }

        /// <summary>
        /// 获取店铺财务总览
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static ShopBillingIndex GetShopBillingIndex(long shopId)
        {
            
            var shopaccount = GetShopAccount(shopId);
            var statistics = OrderApplication.GetYesterDayStatistics(shopId);
            ShopBillingIndex model = new ShopBillingIndex
            {
                YesterDayOrders = statistics.OrdersNum,
                YesterDayPayOrders = statistics.PayOrdersNum,
                YesterDaySaleAmount = statistics.SaleAmount,
                ShopAccout = shopaccount
            };
            return model;
        }


        /// <summary>
        /// 获取平台财务总览
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static PlatBillingIndex GetPlatBillingIndex()
        {
            var platAccount = GetPlatAccount();
            var statistics = OrderApplication.GetYesterDayStatistics(0);
            PlatBillingIndex model = new PlatBillingIndex
            {
                YesterDayOrders = statistics.OrdersNum,
                YesterDayPayOrders = statistics.PayOrdersNum,
                YesterDaySaleAmount = statistics.SaleAmount,
                PlatAccout = platAccount
            };
            return model;
        }

        /// <summary>
        /// 根据日期获取当前结算周期
        /// </summary>
        /// <returns></returns>
        public static SettlementCycle GetCurrentBilingTime()
        {
            var settlementCycle = SiteSettingApplication.SiteSettings.WeekSettlement;
            var end = _iBillingService.GetLastSettlementTime();
            return GetDateBilingTime(settlementCycle, end, DateTime.Now);
        }


        /// <summary>
        /// 根据日期获取该日期的结算周期
        /// </summary>
        /// <returns></returns>
        public static SettlementCycle GetDateBilingTime(int settlementCycle, DateTime? endDate, DateTime dt)
        {
            SettlementCycle model = new SettlementCycle();
            var end = endDate;
            if (!end.HasValue)
            {
                model.StartTime = DateTime.Now.Date;
                model.EndTime = model.StartTime.AddDays(settlementCycle);
            }
            else
            {
                var now = dt.Date;

                var days = (now - end.Value.Date).Days; //和最后结算时间相差的天数

                var d = days % settlementCycle;

                var newend = now.AddDays(settlementCycle - d);

                var newStart = newend.AddDays(-settlementCycle);

                model.StartTime = newStart;
                model.EndTime = newend;
            }
            return model;
        }

        /// <summary>
        /// 平台待结算列表上方显示的汇总信息
        /// </summary>
        /// <returns></returns>
        public static PlatSettlementCycle GetPlatSettlementCycle()
        {
            var model = GetCurrentBilingTime();
            PlatSettlementCycle info = new PlatSettlementCycle();
            info.StartTime = model.StartTime;
            info.EndTime = model.EndTime;
            info.PlatCommission = _iBillingService.GetPendingPlatCommission();
            return info;
        }

        /// <summary>
        /// 店铺待结算列表上方显示的汇总信息
        /// </summary>
        /// <returns></returns>
        public static ShopSettlementCycle GetShopSettlementCycle(long shopId)
        {
            var shopAccount = GetShopAccount(shopId);
            var model = GetCurrentBilingTime();
            ShopSettlementCycle info = new ShopSettlementCycle();
            info.StartTime = model.StartTime;
            info.EndTime = model.EndTime;
            info.PendingSettlement = shopAccount.PendingSettlement;
            return info;
        }

        /// <summary>
        ///分页获取待结算订单列表
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="shopId">店铺ID</param>
        /// <param name="StartTime">订单完成时间</param>
        /// <param name="EndTime">订单完成时间</param>
        ///<param name="pageNo">当前页</param>
        ///<param name="pageSize">每页显示记录数</param>
        /// <returns></returns>
        public static QueryPageModel<PendingSettlementOrders> GetPendingSettlementOrders(PendingSettlementOrderQuery query)
        {
            QueryPageModel<PendingSettlementOrders> orders = new QueryPageModel<PendingSettlementOrders>();
            var model = _iBillingService.GetPendingSettlementOrders(query);
            orders.Total = model.Total;
            //  Mapper.CreateMap<PendingSettlementOrderInfo, PendingSettlementOrders>();
            //orders.Models = Mapper.Map<List<PendingSettlementOrderInfo>, List<PendingSettlementOrders>>(model.Models);
            orders.Models = model.Models.Map<List<PendingSettlementOrders>>();

            var orderids = orders.Models.Select(e => e.OrderId);
            var orderInfos = OrderApplication.GetOrders(orderids);
            foreach(var p in orders.Models)
            {
                var order = orderInfos.FirstOrDefault(e => e.Id == p.OrderId);
                p.Status = order != null ? order.OrderStatusStr : string.Empty;
            }

            //var settlementCycle = SiteSettingApplication.SiteSettings.WeekSettlement;
            //var end = _iBillingService.GetLastSettlementTime();
            //var CurrentSettlementCycle = GetDateBilingTime(settlementCycle, end, DateTime.Now); //节省一次查询
            //foreach (var m in orders.Models)
            //{
            //    m.DistributorCommission = m.DistributorCommission - m.DistributorCommissionReturn;
            //    m.PlatCommission = m.PlatCommission - m.PlatCommissionReturn;
            //    if (m.OrderFinshTime < CurrentSettlementCycle.StartTime) //如果订单完成时间不在当前结算周期之内
            //    {
            //        var cycle = GetDateBilingTime(settlementCycle, end, m.OrderFinshTime);
            //        m.SettlementCycle = "此订单为" + cycle.StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "至" + cycle.EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "周期内订单";
            //    }
            //}

            return orders;
        }

        /// <summary>
        ///分页获取待结算订单列表
        /// </summary>
		/// <param name="query"></param>
        /// <returns></returns>
        public static List<PendingSettlementOrders> GetAllPendingSettlementOrders(PendingSettlementOrderQuery query)
        {
            QueryPageModel<PendingSettlementOrders> orders = new QueryPageModel<PendingSettlementOrders>();
            var models = _iBillingService.GetAllPendingSettlementOrders(query);
            //    var result = Mapper.Map<List<PendingSettlementOrderInfo>, List<PendingSettlementOrders>>(models);

            var result = models.Map<List<PendingSettlementOrders>>();

            var orderids = models.Select(e => e.OrderId);
            var orderInfos = OrderApplication.GetOrders(orderids);
            var settlementCycle = SiteSettingApplication.SiteSettings.WeekSettlement;
            var end = _iBillingService.GetLastSettlementTime();
            var CurrentSettlementCycle = GetDateBilingTime(settlementCycle, end, DateTime.Now); //节省一次查询
            foreach (var m in result)
            {
                m.DistributorCommission = m.DistributorCommission;
                m.PlatCommission = m.PlatCommission;
                if (m.OrderFinshTime < CurrentSettlementCycle.StartTime) //如果订单完成时间不在当前结算周期之内
                {
                    var cycle = GetDateBilingTime(settlementCycle, end, m.OrderFinshTime);
                    m.SettlementCycle = "此订单为" + cycle.StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "至" + cycle.EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "周期内订单";
                }

                var order = orderInfos.FirstOrDefault(e => e.Id == m.OrderId);
                m.Status = order != null ? order.OrderStatusStr : string.Empty;
            }

            return result;
        }

        /// <summary>
        /// 根据订单ID获取结算详情（传入shopId防止跨店铺调用）
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="shopId">店铺ID</param>
        /// <returns></returns>
        public static OrderSettlementDetail GetPendingOrderSettlementDetail(long orderId, long? shopId = null)
        {
            var model = _iBillingService.GetPendingSettlementOrderDetail(orderId);
            if (shopId.HasValue && shopId.Value != model.ShopId)
            {
                throw new Core.MallException("找不到该店铺的结算详情");
            }

            //var order = OrderApplication.GetOrder(orderId);
            OrderSettlementDetail detail = new OrderSettlementDetail();
            detail.Freight = model.FreightAmount;
            detail.RefundAmount = model.RefundAmount;
            detail.DistributorCommission = model.DistributorCommission;
            detail.DistributorCommissionReturn = model.DistributorCommissionReturn;
            detail.PlatCommission = model.PlatCommission;
            detail.PlatCommissionReturn = model.PlatCommissionReturn;
            detail.ProductsTotal = model.ProductsAmount;
            detail.OrderAmount = model.OrderAmount;
            detail.OrderPayTime = model.CreateDate.ToString("yyyy-MM-dd HH:mm:ss");
            detail.IntegralDiscount = model.IntegralDiscount;
            detail.OrderRefundTime = model.RefundDate.HasValue ? model.RefundDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";
            
            return detail;
        }

        /// <summary>
        /// 根据订单ID获取结算详情（传入shopId防止跨店铺调用）
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="shopId">店铺ID</param>
        /// <returns></returns>
        public static OrderSettlementDetail GetOrderSettlementDetail(long orderId, long? shopId = null)
        {
            var model = _iBillingService.GetSettlementOrderDetail(orderId);
            if (shopId.HasValue && shopId.Value != model.ShopId)
            {
                throw new Core.MallException("找不到该店铺的结算详情");
            }

            var order = OrderApplication.GetOrder(orderId);
            var refund = RefundApplication.GetOrderRefundList(orderId);

            OrderSettlementDetail detail = new OrderSettlementDetail();
            detail.OrderAmount = model.OrderAmount;
            detail.Freight = model.FreightAmount;
            detail.RefundAmount = model.RefundTotalAmount;
            detail.DistributorCommission = model.BrokerageAmount;
            detail.DistributorCommissionReturn = model.ReturnBrokerageAmount;
            detail.PlatCommission = model.CommissionAmount;
            detail.PlatCommissionReturn = model.RefundCommisAmount;
            detail.ProductsTotal = model.ProductActualPaidAmount;
            detail.OrderPayTime = order.PayDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
            detail.IntegralDiscount = model.IntegralDiscount;
            if (refund != null && refund.Count > 0)
            {
                detail.OrderRefundTime = refund.FirstOrDefault().ManagerConfirmDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
            return detail;
        }

        /// <summary>
        /// 分页获取已结算订单列表
        /// </summary>
        /// <param name="query">查询实体</param>
        /// <returns></returns>
        public static QueryPageModel<SettledOrders> GetSettlementOrders(SettlementOrderQuery query)
        {
            var orders = new QueryPageModel<SettledOrders>();
            var model = _iBillingService.GetSettlementOrders(query);
            orders.Total = model.Total;
            var SettledOrders = new List<SettledOrders>();
            foreach (var m in model.Models)
            {
                var o = new SettledOrders();
                o.DistributorCommission = m.BrokerageAmount;
                o.OrderAmount = m.OrderAmount;
                o.OrderFinshTime = m.OrderFinshDate.ToString("yyyy-MM-dd HH:mm:ss");
                o.OrderId = m.OrderId;
                o.PlatCommission = m.CommissionAmount;
                o.RefundAmount = m.RefundTotalAmount;
                o.SettledTime = m.Date.ToString("yyyy-MM-dd HH:mm:ss");
                o.SettlementAmount = m.SettlementAmount;
                o.ShopName = m.ShopName;
                o.ShopId = m.ShopId;
                o.PaymentTypeName = m.PaymentTypeName;
                o.FreightAmount = m.FreightAmount;
                o.IntegralDiscount = m.IntegralDiscount;
                SettledOrders.Add(o);
            }
            orders.Models = SettledOrders;
            return orders;
        }

        /// <summary>
        /// 获取已结算订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<SettledOrders> GetAllSettlementOrders(SettlementOrderQuery query)
        {
            var models = _iBillingService.GetAllSettlementOrders(query);
            var settledOrders = new List<SettledOrders>();
            foreach (var m in models)
            {
                var o = new SettledOrders();
                o.DistributorCommission = m.BrokerageAmount;
                o.OrderAmount = m.OrderAmount;
                o.OrderFinshTime = m.OrderFinshDate.ToString("yyyy-MM-dd HH:mm:ss");
                o.OrderId = m.OrderId;
                o.PlatCommission = m.CommissionAmount;
                o.RefundAmount = m.RefundTotalAmount;
                o.SettledTime = m.Date.ToString("yyyy-MM-dd HH:mm:ss");
                o.SettlementAmount = m.SettlementAmount;
                o.ShopName = m.ShopName;
                o.ShopId = m.ShopId;
                o.PaymentTypeName = m.PaymentTypeName;
                o.IntegralDiscount = m.IntegralDiscount;
                settledOrders.Add(o);
            }
            return settledOrders;
        }

        /// <summary>
        /// 分页获取店铺的收支明细
        /// </summary>
        /// <param name="query">查询实体</param>
        /// <returns></returns>
        public static QueryPageModel<ShopAccountItem> GetShopAccountItem(ShopAccountItemQuery query)
        {
            var model = _iBillingService.GetShopAccountItem(query);

            var accountItem = model.Models.Map<List<ShopAccountItem>>();
            //补充收入明细的结算周期数据
            var accountIds = accountItem.Where(e=>e.IsIncome).Select(e => long.Parse(e.DetailId));
            var accounts = AccountApplication.GetAccounts(accountIds);
            foreach(var item in accountItem)
            {
                var account = accounts.FirstOrDefault(e => e.Id.ToString() == item.DetailId);
                var sDate = account != null ? account.StartDate.Date : item.CreateTime.AddDays(-item.SettlementCycle).Date;
                var eDate = account != null ? account.EndDate.Date : item.CreateTime.Date;
                item.Cycle = string.Format("{0:yyyy-MM-dd HH:mm:ss}-{1:yyyy-MM-dd HH:mm:ss}", sDate, eDate);
                item.SettlementCycleStart = sDate;
                item.SettlementCycleEnd = eDate;
            }

            return new QueryPageModel<ShopAccountItem>()
            {
                Total = model.Total,
                Models = accountItem
            };
        }

        /// <summary>
        /// 获取店铺的收支明细
        /// </summary>
        /// <param name="query">查询实体</param>
        /// <returns></returns>
        public static List<ShopAccountItem> GetAllShopAccountItem(ShopAccountItemQuery query)
        {
            var models = _iBillingService.GetAllShopAccountItem(query);

            return models.Map<List<ShopAccountItem>>();
        }

        /// <summary>
        /// 统计待结算订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<StatisticsPendingSettlement> StatisticsPendingSettlementOrders(StatisticsPendingSettlementQuery query)
        {
            return _iBillingService.StatisticsPendingSettlementOrders(query);
        }

        /// <summary>
        /// 统计待结算订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<StatisticsPendingSettlement> AllStatisticsPendingSettlementOrders(StatisticsPendingSettlementQuery query)
        {
            return _iBillingService.AllStatisticsPendingSettlementOrders(query);
        }

        /// <summary>
        /// 分页获取平台的收支明细
        /// </summary>
        /// <param name="query">查询实体</param> 
        /// <returns></returns>
        public static QueryPageModel<PlatAccountItem> GetPlatAccountItem(PlatAccountItemQuery query)
        {
            var model = _iBillingService.GetPlatAccountItem(query);

            var list = new List<PlatAccountItem>();
            foreach (var m in model.Models)
            {
                PlatAccountItem PlatAccountItem = new PlatAccountItem();
                PlatAccountItem.AccountNo = m.AccountNo;
                PlatAccountItem.Balance = m.Balance.ToString();
                PlatAccountItem.CreateTime = m.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
                PlatAccountItem.DetailId = m.DetailId;
                PlatAccountItem.PlatAccountType = m.TradeType;
                if (m.IsIncome)
                    PlatAccountItem.Income = m.Amount.ToString();
                else
                    PlatAccountItem.Expenditure = m.Amount.ToString();
                PlatAccountItem.Id = m.Id;
                list.Add(PlatAccountItem);
            }
            return new QueryPageModel<PlatAccountItem>
            {
                Total = model.Total,
                Models = list
            };
        }

        /// <summary>
        /// 分页获取平台的收支明细
        /// </summary>
        /// <param name="query">查询实体</param> 
        /// <returns></returns>
        public static List<PlatAccountItem> GetAllPlatAccountItem(PlatAccountItemQuery query)
        {
            var models = _iBillingService.GetAllPlatAccountItem(query);
            List<PlatAccountItem> items = new List<PlatAccountItem>();
            foreach (var m in models)
            {
                PlatAccountItem PlatAccountItem = new PlatAccountItem();
                PlatAccountItem.AccountNo = m.AccountNo;
                PlatAccountItem.Balance = m.Balance.ToString();
                PlatAccountItem.CreateTime = m.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
                PlatAccountItem.DetailId = m.DetailId;
                PlatAccountItem.PlatAccountType = m.TradeType;
                if (m.IsIncome)
                {
                    PlatAccountItem.Income = m.Amount.ToString();
                }
                else
                {
                    PlatAccountItem.Expenditure = m.Amount.ToString();
                }
                PlatAccountItem.Id = m.Id;
                items.Add(PlatAccountItem);
            }

            return items;
        }

        /// <summary>
        /// 获取营销服务费用明细（需移至营销服务BLL但目前没有此BLL）
        /// </summary>
        /// <param name="query">营销费用购买Id</param>
        /// <returns></returns>
        public static MarketServicesRecord GetMarketServiceRecord(long Id, long? shopId = null)
        {
            var model = MarketApplication.GetShopMarketServiceRecordInfo(Id);
            var active = MarketApplication.GetMarketService(model.MarketServiceId);
            if (shopId.HasValue && shopId.Value != active.ShopId)
            {
                throw new Core.MallException("找不到店铺的购买明细");
            }
            var record = ConvertToMarketServicesRecord(model);
            return record;
        }

        private static MarketServicesRecord ConvertToMarketServicesRecord(MarketServiceRecordInfo info)
        {
            MarketServicesRecord record = null;
            if (info != null)
            {
                var market = MarketApplication.GetMarketService(info.MarketServiceId);
                record = new MarketServicesRecord();
                record.BuyTime = info.BuyTime.ToString("yyyy-MM-dd HH:mm:ss");
                record.BuyingCycle = info.StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "至" + info.EndTime.ToString("yyyy-MM-dd HH:mm:ss");
                record.MarketType = (market.TypeId).ToDescription();
                record.Price = info.Price;
                record.ShopId = market.ShopId;
                record.ShopName = market.ShopName;
            }
            return record;
        }

        #region 充值
        /// <summary>
        /// 店铺创建支付
        /// </summary>
        /// <param name="shopId">店铺ID</param>
        /// <param name="balance">支付金额</param>
        /// <param name="webRoot">站点URL</param>
        /// <returns></returns>
        public static ChargePayModel PaymentList(long shopId, decimal balance, string webRoot)
        {

            Mall.Entities.ChargeDetailShopInfo model = new Mall.Entities.ChargeDetailShopInfo()
            {
                ChargeAmount = balance,
                ChargeStatus = Mall.Entities.ChargeDetailInfo.ChargeDetailStatus.WaitPay,
                CreateTime = DateTime.Now,
                ShopId = shopId
            };
            long orderIds = MemberCapitalApplication.AddChargeDetailShop(model);

            ChargePayModel viewmodel = new ChargePayModel();

            //获取同步返回地址
            string returnUrl = webRoot + "/SellerAdmin/Pay/CapitalChargeReturn/{0}";

            //获取异步通知地址
            string payNotify = webRoot + "/SellerAdmin/Pay/CapitalChargeNotify/{0}/";

            var payments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(PlatformType.PC));

            const string RELATEIVE_PATH = "/Plugins/Payment/";

            var models = payments.Select(item =>
            {
                string requestUrl = string.Empty;
                try
                {
                    requestUrl = item.Biz.GetRequestUrl(string.Format(returnUrl, EncodePaymentId(item.PluginInfo.PluginId)), string.Format(payNotify, EncodePaymentId(item.PluginInfo.PluginId)), orderIds.ToString(), model.ChargeAmount, "预存款充值");
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
            models = models.Where(item => !string.IsNullOrEmpty(item.RequestUrl) && item.Id != "Mall.Plugin.Payment.WeiXinPay" && item.Id != "Mall.Plugin.Payment.WeiXinPay_Native");//只选择正常加载的插件
            viewmodel.OrderIds = orderIds.ToString();
            viewmodel.TotalAmount = model.ChargeAmount;
            viewmodel.Step = 1;
            viewmodel.UnpaidTimeout = SiteSettingApplication.SiteSettings.UnpaidTimeout;
            viewmodel.models = models.ToList();
            return viewmodel;
        }

        /// <summary>
        /// 对PaymentId进行加密（因为PaymentId中包含小数点"."，因此进行编码替换）
        /// </summary>
        private static string EncodePaymentId(string paymentId)
        {
            return paymentId.Replace(".", "-");
        }

        /// <summary>
        /// 店铺充值
        /// </summary>
        /// <param name="Id">充值流水订单ID</param>
        /// <param name="TradNo">支付流水号</param>
        /// <param name="ChargeWay">支付方式</param>
        public static void ShopRecharge(long Id, string TradNo, string ChargeWay)
        {
            //处理充值流水记录
            var model = MemberCapitalApplication.GetChargeDetailShop(Id);
            if (model.ChargeStatus != Mall.Entities.ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess)
            {
                model.ChargeStatus = Mall.Entities.ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess;
                model.ChargeTime = DateTime.Now;
                model.ChargeWay = ChargeWay;
                MemberCapitalApplication.UpdateChargeDetailShop(model);

                //资金处理
                _iBillingService.UpdateAccount(model.ShopId, model.ChargeAmount, Mall.CommonModel.ShopAccountType.Recharge, TradNo, ChargeWay, Id);
            }
        }

        #endregion

        #region 提现
        private static object obj = new object();
        /// <summary>
        /// 商家申请提现
        /// </summary>
        /// <param name="draw">申请提现实体</param>
        /// <returns></returns>
        public static bool ShopApplyWithDraw(ShopWithDraw draw)
        {
            var mAccount = _iBillingService.GetShopAccount(draw.ShopId);
            if (mAccount.Balance >= draw.WithdrawalAmount)
            {
                var model = ShopApplication.GetShop(draw.ShopId);
                string Account = "";
                string AccountName = "";
                if (draw.WithdrawType.Equals(WithdrawType.BankCard))
                {
                    Account = model.BankAccountNumber;
                    AccountName = model.BankAccountName;
                }
                else if (draw.WithdrawType.Equals(WithdrawType.ALipay))
                {
                    Account = draw.Account;
                    AccountName = draw.AccountName;
                }
                else
                {
                    Account = model.WeiXinOpenId;
                    AccountName = model.WeiXinTrueName;
                }


                lock (obj)
                {
                    //处理余额
                    var mShopAccountInfo = _iBillingService.GetShopAccount(draw.ShopId);
                    mShopAccountInfo.Balance -= draw.WithdrawalAmount;
                    _iBillingService.UpdateShopAccount(mShopAccountInfo);
                }
                ShopWithDrawInfo Models = new ShopWithDrawInfo()
                {
                    Account = Account,
                    AccountName = AccountName,
                    ApplyTime = DateTime.Now,
                    CashAmount = draw.WithdrawalAmount,
                    CashType = draw.WithdrawType,
                    SellerId = draw.SellerId,
                    SellerName = draw.SellerName,
                    ShopId = draw.ShopId,
                    ShopRemark = "",
                    Status = Mall.CommonModel.WithdrawStaus.WatingAudit,
                    ShopName = model.ShopName,
                    CashNo = DateTime.Now.ToString("yyyyMMddHHmmssffff")
                };
                _iBillingService.AddShopWithDrawInfo(Models);


                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 店铺提现审核
        /// </summary>
        /// <param name="Id">审核ID</param>
        /// <param name="status">审核状态</param>
        /// <param name="Remark">平台备注</param>
        /// <param name="IpAddress">操作IP</param>
        /// <param name="UserName">操作人名称</param>
        /// <param name="isSyn">是否异步回调(用户支付宝付款判断)</param>
        /// <returns></returns>
        public static ShopWithDrawConfirmResult ShopApplyWithDraw(long Id, Mall.CommonModel.WithdrawStaus status, string Remark, string IpAddress = "", string UserName = "", string webRoot = "")
        {
            var model = _iBillingService.GetShopWithDrawInfo(Id);
            ShopWithDrawConfirmResult result = new ShopWithDrawConfirmResult { };

            if (status == Mall.CommonModel.WithdrawStaus.Refused)//拒绝
            {
                model.Status = status;
                model.PlatRemark = Remark;
                model.DealTime = DateTime.Now;
                _iBillingService.UpdateShopWithDraw(model);

                lock (obj)
                {
                    //处理余额
                    var mShopAccountInfo = _iBillingService.GetShopAccount(model.ShopId);
                    mShopAccountInfo.Balance += model.CashAmount;
                    _iBillingService.UpdateShopAccount(mShopAccountInfo);
                }


                //操作日志
                OperationLogApplication.AddPlatformOperationLog(new Entities.LogInfo
                {
                    Date = DateTime.Now,
                    Description = string.Format("店铺提现拒绝，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId, status, Remark),
                    IPAddress = IpAddress,
                    PageUrl = "/Admin/ShopWithDraw/Management",
                    UserName = UserName,
                    ShopId = 0

                });
                result.Success = true;
                return result;
            }
            else if (model.CashType == Mall.CommonModel.WithdrawType.BankCard)//银行卡
            {
                model.Status = status;
                model.PlatRemark = Remark;
                model.DealTime = DateTime.Now;
                _iBillingService.UpdateShopWithDraw(model);
                //资金处理
                UpdateAccount(model.ShopId, model.CashAmount, Mall.CommonModel.ShopAccountType.WithDraw, DateTime.Now.ToString("yyyyMMddHHmmssffff"), "银行卡提现", Id);


                //操作日志
                OperationLogApplication.AddPlatformOperationLog(new Entities.LogInfo
                {
                    Date = DateTime.Now,
                    Description = string.Format("店铺银行卡提现审核成功，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId, status, Remark),
                    IPAddress = IpAddress,
                    PageUrl = "/Admin/ShopWithDraw/Management",
                    UserName = UserName,
                    ShopId = 0

                });
                result.Success = true;
                return result;
            }
            else if (model.CashType == Mall.CommonModel.WithdrawType.ALipay)//支付宝
            {
                try
                {
                    if (status != WithdrawStaus.Succeed)
                    {
                        throw new MallException("错误的操作状态！");
                    }
                    #region 支付宝提现
                    if (model.Status == WithdrawStaus.PayPending)
                    {
                        throw new MallException("等待第三方处理中，如有误操作，请先取消后再进行付款操作！");
                    }

                    var plugins = PluginsManagement.GetPlugins<IPaymentPlugin>(true).FirstOrDefault(e => e.PluginInfo.PluginId == PLUGIN_PAYMENT_ALIPAY);
                    if (plugins == null)
                    {
                        throw new MallException("未找到支付插件!");
                    }
                    //异步通知地址
                    string payNotify = webRoot + "/Pay/ShopEnterpriseNotify/{0}?outid={1}";
                    EnterprisePayPara para = new EnterprisePayPara()
                    {
                        amount = model.CashAmount,
                        check_name = true,
                        openid = model.Account,
                        re_user_name = model.AccountName,
                        out_trade_no = model.ApplyTime.ToString("yyyyMMddHHmmss") + model.Id.ToString(),
                        desc = "提现",
                        notify_url = string.Format(payNotify, EncodePaymentId(plugins.PluginInfo.PluginId), model.Id.ToString())
                    };
                    PaymentInfo payresult = plugins.Biz.EnterprisePay(para);

                    model.SerialNo = payresult.TradNo;
                    model.Status = WithdrawStaus.Succeed;
                    model.PlatRemark = Remark;
                    model.DealTime = payresult.TradeTime.HasValue ? payresult.TradeTime.Value : DateTime.Now;
                    _iBillingService.UpdateShopWithDraw(model);
                    //资金处理
                    UpdateAccount(model.ShopId, model.CashAmount, Mall.CommonModel.ShopAccountType.WithDraw, payresult.TradNo, "支付宝提现", Id);

                    //操作日志
                    OperationLogApplication.AddPlatformOperationLog(new Entities.LogInfo
                    {
                        Date = DateTime.Now,
                        Description = string.Format("店铺开始支付宝提现，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId, status, Remark),
                        IPAddress = IpAddress,
                        PageUrl = "/Admin/ShopWithDraw/Management",
                        UserName = UserName,
                        ShopId = 0

                    });
                    result.Success = true;
                    result.JumpUrl = payresult.ResponseContentWhenFinished;
                    return result;
                }
                catch (Exception ex)
                {
                    Log.Error("商家提现审核异常：" + ex.Message);
                    //throw new MallException(ex.Message);
                    model.Status = WithdrawStaus.Fail;
                    model.PlatRemark = Remark;
                    model.DealTime = DateTime.Now;
                    _iBillingService.UpdateShopWithDraw(model);

                    //操作日志
                    OperationLogApplication.AddPlatformOperationLog(new Entities.LogInfo
                    {
                        Date = DateTime.Now,
                        Description = string.Format("店铺支付宝提现审核失败，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId, status, Remark),
                        IPAddress = IpAddress,
                        PageUrl = "/Admin/ShopWithDraw/Management",
                        UserName = UserName,
                        ShopId = 0

                    });
                    result.Success = false;
                    return result;
                }
                #endregion
            }
            else if (model.CashType == Mall.CommonModel.WithdrawType.WeiChat)//微信
            {
                var plugins = PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(e => e.PluginInfo.PluginId.ToLower().Contains("weixin")).FirstOrDefault();
                if (plugins != null)
                {
                    try
                    {
                        var shopModel = ShopApplication.GetShop(model.ShopId);
                        EnterprisePayPara para = new EnterprisePayPara()
                        {
                            amount = model.CashAmount,
                            check_name = false,
                            openid = shopModel.WeiXinOpenId,
                            out_trade_no = model.CashNo.ToString(),
                            desc = "提现"
                        };
                        PaymentInfo payresult = plugins.Biz.EnterprisePay(para);

                        model.SerialNo = payresult.TradNo;
                        model.DealTime = DateTime.Now;
                        model.Status = WithdrawStaus.Succeed;
                        model.PlatRemark = Remark;
                        _iBillingService.UpdateShopWithDraw(model);
                        //资金处理
                        UpdateAccount(model.ShopId, model.CashAmount, Mall.CommonModel.ShopAccountType.WithDraw, payresult.TradNo, "微信提现", Id);

                        //操作日志
                        OperationLogApplication.AddPlatformOperationLog(new Entities.LogInfo
                        {
                            Date = DateTime.Now,
                            Description = string.Format("店铺微信提现审核成功，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId, status, Remark),
                            IPAddress = IpAddress,
                            PageUrl = "/Admin/ShopWithDraw/Management",
                            UserName = UserName,
                            ShopId = 0

                        });

                        result.Success = true;
                        return result;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("调用企业付款接口异常：" + ex.Message);
                        model.Status = WithdrawStaus.Fail;
                        model.PlatRemark = Remark;
                        model.DealTime = DateTime.Now;
                        _iBillingService.UpdateShopWithDraw(model);

                        //操作日志
                        OperationLogApplication.AddPlatformOperationLog(new Entities.LogInfo
                        {
                            Date = DateTime.Now,
                            Description = string.Format("店铺微信提现审核失败，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId, status, Remark),
                            IPAddress = IpAddress,
                            PageUrl = "/Admin/ShopWithDraw/Management",
                            UserName = UserName,
                            ShopId = 0

                        });
                        result.Success = false;
                        return result;
                    }
                }
                else
                {
                    result.Success = false;
                    return result;
                }
            }
            else
            {
                result.Success = false;
                return result;
            }

        }
        /// <summary>
        /// 店铺提现审核批量处理
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="status"></param>
        /// <param name="Remark"></param>
        /// <param name="IpAddress"></param>
        /// <param name="UserName"></param>
        /// <param name="webRoot"></param>
        /// <returns></returns>
        public static List<ShopWithDrawConfirmResult> ShopApplyWithDrawBatch(string ids, Mall.CommonModel.WithdrawStaus status, string Remark, string IpAddress = "", string UserName = "", string webRoot = "")
        {
            var idArray = ids.Split(',').Select(e =>
            {
                long id = 0;
                long.TryParse(e, out id);
                return id;
            }).Where(e => e > 0);
            var modelList = _iBillingService.GetAllShopWithDraw(new WithdrawQuery {
                  Ids= idArray.ToArray()
            });
            List<ShopWithDrawConfirmResult> resultList = new List<ShopWithDrawConfirmResult>();
            foreach (var model in modelList)
            {
                if (status == Mall.CommonModel.WithdrawStaus.Refused)//拒绝
                {
                    model.Status = status;
                    model.PlatRemark = Remark;
                    model.DealTime = DateTime.Now;
                    _iBillingService.UpdateShopWithDraw(model);

                    lock (obj)
                    {
                        //处理余额
                        var mShopAccountInfo = _iBillingService.GetShopAccount(model.ShopId);
                        mShopAccountInfo.Balance += model.CashAmount;
                        _iBillingService.UpdateShopAccount(mShopAccountInfo);
                    }


                    //操作日志
                    OperationLogApplication.AddPlatformOperationLog(new Entities.LogInfo
                    {
                        Date = DateTime.Now,
                        Description = string.Format("店铺提现拒绝，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId, status, Remark),
                        IPAddress = IpAddress,
                        PageUrl = "/Admin/ShopWithDraw/Management",
                        UserName = UserName,
                        ShopId = 0

                    });
                    ShopWithDrawConfirmResult result = new ShopWithDrawConfirmResult();
                    result.Success = true;
                    resultList.Add(result);
                }
                else if (model.CashType == Mall.CommonModel.WithdrawType.BankCard)//银行卡
                {
                    model.Status = status;
                    model.PlatRemark = Remark;
                    model.DealTime = DateTime.Now;
                    _iBillingService.UpdateShopWithDraw(model);
                    //资金处理
                    UpdateAccount(model.ShopId, model.CashAmount, Mall.CommonModel.ShopAccountType.WithDraw, DateTime.Now.ToString("yyyyMMddHHmmssffff"), "银行卡提现", model.Id);


                    //操作日志
                    OperationLogApplication.AddPlatformOperationLog(new Entities.LogInfo
                    {
                        Date = DateTime.Now,
                        Description = string.Format("店铺银行卡提现审核成功，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId, status, Remark),
                        IPAddress = IpAddress,
                        PageUrl = "/Admin/ShopWithDraw/Management",
                        UserName = UserName,
                        ShopId = 0

                    });
                    ShopWithDrawConfirmResult result = new ShopWithDrawConfirmResult();
                    result.Success = true;
                    resultList.Add(result);
                }
                else if (model.CashType == Mall.CommonModel.WithdrawType.ALipay)//支付宝
                {
                    try
                    {
                        if (status != WithdrawStaus.Succeed)
                        {
                            throw new MallException("错误的操作状态！");
                        }
                        #region 支付宝提现
                        if (model.Status == WithdrawStaus.PayPending)
                        {
                            throw new MallException("等待第三方处理中，如有误操作，请先取消后再进行付款操作！");
                        }

                        var plugins = PluginsManagement.GetPlugins<IPaymentPlugin>(true).FirstOrDefault(e => e.PluginInfo.PluginId == PLUGIN_PAYMENT_ALIPAY);
                        if (plugins == null)
                        {
                            throw new MallException("未找到支付插件!");
                        }
                        //异步通知地址
                        //string payNotify = webRoot + "/Pay/ShopEnterpriseNotify/{0}?outid={1}";
                        //生成商户单号
                        var tradeno = model.ApplyTime.ToString("yyyyMMddHHmmss")+ model.Id.ToString();
                        EnterprisePayPara para = new EnterprisePayPara()
                        {
                            amount = model.CashAmount,
                            check_name = true,
                            openid = model.Account,
                            re_user_name = model.AccountName,
                            out_trade_no = tradeno,
                            desc = "提现"//,
                           // notify_url = string.Format(payNotify, EncodePaymentId(plugins.PluginInfo.PluginId), model.Id.ToString())
                        };
                        PaymentInfo payresult = plugins.Biz.EnterprisePay(para);
                        model.SerialNo = payresult.TradNo;
                        model.Status = WithdrawStaus.Succeed;
                        model.PlatRemark = Remark;
                        model.DealTime = payresult.TradeTime.HasValue ? payresult.TradeTime.Value : DateTime.Now;
                        _iBillingService.UpdateShopWithDraw(model);
                        //资金处理
                        UpdateAccount(model.ShopId, model.CashAmount, Mall.CommonModel.ShopAccountType.WithDraw, payresult.TradNo, "支付宝提现", model.Id);
                        //操作日志
                        OperationLogApplication.AddPlatformOperationLog(new Entities.LogInfo
                        {
                            Date = DateTime.Now,
                            Description = string.Format("店铺开始支付宝提现，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId, status, Remark),
                            IPAddress = IpAddress,
                            PageUrl = "/Admin/ShopWithDraw/Management",
                            UserName = UserName,
                            ShopId = 0

                        });

                        ShopWithDrawConfirmResult result = new ShopWithDrawConfirmResult();
                        result.Success = true;
                        result.JumpUrl = payresult.ResponseContentWhenFinished;
                        resultList.Add(result);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("商家提现审核异常：" + ex.Message);
                        //throw new MallException(ex.Message);
                        model.Status = WithdrawStaus.Fail;
                        model.PlatRemark = Remark;
                        model.DealTime = DateTime.Now;
                        _iBillingService.UpdateShopWithDraw(model);

                        //操作日志
                        OperationLogApplication.AddPlatformOperationLog(new Entities.LogInfo
                        {
                            Date = DateTime.Now,
                            Description = string.Format("店铺支付宝提现审核失败，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId, status, Remark),
                            IPAddress = IpAddress,
                            PageUrl = "/Admin/ShopWithDraw/Management",
                            UserName = UserName,
                            ShopId = 0
                        });

                        ShopWithDrawConfirmResult result = new ShopWithDrawConfirmResult();
                        result.Success = false;
                        resultList.Add(result);
                    }
                    #endregion
                }
                else if (model.CashType == Mall.CommonModel.WithdrawType.WeiChat)//微信
                {
                    var plugins = PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(e => e.PluginInfo.PluginId.ToLower().Contains("weixin")).FirstOrDefault();
                    if (plugins != null)
                    {
                        try
                        {
                            var shopModel = ShopApplication.GetShop(model.ShopId);
                            EnterprisePayPara para = new EnterprisePayPara()
                            {
                                amount = model.CashAmount,
                                check_name = false,
                                openid = shopModel.WeiXinOpenId,
                                out_trade_no = model.CashNo.ToString(),
                                desc = "提现"
                            };
                            PaymentInfo payresult = plugins.Biz.EnterprisePay(para);

                            model.SerialNo = payresult.TradNo;
                            model.DealTime = DateTime.Now;
                            model.Status = WithdrawStaus.Succeed;
                            model.PlatRemark = Remark;
                            _iBillingService.UpdateShopWithDraw(model);
                            //资金处理
                            UpdateAccount(model.ShopId, model.CashAmount, Mall.CommonModel.ShopAccountType.WithDraw, payresult.TradNo, "微信提现", model.Id);

                            //操作日志
                            OperationLogApplication.AddPlatformOperationLog(new Entities.LogInfo
                            {
                                Date = DateTime.Now,
                                Description = string.Format("店铺微信提现审核成功，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId, status, Remark),
                                IPAddress = IpAddress,
                                PageUrl = "/Admin/ShopWithDraw/Management",
                                UserName = UserName,
                                ShopId = 0

                            });

                            ShopWithDrawConfirmResult result = new ShopWithDrawConfirmResult();
                            result.Success = true;
                            resultList.Add(result);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("调用企业付款接口异常：" + ex.Message);
                            model.Status = WithdrawStaus.Fail;
                            model.PlatRemark = Remark;
                            model.DealTime = DateTime.Now;
                            _iBillingService.UpdateShopWithDraw(model);

                            //操作日志
                            OperationLogApplication.AddPlatformOperationLog(new Entities.LogInfo
                            {
                                Date = DateTime.Now,
                                Description = string.Format("店铺微信提现审核失败，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId, status, Remark),
                                IPAddress = IpAddress,
                                PageUrl = "/Admin/ShopWithDraw/Management",
                                UserName = UserName,
                                ShopId = 0

                            });
                            ShopWithDrawConfirmResult result = new ShopWithDrawConfirmResult();
                            result.Success = false;
                            resultList.Add(result);
                        }
                    }
                    else
                    {
                        ShopWithDrawConfirmResult result = new ShopWithDrawConfirmResult();
                        result.Success = false;
                        resultList.Add(result);
                    }
                }
                else
                {
                    ShopWithDrawConfirmResult result = new ShopWithDrawConfirmResult();
                    result.Success = false;
                    resultList.Add(result);
                }
            }
            return resultList;
        }
        public static ShopWithDrawConfirmResult ShopApplyWithDrawCallBack(long Id, Mall.CommonModel.WithdrawStaus status
            , EnterprisePayNotifyInfo payresult, string Remark, string IpAddress = "", string UserName = "")
        {
            var model = _iBillingService.GetShopWithDrawInfo(Id);
            ShopWithDrawConfirmResult result = new ShopWithDrawConfirmResult { };

            if (model.CashType == Mall.CommonModel.WithdrawType.ALipay)//支付宝
            {
                #region 支付宝提现
                try
                {
                    if (status == WithdrawStaus.Succeed)
                    {
                        //资金处理：在成功后处理，支付宝是异步回调，会再次调用此方法
                        UpdateAccount(model.ShopId, model.CashAmount, Mall.CommonModel.ShopAccountType.WithDraw, payresult.batch_no, "支付宝提现", Id);
                        _iBillingService.SetShopWithDrawSuccess(Id);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    Log.Error("商家提现审核异常：" + ex.Message);

                    model.Status = WithdrawStaus.Fail;
                    model.PlatRemark = Remark;
                    model.DealTime = DateTime.Now;
                    _iBillingService.UpdateShopWithDraw(model);

                    //操作日志
                    OperationLogApplication.AddPlatformOperationLog(new Entities.LogInfo
                    {
                        Date = DateTime.Now,
                        Description = string.Format("店铺支付宝提现审核失败，店铺Id={0},状态为：{1}, 说明是：{2}", model.ShopId, status, Remark),
                        IPAddress = IpAddress,
                        PageUrl = "/Admin/ShopWithDraw/Management",
                        UserName = UserName,
                        ShopId = 0

                    });
                    throw new MallException("付款接口异常!");
                }
                #endregion
            }
            else
            {
                result.Success = false;
                return result;
            }
        }

        /// <summary>
        /// 取消提现
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static bool CancelShopApplyWithDraw(long Id)
        {
            var model = _iBillingService.GetShopWithDrawInfo(Id);
            if (model.Status == Mall.CommonModel.WithdrawStaus.PayPending)//拒绝
            {
                model.Status = WithdrawStaus.WatingAudit;
                _iBillingService.UpdateShopWithDraw(model);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 资金记录添加
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="money"></param>
        /// <param name="TradeType"></param>
        /// <param name="AccountNo"></param>
        /// <param name="ChargeWay"></param>
        /// <param name="detailID"></param>
        private static void UpdateAccount(long shopId, decimal money, Mall.CommonModel.ShopAccountType TradeType, string AccountNo, string ChargeWay, long detailID = 0)
        {
            lock (obj)
            {
                //处理余额
                var mShopAccountInfo = _iBillingService.GetShopAccount(shopId);
                //处理充值记录
                ShopAccountItemInfo mShopAccountItemInfo = new ShopAccountItemInfo()
                {
                    AccountNo = AccountNo,
                    AccoutID = mShopAccountInfo.Id,
                    Amount = money,
                    Balance = mShopAccountInfo.Balance,
                    CreateTime = DateTime.Now,
                    DetailId = detailID.ToString(),
                    IsIncome = false,
                    ReMark = ChargeWay,
                    ShopId = shopId,
                    ShopName = mShopAccountInfo.ShopName,
                    TradeType = TradeType
                };
                _iBillingService.AddShopAccountItem(mShopAccountItemInfo);
            }
        }

        /// <summary>
        /// 获取店铺提现详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static ShopWithDrawInfo GetShopWithDrawInfo(long Id)
        {
            var data = _iBillingService.GetShopWithDrawInfo(Id);
            return data;
        }
        /// <summary>
        /// 分页查询店铺提现记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopWithDrawItem> GetShopWithDraw(WithdrawQuery query)
        {
            var model = _iBillingService.GetShopWithDraw(query);

            QueryPageModel<ShopWithDrawItem> Models = new QueryPageModel<ShopWithDrawItem>();

            List<ShopWithDrawItem> items = new List<ShopWithDrawItem>();
            foreach (ShopWithDrawInfo mInfo in model.Models)
            {
                ShopWithDrawItem swdi = new ShopWithDrawItem();

                swdi.Id = mInfo.Id;
                swdi.Account = mInfo.Account;
                swdi.AccountName = mInfo.AccountName;
                swdi.AccountNo = long.Parse(mInfo.CashNo);
                swdi.AccountNoText = mInfo.CashNo;
                swdi.ApplyTime = mInfo.ApplyTime.ToString("yyyy-MM-dd HH:mm:ss");
                swdi.CashAmount = mInfo.CashAmount.ToString("f2");
                swdi.WithStatus = (int)mInfo.Status;
                swdi.CashType = mInfo.CashType.ToDescription();
                swdi.DealTime = mInfo.DealTime == null ? "" : mInfo.DealTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                swdi.PlatRemark = mInfo.PlatRemark;
                swdi.SellerId = mInfo.SellerId;
                swdi.SellerName = mInfo.SellerName;
                swdi.ShopId = mInfo.ShopId;
                swdi.ShopName = mInfo.ShopName;
                swdi.ShopRemark = mInfo.ShopRemark;
                swdi.Status = mInfo.Status.ToDescription();

                items.Add(swdi);
            }
            Models.Models = items;
            Models.Total = model.Total;
            return Models;
        }

        /// <summary>
        /// 查询店铺提现记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<ShopWithDrawItem> GetAllShopWithDraw(WithdrawQuery query)
        {
            var models = _iBillingService.GetAllShopWithDraw(query);

            var items = new List<ShopWithDrawItem>();
            foreach (ShopWithDrawInfo mInfo in models)
            {
                var swdi = new ShopWithDrawItem();

                swdi.Id = mInfo.Id;
                swdi.Account = mInfo.Account;
                swdi.AccountName = mInfo.AccountName;
                swdi.AccountNo = long.Parse(mInfo.CashNo);
                swdi.AccountNoText = swdi.AccountNo.ToString();
                swdi.ApplyTime = mInfo.ApplyTime.ToString("yyyy-MM-dd HH:mm:ss");
                swdi.CashAmount = mInfo.CashAmount.ToString("f2");
                swdi.WithStatus = (int)mInfo.Status;
                swdi.CashType = mInfo.CashType.ToDescription();
                swdi.DealTime = mInfo.DealTime.HasValue ? mInfo.DealTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";
                swdi.PlatRemark = mInfo.PlatRemark;
                swdi.SellerId = mInfo.SellerId;
                swdi.SellerName = mInfo.SellerName;
                swdi.ShopId = mInfo.ShopId;
                swdi.ShopName = mInfo.ShopName;
                swdi.ShopRemark = mInfo.ShopRemark;
                swdi.Status = mInfo.Status.ToDescription();

                items.Add(swdi);
            }

            return items;
        }
        /// <summary>
        /// 获取店铺提现数
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static int GetShopWithDrawCount(WithdrawQuery query)
        {
            return _iBillingService.GetShopWithDrawCount(query);
        }
        #endregion

        /// <summary>
        /// 店铺结算统计信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static SettlementStatistics GetShopSettlementStatistics(long? shopId, long? accountId = null)
        {
            var SettlementAmount = _iBillingService.GetSettlementAmount(shopId, accountId);
            var PlatCommission = _iBillingService.GetPlatCommission(shopId, accountId);
            var DistributorCommission = _iBillingService.GetDistributorCommission(shopId, accountId);
            return new SettlementStatistics()
            {
                DistributorCommission = DistributorCommission,
                PlatCommission = PlatCommission,
                SettlementAmount = SettlementAmount
            };
        }
        /// <summary>
        /// 平台结算统计信息
        /// </summary>
        /// <returns></returns>

        public static PlatSettlementStatistics GetPlatSettlementStatistics()
        {
            var SettlementAmount = _iBillingService.GetSettlementAmount();
            var PlatCommission = _iBillingService.GetPlatCommission();
            var DistributorCommission = _iBillingService.GetDistributorCommission();

            return new PlatSettlementStatistics()
            {
                DistributorCommission = DistributorCommission,
                PlatCommission = PlatCommission,
                SettlementAmount = SettlementAmount
            };
        }

        /// <summary>
        /// 获取上一次结算的金额
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static decimal GetLastSettlementByShopId(long shopId)
        {
            return _iBillingService.GetLastSettlementByShopId(shopId);
        }


        /// <summary>
        /// 获取上一次结算的基本信息
        /// </summary>
        /// <returns></returns>
        public static SmipleAccount GetLastSettlementInfo()
        {
            var model = _iBillingService.GetLastAccountInfo();
            SmipleAccount m = new SmipleAccount();
            m.AccountId = model.Id;
            m.StartDate = model.StartDate;
            m.EndDate = model.EndDate;
            return m;
        }



        /// <summary>
        /// 店铺近一年的结算历史
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopSettledHistory> GetShopYearSettledHistory(ShopSettledHistoryQuery query)
        {
            query.MinSettleTime = DateTime.Now.AddYears(-1);
            ShopAccountItemQuery itemQuery = new ShopAccountItemQuery();
            itemQuery.TimeStart = query.MinSettleTime;
            itemQuery.PageNo = query.PageNo;
            itemQuery.PageSize = query.PageSize;
            itemQuery.ShopId = query.ShopId;
            itemQuery.ShopAccountType = ShopAccountType.SettlementIncome;
            var account = _iBillingService.GetShopAccountItem(itemQuery); ;
            //var account = _iBillingService.GetSettledHistory(query);
            QueryPageModel<ShopSettledHistory> result = new QueryPageModel<ShopSettledHistory>();
            result.Total = account.Total;

            //补充结算周期数据
            var accountIds = account.Models.Select(e => long.Parse(e.DetailId));
            var accounts = AccountApplication.GetAccounts(accountIds);

            List<ShopSettledHistory> history = new List<ShopSettledHistory>();
            foreach (var m in account.Models)
            {
                var info = accounts.FirstOrDefault(e => e.Id.ToString() == m.DetailId);
                var sDate = account != null ? info.StartDate.Date : m.CreateTime.AddDays(-m.SettlementCycle).Date;
                var eDate = account != null ? info.EndDate.Date : m.CreateTime.Date;

                ShopSettledHistory h = new ShopSettledHistory();
                h.AccountTime = m.CreateTime;
                h.StartTime = sDate;
                h.EndTime = eDate;
                h.SettlementAmount = m.Amount;
                h.AccountId = long.Parse(m.DetailId);
                history.Add(h);
            }
            result.Models = history;
            return result;
        }

        /// <summary>
        /// 商家冻结金额
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static decimal GetFrozenAmount(long shopId)
        {
            return _iBillingService.GetFrozenAmount(shopId);
        }

        #region 支付宝提现

        //public static string PreGetRequestUrl(string[] ids, string returnUrl, string notifyUrl, string[] userAccount, string[] realName, decimal[] amount)
        //{
        //    var siteSetting = SiteSettingApplication.SiteSettings;
        //    //商户设置初始化
        //    string partner = siteSetting.Withdraw_AlipayPartner;
        //    string key = siteSetting.Withdraw_AlipayKey;
        //    string input_charset = "utf-8";

        //    //服务器异步通知页面路径
        //    string notify_url = notifyUrl;
        //    //需http://格式的完整路径，不允许加?id=123这类自定义参数

        //    //付款账号
        //    string email = siteSetting.Withdraw_AlipayAccount;
        //    //必填

        //    //付款账户名
        //    string account_name = siteSetting.Withdraw_AlipayAccountName;
        //    //必填，个人支付宝账号是真实姓名公司支付宝账号是公司名称

        //    //付款当天日期
        //    string pay_date = DateTime.Now.ToString("yyyyMMdd");
        //    //必填，格式：年[4位]月[2位]日[2位]，如：20100801

        //    //批次号
        //    string batch_no = DateTime.Now.ToString("yyyyMMddHHmmssff");
        //    //必填，格式：当天日期[8位]+序列号[3至16位]，如：201008010000001

        //    //付款总金额
        //    decimal batch_fee = 0;
        //    //必填，即参数detail_data的值中所有金额的总和

        //    //付款笔数
        //    string batch_num = string.Empty;
        //    //必填，即参数detail_data的值中，“|”字符出现的数量加1，最大支持1000笔（即“|”字符出现的数量999个）

        //    //付款详细数据
        //    string detail_data = string.Empty;
        //    //必填，格式：流水号1^收款方帐号1^真实姓名^付款金额1^备注说明1|流水号2^收款方帐号2^真实姓名^付款金额2^备注说明2....

        //    int batchLen = ids.Length;
        //    batch_num = batchLen.ToString();
        //    string[] outPayId = new string[batchLen];
        //    for (int i = 0; i < batchLen; i++)
        //    {
        //        var model = _iMemberCapitalService.GetApplyWithDrawInfo(Convert.ToInt64(ids[i]));
        //        batch_fee += amount[i];
        //        outPayId[i] = DateTime.Now.ToString("yyyyMMddHHmmssfff") + i.ToString();
        //        //更新 PayNo
        //        _iMemberCapitalService.UpdateApplyWithDrawInfoPayNo(model.Id, outPayId[i]);
        //        if (i != 0)
        //        {
        //            detail_data += "|";
        //        }
        //        detail_data += outPayId[i] + "^" + userAccount[i] + "^" + realName[i] + "^" + amount[i].ToString("f2") + "^" + "支付宝批量提现";
        //    }

        //    //detail_data = "1001^18627587087^test^1^test";
        //    Submit.setConfig(partner, "MD5", key, input_charset);

        //    //把请求参数打包成数组
        //    SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
        //    sParaTemp.Add("partner", partner);
        //    sParaTemp.Add("_input_charset", input_charset);
        //    sParaTemp.Add("service", "batch_trans_notify");
        //    sParaTemp.Add("notify_url", notify_url);
        //    sParaTemp.Add("email", email);
        //    sParaTemp.Add("account_name", account_name);
        //    sParaTemp.Add("pay_date", pay_date);
        //    sParaTemp.Add("batch_no", batch_no);
        //    sParaTemp.Add("batch_fee", batch_fee.ToString());
        //    sParaTemp.Add("batch_num", batch_num);
        //    sParaTemp.Add("detail_data", detail_data);
        //    return Submit.BuildRequest(sParaTemp, "post", "确认付款");
        //}

        //public static string PreProcessNotify(HttpRequestBase request, string ids, bool isSuccess)
        //{
        //    var queryString = GetQuerystring(request);
        //    Log.Debug("会员支付宝提现Notify,QueryString:" + queryString.ToString());
        //    SortedDictionary<string, string> sPara = GetRequestPost(queryString);
        //    string result = "fail";

        //    if (isSuccess)
        //    {
        //        if (queryString.Count > 0)//判断是否有带返回参数
        //        {
        //            Notify notify = new Notify();
        //            var siteSetting = SiteSettingApplication.SiteSettings;
        //            string key = siteSetting.Withdraw_AlipayKey;
        //            string sign_type = "MD5";
        //            if (sPara.ContainsKey("sign_type"))
        //            {
        //                sign_type = sPara["sign_type"];
        //            }
        //            string paramsInfo = "ids:" + ids;
        //            paramsInfo += "\r\n isSuccess:" + isSuccess.ToString();
        //            foreach (var item in sPara.Keys)
        //            {
        //                paramsInfo += "\r\n " + item + ":" + sPara[item].ToString();
        //            }

        //            Log.Debug("回调参数：" + paramsInfo);
        //            bool verifyResult = false;

        //            try
        //            {
        //                verifyResult = notify.Verify(sPara, sPara["notify_id"], sPara["sign"], key, sign_type);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Debug("验证签名失败：" + ex.Message);
        //                return "";
        //            }

        //            if (verifyResult)
        //            {
        //                Log.Debug("回调验证签名成功");
        //                string detail = string.Empty;
        //                //批量付款数据中转账成功的详细信息
        //                // 'SID号^帐号^姓名^金额^状态^备注^内部流水号^支付时间
        //                if (sPara.ContainsKey("success_details"))
        //                    detail = sPara["success_details"];

        //                if (sPara.ContainsKey("fail_details"))
        //                {
        //                    if (string.IsNullOrEmpty(detail))
        //                    {
        //                        detail = sPara["fail_details"];
        //                    }
        //                    else
        //                    {
        //                        if (detail.LastIndexOf('|') == detail.Length - 1)
        //                            detail += sPara["fail_details"];
        //                        else
        //                            detail += "|" + sPara["fail_details"];
        //                    }
        //                }

        //                if (!string.IsNullOrEmpty(detail))
        //                {
        //                    if (detail.LastIndexOf('|') == detail.Length - 1)
        //                        detail = detail.Substring(0, detail.Length - 1);

        //                    //Log.Debug("detail："+ detail);
        //                    string[] ResultList = detail.Split('|');
        //                    var dIds = ids.Split(',');
        //                    //Log.Debug("处理回调记录1"+ ResultList.Count());
        //                    foreach (var id in dIds)
        //                    {
        //                        long lId = Convert.ToInt64(id);
        //                        var service = _iMemberCapitalService;
        //                        var model = service.GetApplyWithDrawInfo(lId);
        //                        //Log.Debug("处理回调记录2："+(model == null).ToString());
        //                        foreach (string ItemRs in ResultList)
        //                        {
        //                            string[] ItemDetail = ItemRs.Split('^');
        //                            Log.Debug("处理回调记录3：" + ItemDetail.Count());
        //                            if (ItemDetail.Count() >= 8)
        //                            {
        //                                Log.Debug("处理回调记录4：");
        //                                if (model.PayNo == ItemDetail[0] && ItemDetail[4] == "S")
        //                                {
        //                                    //支付宝内部流水号
        //                                    string serialNo = ItemDetail[6];
        //                                    //model.PayNo = ItemDetail[0];
        //                                    model.Remark = "预付款支付宝批量提现成功" + serialNo;
        //                                    model.ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.WithDrawSuccess;
        //                                    model.ConfirmTime = DateTime.Now;
        //                                    model.PayTime = DateTime.Now;
        //                                    Log.Debug("提现:" + model.PayNo);
        //                                    service.ConfirmApplyWithDraw(model);


        //                                }
        //                                else if (model.PayNo == ItemDetail[0] && ItemDetail[4] == "F")
        //                                {
        //                                    string serialNo = ItemDetail[6];
        //                                    //model.PayNo = ItemDetail[0];
        //                                    model.Remark = ItemDetail[5] + "内部流水号" + serialNo;
        //                                    model.ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.PayFail;
        //                                    model.ConfirmTime = DateTime.Now;
        //                                    model.PayTime = DateTime.Now;
        //                                    Log.Debug("提现:" + model.PayNo);
        //                                    service.ConfirmApplyWithDraw(model);
        //                                }

        //                                Log.Debug("处理回调记录5：返回成功");
        //                                result = "success";
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Log.Debug("预付款支付宝提现Notify请求验证失败,QueryString:" + queryString.ToString());
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var dIds = ids.Split(',');
        //        var service = _iMemberCapitalService;

        //        foreach (var id in dIds)
        //        {
        //            ApplyWithDrawQuery query = new ApplyWithDrawQuery
        //            {
        //                withDrawNo = Convert.ToInt64(id),
        //                PageNo = 1,
        //                PageSize = 1
        //            };
        //            var model = service.GetApplyWithDraw(query).Models.FirstOrDefault();

        //            ApplyWithDrawInfo info = new ApplyWithDrawInfo
        //            {
        //                ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.PayFail,
        //                Remark = "预付款支付宝批量提现失败",
        //                ConfirmTime = DateTime.Now,
        //                OpUser = "admin",
        //                ApplyAmount = model.ApplyAmount,
        //                Id = model.Id
        //            };
        //            service.ConfirmApplyWithDraw(info);

        //            Log.Debug("预付款支付宝批量提现异常, 流水号：" + model.PayNo);
        //        }
        //    }

        //    return result;
        //}
        #endregion        
    }
}
