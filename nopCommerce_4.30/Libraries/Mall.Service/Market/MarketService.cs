using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Linq;

namespace Mall.Service
{
    public class MarketService : ServiceBase, IMarketService
    {
        public ActiveMarketServiceInfo GetMarketService(long shopId, MarketType type)
        {
            var market = DbFactory.Default.Get<ActiveMarketServiceInfo>().Where(m => m.TypeId == type && m.ShopId == shopId).FirstOrDefault();
            if (market == null) {
                market = new ActiveMarketServiceInfo
                {
                    ShopId = shopId,
                    TypeId = type,
                };
            }
            return market;
        }

        public ActiveMarketServiceInfo GetMarketService(long id)
        {
            return DbFactory.Default.Get<ActiveMarketServiceInfo>().Where(p => p.Id == id).FirstOrDefault();
        }

        public MarketSettingInfo GetServiceSetting(MarketType type)
        {
            return DbFactory.Default.Get<MarketSettingInfo>().Where(m => m.TypeId == type).FirstOrDefault();
        }

        public QueryPageModel<MarketServiceRecordInfo> GetBoughtShopList(MarketBoughtQuery query)
        {
            var db = DbFactory.Default.Get<MarketServiceRecordInfo>()
                .InnerJoin<ActiveMarketServiceInfo>((m, a) => m.MarketServiceId == a.Id);

            if (query.MarketType.HasValue)
                db.Where<ActiveMarketServiceInfo>(d => d.TypeId == query.MarketType.Value);

            if (!string.IsNullOrWhiteSpace(query.ShopName))
                db = db.Where<ActiveMarketServiceInfo>(d => d.ShopName.Contains(query.ShopName));

            switch (query.Sort.ToLower())
            {
                case "startdate":
                case "starttime":
                    if (query.IsAsc) db.OrderBy(p => p.StartTime);
                    else db.OrderByDescending(p => p.StartTime);
                    break;
                case "enddate":
                case "endtime":
                    if (query.IsAsc) db.OrderBy(p => p.EndTime);
                    else db.OrderByDescending(p => p.EndTime);
                    break;
                default:
                    db.OrderByDescending(o => o.Id);
                    break;

            }
            var data = db.ToPagedList(query.PageNo, query.PageSize);

            return new QueryPageModel<MarketServiceRecordInfo>()
            {
                Models = data,
                Total = data.TotalRecordCount
            };
        }

        /// <summary>
        ///根据ID获取服务购买记录
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public MarketServiceRecordInfo GetShopMarketServiceRecordInfo(long Id)
        {
            return DbFactory.Default.Get<MarketServiceRecordInfo>().Where(a => a.Id == Id).FirstOrDefault();
        }

        public DateTime GetServiceEndTime(long marketId)
        {
            var record = DbFactory.Default.Get<MarketServiceRecordInfo>().Where(p => p.MarketServiceId == marketId).OrderByDescending(p => p.EndTime).FirstOrDefault();
            if (record == null) return DateTime.MinValue;
            return record.EndTime;
        }

        public decimal GetLastBuyPrice(long marketId)
        {
            var record = DbFactory.Default.Get<MarketServiceRecordInfo>().Where(p => p.MarketServiceId == marketId).OrderByDescending(p => p.EndTime).FirstOrDefault();
            if (record == null) return -1;
            return record.Price;
        }

        public void AddOrUpdateServiceSetting(MarketSettingInfo info)
        {
            var model = DbFactory.Default.Get<MarketSettingInfo>().Where(a => a.TypeId == info.TypeId).FirstOrDefault();

            if (model == null)
            {
                DbFactory.Default.Add(info);
            }
            else
            {
                model.Price = info.Price;
                DbFactory.Default.Update(model);
            }
        }


