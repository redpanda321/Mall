using Mall.CommonModel;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Service
{
    /// <summary>
    /// 结算的数据层实现
    /// </summary>
    public class BillingService : ServiceBase, IBillingService
    {
        #region 字段
        private static object obj = new object();
        #endregion

        #region 方法
        /// <summary>
        /// 店铺帐户（在入驻成功后建立一个帐户）
        /// </summary>
        /// <param name="model"></param>
        public void AddShopAccount(ShopAccountInfo model)
        {
            //if (!Context.ShopAccountInfo.Any(a => a.ShopId == model.ShopId))
            if (!DbFactory.Default.Get<ShopAccountInfo>().Where(a => a.ShopId == model.ShopId).Exist())
            {
                //Context.ShopAccountInfo.Add(model);
                //Context.SaveChanges();
                DbFactory.Default.Add(model);
            }
        }

        /// <summary>
        /// 更新店铺资金信息（结算时，退款时，充值时）
        /// </summary>
        /// <param name="model"></param>
        public void UpdateShopAccount(ShopAccountInfo model)
        {
            //Context.ShopAccountInfo.Attach(model);
            //Context.Entry(model).State = System.Data.Entity.EntityState.Modified;
            //Context.SaveChanges();
            DbFactory.Default.Update(model);
        }

        /// <summary>
        /// 根据店铺ID获取店铺帐户信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public ShopAccountInfo GetShopAccount(long shopId)
        {
            //var model = Context.ShopAccountInfo.FirstOrDefault(a => a.ShopId == shopId);
            var model = DbFactory.Default.Get<ShopAccountInfo>().Where(a => a.ShopId == shopId).FirstOrDefault();
            //待结算金额总在变化改为实时查询吧20160709
            //var PendingSettlementAmount = Context.PendingSettlementOrdersInfo.Where(a => a.ShopId == shopId).Sum(a => (decimal?)a.SettlementAmount).GetValueOrDefault();
            var PendingSettlementAmount = DbFactory.Default.Get<PendingSettlementOrderInfo>().Where(a => a.ShopId == shopId).Sum<decimal>(a => a.SettlementAmount);
            model.PendingSettlement = PendingSettlementAmount;
            return model;
        }

        /// <summary>
        /// 店铺流水
        /// </summary>
        /// <param name="item"></param>
        public void AddShopAccountItem(ShopAccountItemInfo model)
        {

            DbFactory.Default.Add(model);
        }

        /// <summary>
        /// 分页获取店铺流水
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<ShopAccountItemInfo> GetShopAccountItem(ShopAccountItemQuery query)
        {
            var db = ToWhere(query);
            switch (query.Sort.ToLower())
            {
                case "amount":
                    if (query.IsAsc) db.OrderBy(p => p.Amount);
                    else db.OrderByDescending(p => p.Amount);
                    break;
                case "createtime":
                    if (query.IsAsc) db.OrderBy(p => p.CreateTime);
                    else db.OrderByDescending(p => p.CreateTime);
                    break;
                default:
                    db.OrderByDescending(o => o.Id);
                    break;
            }
            var models = db.ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<ShopAccountItemInfo>() { Models = models, Total = models.TotalRecordCount };
        }

        /// <summary>
        /// 获取店铺流水
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<ShopAccountItemInfo> GetAllShopAccountItem(ShopAccountItemQuery query)
        {
            var itemQuery = ToWhere(query);
            return itemQuery.ToList();
        }

        /// <summary>
        /// 分页获取平台流水
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<PlatAccountItemInfo> GetPlatAccountItem(PlatAccountItemQuery query)
        {
            var db = ToWhere(query);
            var models = db.OrderByDescending(o => o.Id).ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<PlatAccountItemInfo>
            {
                Models = models,
                Total = models.TotalRecordCount
            };
        }

        /// <summary>
        /// 分页获取平台流水
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<PlatAccountItemInfo> GetAllPlatAccountItem(PlatAccountItemQuery query)
        {
            var itemQuery = ToWhere(query);
            return itemQuery.OrderByDescending(p => p.Id).ToList();
        }

        /// <summary>
        /// 店铺提现申请
        /// </summary>
        /// <param name="item"></param>
        public void AddShopWithDrawInfo(ShopWithDrawInfo info)
        {
            //Context.ShopWithDrawInfo.Add(info);
            //Context.SaveChanges();
            DbFactory.Default.Add(info);
        }

        /// <summary>
        /// 获取店铺提现详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ShopWithDrawInfo GetShopWithDrawInfo(long Id)
        {
            //return Context.ShopWithDrawInfo.FirstOrDefault(a => a.Id == Id);
            return DbFactory.Default.Get<ShopWithDrawInfo>().Where(a => a.Id == Id).FirstOrDefault();
        }

        public LineChartDataModel<decimal> GetTradeChart(DateTime start, DateTime end, long? shopId)
        {
            LineChartDataModel<decimal> chart = new LineChartDataModel<decimal>() { SeriesData = new List<ChartSeries<decimal>>() };

            var data = DbFactory.Default.Get<OrderInfo>().Where(a => a.PayDate >= start && a.PayDate <= end);
            if (shopId.HasValue && shopId.Value > 0)
            {
                data.Where(a => a.ShopId == shopId);
            }
            var list = data.Select(a => new { Amount = (a.ProductTotalAmount + a.Freight + a.Tax - a.DiscountAmount), PayDate = a.PayDate }).ToList<dynamic>();

            var days = (end - start).Days; //相差的天数

            string[] arr = new string[days + 1];
            for (int i = 0; i <= days; i++)
            {
                arr[i] = start.Date.AddDays(i).ToString("MM/dd");
            }

            chart.XAxisData = arr;

            var arrEx = new string[days + 1];

            var chartItem = new ChartSeries<decimal> { Name = "交易额走势图", Data = new decimal[days + 1] };

            for (int i = 0; i <= days; i++)
            {
                var date = start.Date.AddDays(i).Date;

                var m = list.Where(a => DateTime.Parse(a.PayDate.ToString("yyyy/MM/dd")) == date).ToList();
                if (m.Count > 0)
                {
                    chartItem.Data[i] = m.Sum(a => (decimal)a.Amount);
                    arrEx[i] = date + "的销售额为:" + chartItem.Data[i];
                }
                else
                {
                    chartItem.Data[i] = 0;
                    arrEx[i] = date + "的销售额为:" + chartItem.Data[i];
                }
            }
            chart.ExpandProp = arrEx;
            chart.SeriesData.Add(chartItem);

            return chart;
        }

        public LineChartDataModel<decimal> GetTradeChartMonth(DateTime start, DateTime end, long? shopId)
        {
            LineChartDataModel<decimal> chart = new LineChartDataModel<decimal>() { SeriesData = new List<ChartSeries<decimal>>() };

            //var data = Context.OrderInfo.Where(a => a.PayDate.HasValue && a.PayDate >= start && a.PayDate < end);
            var data = DbFactory.Default.Get<OrderInfo>().Where(a => a.PayDate >= start && a.PayDate < end);
            if (shopId.HasValue && shopId.Value > 0)
            {
                data.Where(a => a.ShopId == shopId);
            }
            var list = data.Select(a => new { Amount = (a.ProductTotalAmount + a.Freight + a.Tax - a.DiscountAmount), PayDate = a.PayDate }).ToList<dynamic>();

            int days = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);//本月天数
            string[] arr = new string[days];
            for (int i = 0; i <= days - 1; i++)
            {
                arr[i] = start.Date.AddDays(i).ToString("MM/dd");
            }

            chart.XAxisData = arr;

            var chartItem = new ChartSeries<decimal> { Name = "交易额走势图", Data = new decimal[days + 1] };
            var ExpandProp = new string[days + 1];

            for (int i = 0; i <= days; i++)
            {
                var date = start.Date.AddDays(i).Date;

                var m = list.Where(a => DateTime.Parse(a.PayDate.ToString("yyyy/MM/dd")) == date).ToList();
                if (m.Count > 0)
                {
                    chartItem.Data[i] = m.Sum(a => (decimal)a.Amount);
                }
                else
                {
                    chartItem.Data[i] = 0;
                }
            }
            chart.SeriesData.Add(chartItem);

            return chart;
        }

        /// <summary>
        /// 更新提现数据
        /// </summary>
        /// <param name="model"></param>
        public void UpdateShopWithDraw(ShopWithDrawInfo model)
        {
            //Context.ShopWithDrawInfo.Attach(model);
            //Context.Entry(model).State = System.Data.Entity.EntityState.Modified;
            //Context.SaveChanges();
            DbFactory.Default.Update(model);
        }

        /// <summary>
        /// 分页获取店铺提现记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<ShopWithDrawInfo> GetShopWithDraw(WithdrawQuery query)
        {
            var db = WhereBuilder(query);
            switch (query.Sort.ToLower())
            {
                case "dealtime":
                    if (query.IsAsc) db.OrderBy(p => p.DealTime);
                    else db.OrderByDescending(p => p.DealTime);
                    break;
                case "applytime":
                    if (query.IsAsc) db.OrderBy(p => p.ApplyTime);
                    else db.OrderByDescending(p => p.ApplyTime);
                    break;
                case "cashamount":
                    if (query.IsAsc) db.OrderBy(p => p.CashAmount);
                    else db.OrderByDescending(p => p.CashAmount);
                    break;
                default:
                    db.OrderByDescending(o => o.Id);
                    break;
            }
            var models = db.ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<ShopWithDrawInfo>() { Models = models, Total = models.TotalRecordCount };
        }

        /// <summary>
        /// 获取店铺提现记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<ShopWithDrawInfo> GetAllShopWithDraw(WithdrawQuery query)
        {
            var pendingQuery = WhereBuilder(query);
            return pendingQuery.OrderByDescending(a => a.Id).ToList();
        }


        public int GetShopWithDrawCount(WithdrawQuery query)
        {
            var db = WhereBuilder(query);
            return db.Count();
        }
        /// <summary>
        /// 返回商家冻结金额
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public decimal GetFrozenAmount(long shopId)
        {
            return DbFactory.Default.Get<ShopWithDrawInfo>().Where(s => s.ShopId == shopId && (s.Status == WithdrawStaus.WatingAudit || s.Status == WithdrawStaus.PayPending)).Sum<decimal>(s => s.CashAmount);
        }

        /// <summary>
        /// 添加待结算订单
        /// </summary>
        /// <param name="model"></param>
        public void AddPendingSettlementOrders(PendingSettlementOrderInfo model)
        {
            //Context.PendingSettlementOrdersInfo.Add(model);
            //Context.SaveChanges();
            DbFactory.Default.Add(model);
        }

        /// <summary>
        /// 获取待结算订单详情
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public PendingSettlementOrderInfo GetPendingSettlementOrderDetail(long orderId)
        {
            //var model = Context.PendingSettlementOrdersInfo.Where(a => a.OrderId == orderId).FirstOrDefault();
            //return model;
            return DbFactory.Default.Get<PendingSettlementOrderInfo>().Where(a => a.OrderId == orderId).FirstOrDefault();
        }

        /// <summary>
        /// 已结算详情
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public AccountDetailInfo GetSettlementOrderDetail(long orderId)
        {
            //var model = Context.AccountDetailInfo.Where(a => a.OrderId == orderId).FirstOrDefault();
            //return model;
            return DbFactory.Default.Get<AccountDetailInfo>().Where(a => a.OrderId == orderId).FirstOrDefault();
        }

        /// <summary>
        /// 获取上次结算的结束日期
        /// </summary>
        /// <returns></returns>
        public DateTime? GetLastSettlementTime()
        {
            DateTime? last = null;
            //已结算表查找最后一次结算记录
            //var settlementOrder = Context.AccountInfo.OrderByDescending(a => a.Id).FirstOrDefault();
            var settlementOrder = DbFactory.Default.Get<AccountInfo>().OrderByDescending(a => a.Id).FirstOrDefault();
            if (settlementOrder != null)
            {
                last = settlementOrder.AccountDate.Date;
            }
            else
            {
                //var order = Context.OrderInfo.Where(a => a.OrderStatus == OrderInfo.OrderOperateStatus.Finish).FirstOrDefault();
                var order = DbFactory.Default.Get<OrderInfo>().Where(a => a.OrderStatus == OrderInfo.OrderOperateStatus.Finish).FirstOrDefault();
                if (order != null)
                {
                    last = order.FinishDate.Value.Date;
                }
            }
            return last;
        }

        /// <summary>
        /// 分页获取待结算订单
        /// </summary>
        /// <param name="query">待结算订单查询实体</param>
        /// <returns></returns>
        public QueryPageModel<PendingSettlementOrderInfo> GetPendingSettlementOrders(PendingSettlementOrderQuery query)
        {
            //int total = 0;
            var pendingQuery = ToWhere(query);
            //pendingQuery = pendingQuery.GetPage(out total, d => d.OrderByDescending(o => o.Id), query.PageNo, query.PageSize);
            var models = pendingQuery.OrderByDescending(o => o.Id).ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<PendingSettlementOrderInfo>() { Models = models, Total = models.TotalRecordCount };
        }

        /// <summary>
        /// 统计待结算订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<StatisticsPendingSettlement> StatisticsPendingSettlementOrders(StatisticsPendingSettlementQuery query)
        {
            var db = DbFactory.Default.Get<PendingSettlementOrderInfo>();
            if (!string.IsNullOrEmpty(query.ShopName))
            {
                List<ShopInfo> infos = DbFactory.Default.Get<ShopInfo>().Where(s => s.ShopName.Contains(query.ShopName)).ToList();
                var shopIds = infos.Select(a => a.Id);
                db = db.Where(p => p.ShopId.ExIn(shopIds));
            }
            db = db.GroupBy(p => p.ShopId).Select(p => new { ShopId = p.ShopId, ShopName = p.ShopName, Amount = p.SettlementAmount.ExSum() });

            switch (query.Sort.ToLower())
            {
                case "amount":
                    if (query.IsAsc) db.OrderBy(p => p.SettlementAmount.ExSum());
                    else db.OrderByDescending(p => p.SettlementAmount.ExSum());
                    break;
                default:
                    db.OrderByDescending(p => p.SettlementAmount.ExSum());
                    break;
            }
            var data = db.ToPagedList<StatisticsPendingSettlement>(query.PageNo, query.PageSize);
            var result = new QueryPageModel<StatisticsPendingSettlement>
            {
                Models = data,
                Total = data.TotalRecordCount
            };
            return result;
        }

        /// <summary>
        /// 统计待结算订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<StatisticsPendingSettlement> AllStatisticsPendingSettlementOrders(StatisticsPendingSettlementQuery query)
        {
            //var data = this.Context.PendingSettlementOrdersInfo.AsQueryable();
            var data = DbFactory.Default.Get<PendingSettlementOrderInfo>();

            if (!string.IsNullOrEmpty(query.ShopName))
            {
                List<ShopInfo> infos = DbFactory.Default.Get<ShopInfo>().Where(s => s.ShopName.Contains(query.ShopName)).ToList();
                var shopIds = infos.Select(a => a.Id);
                data.Where(p => p.ShopName.ExIn(shopIds));
            }

            //var models = data.GroupBy(p => p.ShopId).Select(p => new StatisticsPendingSettlement
            //{
            //    ShopId = p.Key,
            //    Amount = p.Sum(pp => pp.SettlementAmount)
            //});
            //return models.ToList();
            var models = data.GroupBy(p => p.ShopId).Select(p => new { ShopId = p.ShopId, Amount = p.SettlementAmount.ExSum() }).ToList<StatisticsPendingSettlement>();
            return models;
        }

        /// <summary>
        /// 获取待结算订单
        /// </summary>
        /// <param name="query">待结算订单查询实体</param>
        /// <returns></returns>
        public List<PendingSettlementOrderInfo> GetAllPendingSettlementOrders(PendingSettlementOrderQuery query)
        {
            var pendingQuery = ToWhere(query);
            return pendingQuery.OrderByDescending(o => o.Id).ToList();
        }

        /// <summary>
        /// 分页获取已结算订单
        /// </summary>
        /// <param name="query">结算订单查询实体</param>
        /// <returns></returns>
        public QueryPageModel<AccountDetailInfo> GetSettlementOrders(SettlementOrderQuery query)
        {
            var accountDetailQuery = ToWhere(query);
            var models = accountDetailQuery.OrderByDescending(o => o.Id).ToPagedList(query.PageNo, query.PageSize);
            QueryPageModel<AccountDetailInfo> pageModel = new QueryPageModel<AccountDetailInfo>() { Models = models, Total = models.TotalRecordCount };
            return pageModel;
        }

        /// <summary>
        /// 获取已结算订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<AccountDetailInfo> GetAllSettlementOrders(SettlementOrderQuery query)
        {
            var accountDetailQuery = ToWhere(query);
            return accountDetailQuery.ToList();
        }

        public void Settle()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 处理余额私有方法
        /// </summary>
        /// <param name="shopId">店铺ID</param>
        /// <param name="money">金额</param>
        /// <param name="TradeType">类别</param>
        /// <param name="AccountNo">交易流水号</param>
        /// <param name="ChargeWay">备注</param>
        /// <param name="AccoutID">关联资金编号</param>
        public void UpdateAccount(long shopId, decimal money, ShopAccountType TradeType, string AccountNo, string ChargeWay, long detailID = 0)
        {
            lock (obj)
            {
                //处理余额
                var mShopAccountInfo = GetShopAccount(shopId);
                mShopAccountInfo.Balance += money;
                UpdateShopAccount(mShopAccountInfo);
                var isincome = true;
                if (TradeType == ShopAccountType.Refund || TradeType == ShopAccountType.MarketingServices || TradeType == ShopAccountType.WithDraw)
                {
                    isincome = false;
                }
                //处理充值记录
                ShopAccountItemInfo mShopAccountItemInfo = new ShopAccountItemInfo()
                {
                    AccountNo = AccountNo,
                    AccoutID = mShopAccountInfo.Id,
                    Amount = Math.Abs(money),
                    Balance = mShopAccountInfo.Balance,
                    CreateTime = DateTime.Now,
                    DetailId = detailID.ToString(),
                    IsIncome = isincome,
                    ReMark = ChargeWay,
                    ShopId = shopId,
                    ShopName = mShopAccountInfo.ShopName,
                    TradeType = TradeType
                };

                ///平台佣金退还
                if (TradeType == ShopAccountType.PlatCommissionRefund)
                {
                    var platAccount = GetPlatAccount();
                    platAccount.Balance -= money;
                    UpdatePlatAccount(platAccount);
                    var info = new PlatAccountItemInfo()
                    {
                        AccountNo = AccountNo,
                        AccoutID = platAccount.Id,
                        Amount = Math.Abs(money),
                        Balance = platAccount.Balance,
                        CreateTime = DateTime.Now,
                        DetailId = detailID.ToString(),
                        IsIncome = false,
                        ReMark = ChargeWay,
                        TradeType = PlatAccountType.PlatCommissionRefund
                    };
                    DbFactory.Default.Add(info);
                }
                AddShopAccountItem(mShopAccountItemInfo);
            }
        }

        public void UpdatePlatAccount(PlatAccountInfo model)
        {
            //Context.PlatAccountInfo.Attach(model);
            //Context.Entry(model).State = System.Data.Entity.EntityState.Modified;
            //Context.SaveChanges();
            DbFactory.Default.Update(model);
        }

        /// <summary>
        /// 获取平台佣金总额
        /// </summary>
        /// <param name="shopId">店铺ID选填</param>
        /// <returns></returns>
        public decimal GetPlatCommission(long? shopId = null, long? accountId = null)
        {
            //var model = Context.AccountDetailInfo.AsQueryable();
            var model = DbFactory.Default.Get<AccountDetailInfo>();
            if (shopId.HasValue)
                model.Where(a => a.ShopId == shopId);
            if (accountId.HasValue)
                model.Where(p => p.AccountId == accountId.Value);
            decimal amount = 0;
            amount = model.Sum<decimal>(a => (a.CommissionAmount));
            return amount;
        }

        /// <summary>
        /// 获取分销佣金总额
        /// </summary>
        /// <param name="shopId">店铺ID选填</param>
        /// <returns></returns>

        public decimal GetDistributorCommission(long? shopId = null, long? accountId = null)
        {
            //var model = Context.AccountDetailInfo.AsQueryable();
            var model = DbFactory.Default.Get<AccountDetailInfo>();
            if (shopId.HasValue)
                model.Where(a => a.ShopId == shopId);
            if (accountId.HasValue)
                model.Where(p => p.AccountId == accountId.Value);
            decimal amount = model.Sum<decimal>(a => (a.BrokerageAmount));
            return amount;
        }

        /// <summary>
        /// 获取结算总金额
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public decimal GetSettlementAmount(long? shopId = null, long? accountId = null)
        {
            //var model = Context.AccountDetailInfo.AsQueryable();
            var model = DbFactory.Default.Get<AccountDetailInfo>();
            if (shopId.HasValue)
                model.Where(a => a.ShopId == shopId);
            if (accountId.HasValue)
                model.Where(p => p.AccountId == accountId.Value);
            decimal amount = model.Sum<decimal>(a => a.SettlementAmount);
            return amount;
        }

        /// <summary>
        /// 获取平台待结算佣金总和(平台待结算页面汇总)
        /// </summary>
        /// <returns></returns>
        public decimal GetPendingPlatCommission()
        {
            //var pendingPlatCommission = Context.PendingSettlementOrdersInfo.Sum(a => (decimal?)a.PlatCommission - a.PlatCommissionReturn).GetValueOrDefault();
            var pendingPlatCommission = DbFactory.Default.Get<PendingSettlementOrderInfo>().Sum<decimal>(a => (a.PlatCommission));
            return pendingPlatCommission;
        }

        /// <summary>
        /// 获取平台帐户信息
        /// </summary>
        /// <returns></returns>
        public PlatAccountInfo GetPlatAccount()
        {
            //var model = Context.PlatAccountInfo.FirstOrDefault();
            var model = DbFactory.Default.Get<PlatAccountInfo>().FirstOrDefault();
            #region 当新状态没值时，初始一个(因为之前代码有好些地方要用到PlatAccount但它没初始值报错，这里数据库里初始一条数)
            if (model == null)
            {
                model = new PlatAccountInfo();
                model.Balance = 0;
                model.PendingSettlement = 0;
                model.Settled = 0;

                DbFactory.Default.Add(model);
            }
            #endregion
            //待结算金额总在变化改为实时查询吧20160709
            //var PendingSettlementAmount = Context.PendingSettlementOrdersInfo.Sum(a => (decimal?)a.SettlementAmount).GetValueOrDefault();
            var PendingSettlementAmount = DbFactory.Default.Get<PendingSettlementOrderInfo>().Sum<decimal>(a => a.SettlementAmount);
            model.PendingSettlement = PendingSettlementAmount;
            return model;
        }

        /// <summary>
        /// 提现成功
        /// </summary>
        /// <param name="Id"></param>
        public void SetShopWithDrawSuccess(long Id)
        {
            var obj = DbFactory.Default.Get<ShopWithDrawInfo>(e => e.Id == Id);
            DbFactory.Default.Set<ShopWithDrawInfo>()
                .Set(e => e.Status, WithdrawStaus.Succeed)
                .Where(e => e.Id == Id)
                .Succeed();
            //var obj= Context.ShopWithDrawInfo.FirstOrDefault(d=>d.Id== Id);
            // obj.Status = WithdrawStaus.Succeed;
            // Context.SaveChanges();
        }
        #endregion

        #region 私有方法
        private GetBuilder<AccountDetailInfo> ToWhere(SettlementOrderQuery query)
        {
            //var accountDetailQuery = Context.AccountDetailInfo.AsQueryable();
            var accountDetailQuery = DbFactory.Default.Get<AccountDetailInfo>();
            if (query.ShopId.HasValue)
            {
                accountDetailQuery.Where(a => a.ShopId == query.ShopId.Value);
            }
            if (query.OrderId.HasValue)
            {
                accountDetailQuery.Where(a => a.OrderId == query.OrderId.Value);
            }
            if (query.OrderStart.HasValue)
            {
                accountDetailQuery.Where(a => a.OrderFinshDate >= query.OrderStart.Value);
            }
            if (query.OrderEnd.HasValue)
            {
                var end = query.OrderEnd.Value.Date.AddDays(1);
                accountDetailQuery.Where(a => a.OrderFinshDate < end);
            }
            if (query.SettleStart.HasValue)
            {
                accountDetailQuery.Where(a => a.Date >= query.SettleStart.Value);
            }
            if (query.SettleEnd.HasValue)
            {
                var end = query.SettleEnd.Value.Date.AddDays(1);
                accountDetailQuery.Where(a => a.Date < end);
            }

            if (!string.IsNullOrEmpty(query.ShopName))
            {
                accountDetailQuery.Where(a => a.ShopName.Contains(query.ShopName));
            }
            if (!string.IsNullOrEmpty(query.PaymentName))
            {
                if (string.Equals(query.PaymentName, "组合支付"))
                {
                    accountDetailQuery.Where(a => a.PaymentTypeName.Contains("+"));
                }
                else
                {
                    accountDetailQuery.Where(a => a.PaymentTypeName.Contains(query.PaymentName));
                }
            }
            if (query.WeekSettlementId.HasValue && query.WeekSettlementId.Value != 0)
            {
                var detailId = query.WeekSettlementId.Value;
                accountDetailQuery.Where(a => a.AccountId == detailId);
            }
            return accountDetailQuery;
        }

        private GetBuilder<PendingSettlementOrderInfo> ToWhere(PendingSettlementOrderQuery query)
        {
            //var pendingQuery = Context.PendingSettlementOrdersInfo.AsQueryable();
            var pendingQuery = DbFactory.Default.Get<PendingSettlementOrderInfo>();
            if (query.ShopId.HasValue)
            {
                pendingQuery.Where(a => a.ShopId == query.ShopId.Value);
            }
            if (query.OrderId.HasValue)
            {
                pendingQuery.Where(a => a.OrderId == query.OrderId.Value);
            }
            if (query.OrderStart.HasValue)
            {
                pendingQuery.Where(a => a.OrderFinshTime >= query.OrderStart.Value);
            }
            if (query.OrderEnd.HasValue)
            {
                var end = query.OrderEnd.Value.Date.AddDays(1);
                pendingQuery.Where(a => a.OrderFinshTime < end);
            }
            if (query.CreateDateStart.HasValue)
            {
                pendingQuery.Where(a => a.CreateDate >= query.CreateDateStart.Value.Date);
            }
            if (query.CreateDateEnd.HasValue)
            {
                var end = query.OrderEnd.Value.Date.AddDays(1);
                pendingQuery.Where(a => a.CreateDate < end);
            }
            if (!string.IsNullOrEmpty(query.ShopName))
            {
                pendingQuery.Where(a => a.ShopName.Contains(query.ShopName));
            }
            if (query.ShopId.HasValue)
                pendingQuery.Where(p => p.ShopId == query.ShopId.Value);
            if (!string.IsNullOrEmpty(query.PaymentName))
            {
                if (string.Equals(query.PaymentName, "组合支付"))
                {
                    pendingQuery.Where(a => a.PaymentTypeName.Contains("+"));
                }
                else
                {
                    pendingQuery.Where(a => a.PaymentTypeName.Contains(query.PaymentName));
                }
            }
            return pendingQuery;
        }

        private GetBuilder<ShopAccountItemInfo> ToWhere(ShopAccountItemQuery query)
        {
            var itemQuery = DbFactory.Default.Get<ShopAccountItemInfo>();
            if (query.ShopId.HasValue)
            {
                itemQuery.Where(a => a.ShopId == query.ShopId.Value);
            }
            if (!string.IsNullOrEmpty(query.ShopName))
            {
                List<ShopInfo> infos = DbFactory.Default.Get<ShopInfo>().Where(s => s.ShopName.Contains(query.ShopName)).ToList();
                var shopIds = infos.Select(a => a.Id);
                itemQuery.Where(p => p.ShopId.ExIn(shopIds));
            }
            if (query.IsIncome.HasValue)
            {
                itemQuery.Where(a => a.IsIncome == query.IsIncome.Value);
            }
            if (query.ShopAccountType.HasValue)
            {
                itemQuery.Where(a => a.TradeType == query.ShopAccountType.Value);
            }
            if (query.TimeStart.HasValue)
            {
                itemQuery.Where(a => a.CreateTime >= query.TimeStart.Value);
            }
            if (query.TimeEnd.HasValue)
            {
                var end = query.TimeEnd.Value.Date.AddDays(1);
                itemQuery.Where(a => a.CreateTime < end);
            }
            return itemQuery;
        }

        private GetBuilder<ShopWithDrawInfo> WhereBuilder(WithdrawQuery query)
        {
            var pendingQuery = DbFactory.Default.Get<ShopWithDrawInfo>();

            if (query.ApplyStartTime.HasValue)
                pendingQuery.Where(a => a.ApplyTime >= query.ApplyStartTime.Value);

            if (query.ApplyEndTime.HasValue)
            {
                var end = query.ApplyEndTime.Value.Date.AddDays(1);
                pendingQuery.Where(a => a.ApplyTime < end);
            }
            if (query.AuditedStartTime.HasValue)
            {
                pendingQuery.Where(a => a.DealTime >= query.AuditedStartTime.Value);
            }
            if (query.AuditedEndTime.HasValue)
            {
                var end = query.AuditedEndTime.Value.Date.AddDays(1);
                pendingQuery.Where(a => a.DealTime < end);
            }

            if (query.ShopId.HasValue)
            {
                pendingQuery.Where(a => a.ShopId == query.ShopId.Value);
            }
            if (!string.IsNullOrEmpty(query.ShopName))
            {
                pendingQuery.Where(a => a.ShopName.Contains(query.ShopName));
            }
            if (query.Status.HasValue)
            {
                pendingQuery.Where(a => a.Status == query.Status.Value);
            }
            if (query.Id.HasValue && query.Id.Value != 0)
            {
                pendingQuery.Where(a => a.Id == query.Id.Value);
            }
            if (query.Ids != null && query.Ids.Length > 0)
            {
                pendingQuery.Where(a => a.Id.ExIn(query.Ids));
            }
            return pendingQuery;
        }

        private GetBuilder<PlatAccountItemInfo> ToWhere(PlatAccountItemQuery query)
        {
            var ItemQuery = DbFactory.Default.Get<PlatAccountItemInfo>();
            if (query.PlatAccountType.HasValue)
            {
                ItemQuery.Where(a => a.TradeType == query.PlatAccountType.Value);
            }
            if (query.TimeStart.HasValue)
            {
                ItemQuery.Where(a => a.CreateTime >= query.TimeStart.Value);
            }
            if (query.TimeEnd.HasValue)
            {
                var end = query.TimeEnd.Value.Date.AddDays(1);
                ItemQuery.Where(a => a.CreateTime < end);
            }
            return ItemQuery;
        }

        public decimal GetLastSettlementByShopId(long shopId)
        {
            //var last = Context.ShopAccountItemInfo.Where(a => a.TradeType == ShopAccountType.SettlementIncome && a.ShopId == shopId).OrderByDescending(a => a.Id).FirstOrDefault();
            var last = DbFactory.Default.Get<ShopAccountItemInfo>().Where(a => a.TradeType == ShopAccountType.SettlementIncome && a.ShopId == shopId).OrderByDescending(a => a.Id).FirstOrDefault();
            if (last == null)
                return 0;
            else
                return last.Amount;
        }

        /// <summary>
        /// 获取某个店铺某个结算周期的结算总金额
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public decimal GetShopSettledAmountByAccountId(long shopId, long accountId)
        {
            //var SettlementAmount = Context.AccountDetailInfo.Where(a => a.ShopId == shopId && a.AccountId == accountId).Sum(a => (decimal?)a.SettlementAmount).GetValueOrDefault();
            return DbFactory.Default.Get<AccountDetailInfo>().Where(a => a.ShopId == shopId && a.AccountId == accountId).Sum<decimal>(a => a.SettlementAmount);

        }


        /// <summary>
        /// 获取上一次结算的基本信息
        /// </summary>
        /// <returns></returns>
        public AccountInfo GetLastAccountInfo()
        {
            //var model = Context.AccountInfo.OrderByDescending(a => a.Id).FirstOrDefault();
            var model = DbFactory.Default.Get<AccountInfo>().OrderByDescending(a => a.Id).FirstOrDefault();

            if (model == null)
                model = new AccountInfo();
            return model;
        }

        /// <summary>
        /// 查询结算历史
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<AccountInfo> GetSettledHistory(ShopSettledHistoryQuery query)
        {
            //int total = 0;
            //var model = Context.AccountDetailInfo.Where(a => a.ShopId == query.ShopId).OrderBy(a => a.Id).FirstOrDefault();
            var model = DbFactory.Default.Get<AccountDetailInfo>().Where(a => a.ShopId == query.ShopId).OrderBy(a => a.Id).FirstOrDefault();
            var MinDate = query.MinSettleTime.Date;
            if (model == null) //不存在结算记录返回空
                return new QueryPageModel<AccountInfo>() { Models = new List<AccountInfo>(), Total = 0 };
            else
            {
                MinDate = model.Date.Date;
            }
            //var list = Context.AccountInfo.Where(a => a.AccountDate >= MinDate);
            var list = DbFactory.Default.Get<AccountInfo>().Where(a => a.AccountDate >= MinDate);
            var result = list.OrderByDescending(o => o.Id).ToPagedList(query.PageNo, query.PageSize);
            QueryPageModel<AccountInfo> pageModel = new QueryPageModel<AccountInfo>() { Models = result, Total = result.TotalRecordCount };
            return pageModel;
        }
        #endregion
    }
}