        public void OrderMarketService(int monthCount, long shopId, MarketType type)
        {
            if (shopId <= 0)
            {
                throw new MallException("ShopId不能识别");
            }
            if (monthCount <= 0)
            {
                throw new MallException("购买服务时长必须大于零");
            }
            var shop = DbFactory.Default.Get<ShopInfo>().Where(p => p.Id == shopId).FirstOrDefault();
            if (shop == null || shopId <= 0)
            {
                throw new MallException("ShopId不能识别");
            }
            
            var price = DbFactory.Default.Get<MarketSettingInfo>().Where(a => a.TypeId == type).Select(a => a.Price).FirstOrDefault<decimal>();
            var StartTime = DateTime.Now;
            MarketServiceRecordInfo model = new MarketServiceRecordInfo();
            model.StartTime = StartTime;
            model.Price = price * monthCount;
            var shopAccount = DbFactory.Default.Get<ShopAccountInfo>().Where(a => a.ShopId == shopId).FirstOrDefault();//店铺帐户信息
            #region 它下面会取几次只，如为空会报异常，默认存入0的初始值
            if (shopAccount == null)
            {
                shopAccount = new ShopAccountInfo();
                shopAccount.ShopId = shopId;
                shopAccount.ShopName = shop.ShopName;
                shopAccount.Balance = 0;
                shopAccount.PendingSettlement = 0;
                shopAccount.Settled = 0;
                shopAccount.ReMark = string.Empty;
                DbFactory.Default.Add(shopAccount);
            }
            #endregion

            if (shopAccount.Balance < model.Price) //店铺余额不足以支付服务费用
            {
                throw new MallException("您的店铺余额为：" + shopAccount.Balance + "元,不足以支付此次营销服务购买费用，请先充值。");
            }

            DbFactory.Default.InTransaction(() =>
            {
                var market = DbFactory.Default.Get<ActiveMarketServiceInfo>().Where(a => a.ShopId == shopId && a.TypeId == type).FirstOrDefault();
                if (market != null)
                {

                    var maxTime = MarketApplication.GetServiceEndTime(market.Id);
                    if (maxTime > DateTime.Now) //如果结束时间大于当前时间，续费从结束时间加上购买月数，否则从当前时间加上购买月数
                        StartTime = maxTime;
                    model.StartTime = StartTime;
                    model.BuyTime = DateTime.Now;
                    model.EndTime = StartTime.AddMonths(monthCount);
                    // model.MarketServiceId = market.Id;
                    model.SettlementFlag = 1;
                    model.MarketServiceId = market.Id;

                    DbFactory.Default.Add(model);
                }
                else
                {
                    model.StartTime = StartTime;
                    model.EndTime = StartTime.AddMonths(monthCount);
                    model.SettlementFlag = 1;
                    model.BuyTime = DateTime.Now;

                    ActiveMarketServiceInfo activeMarketServiceInfo = new ActiveMarketServiceInfo();
                    activeMarketServiceInfo.ShopId = shopId;
                    activeMarketServiceInfo.ShopName = shop.ShopName;
                    activeMarketServiceInfo.TypeId = type;
                    DbFactory.Default.Add(activeMarketServiceInfo);

                    model.MarketServiceId = activeMarketServiceInfo.Id;
                    DbFactory.Default.Add(model);
                }

                var ShopAccount = DbFactory.Default.Get<ShopAccountInfo>().Where(a => a.ShopId == shopId).FirstOrDefault();
                ShopAccountItemInfo info = new ShopAccountItemInfo();
                info.IsIncome = false;
                info.ShopId = shopId;
                info.DetailId = model.Id.ToString();
                info.ShopName = shop.ShopName;
                info.AccountNo = shopId + info.DetailId + new Random().Next(10000);
                info.ReMark = "店铺购买" + type.ToDescription() + "服务," + monthCount + "个月";
                info.TradeType = ShopAccountType.MarketingServices;
                info.CreateTime = DateTime.Now;
                info.Amount = price * monthCount;
                info.AccoutID = ShopAccount.Id;
                ShopAccount.Balance -= info.Amount;//总余额减钱
                info.Balance = ShopAccount.Balance;//变动后当前剩余金额

                DbFactory.Default.Add(info);
                DbFactory.Default.Update(ShopAccount);

                var platAccount = DbFactory.Default.Get<PlatAccountInfo>().FirstOrDefault();
                var pinfo = new PlatAccountItemInfo
                {
                    IsIncome = true,
                    DetailId = model.Id.ToString(),
                    AccountNo = info.AccountNo,
                    ReMark = "店铺购买" + type.ToDescription() + "服务," + monthCount + "个月",
                    TradeType = PlatAccountType.MarketingServices,
                    CreateTime = DateTime.Now,
                    Amount = price * monthCount,
                    AccoutID = platAccount.Id
                };
                platAccount.Balance += info.Amount;//总余额加钱
                pinfo.Balance = platAccount.Balance;//变动后当前剩余金额
                DbFactory.Default.Add(pinfo);
                DbFactory.Default.Update(platAccount);
            });
        }
    }
}
