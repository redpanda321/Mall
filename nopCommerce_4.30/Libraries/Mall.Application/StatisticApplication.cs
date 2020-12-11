using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mall.Core;
using Mall.Core.Helper;
using Mall.IServices;
using Mall.CommonModel;
using Mall.Entities;
using Mall.DTO;
using AutoMapper;
using Mall.DTO.QueryModel;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    /// <summary>
    /// 统计应用服务
    /// </summary>
    public class StatisticApplication : BaseApplicaion
    {
        private static IStatisticsService _StatisticsService =  EngineContext.Current.Resolve<IStatisticsService>();

        #region 前台统计
        /// <summary>
        /// 商品访问锁
        /// </summary>
        static Dictionary<long, object> _productVisitLockerDict = new Dictionary<long, object>();
        /// <summary>
        /// 平台统计锁
        /// </summary>
        static object _platVisitLocker = new object();
        static object _productVisitLocker = new object();
        static object _shopVisitLocker = new object();
        /// <summary>
        /// 店铺统计锁
        /// </summary>
        static Dictionary<long, object> _shopVisitLockerDict = new Dictionary<long, object>();

        /// <summary>
        /// 1、商品流量统计
        /// 2、商品浏览人数统计
        /// 3、店铺浏览人数统计（浏览商品时需统计店铺，所以组合在一起）
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="shopid"></param>
        public static void StatisticVisitCount(long pid, long shopid)
        {
            StatisticShopVisitUserCount(shopid);//店铺浏览人数
            StatisticProductVisitCount(pid, shopid);//商品浏览量
            StatisticProductVisitUserCount(pid, shopid);//商品浏览人数
        }

        /// <summary>
        /// 统计平台访问人数(cookie机制)
        /// 按天统计，一天内计一次
        /// </summary>
        public static void StatisticPlatVisitUserCount()
        {
            var platVisitTimestamp = WebHelper.GetCookie(CommonConst.MALL_PLAT_VISIT_COUNT);
            var nowTicks = DateTime.Now.Date.Ticks;
            if (!string.IsNullOrWhiteSpace(platVisitTimestamp))
            {
                var statisticTimestamp = Decrypt(platVisitTimestamp);//解密
                long ticks = 0;
                if (!long.TryParse(statisticTimestamp, out ticks))
                {//格式异常
                    return;
                }
                if (ticks >= nowTicks)
                {//有今天的统计cookie，说明已经统计
                    return;
                }
            }
            var nowDate = new DateTime(nowTicks);
            //没有今天的统计cookie，则统计数加1
            Task.Factory.StartNew(() =>
            {
                lock (_platVisitLocker)
                {
                    int count = 1;
                    if (Core.Cache.Exists(CommonConst.MALL_PLAT_VISIT_COUNT))
                    {
                        count = Core.Cache.Get<int>(CommonConst.MALL_PLAT_VISIT_COUNT);
                        count++;
                    }
                    if (count >= CommonConst.MALL_PLAT_VISIT_COUNT_MAX)
                    {
                        _StatisticsService.AddPlatVisitUser(nowDate, count);
                        count = 0;
                    }
                    Core.Cache.Insert(CommonConst.MALL_PLAT_VISIT_COUNT, count);
                }
            });

            //加密
            platVisitTimestamp = Encrypt(nowTicks.ToString());
            WebHelper.SetCookie(CommonConst.MALL_PLAT_VISIT_COUNT, platVisitTimestamp);
        }

        /// <summary>
        /// 统计店铺访问人数(cookie机制)
        /// 按天统计，一天内计一次
        /// </summary>
        /// <param name="shopId"></param>
        public static void StatisticShopVisitUserCount(long shopId)
        {
            var cookieKey = CommonConst.MALL_SHOP_VISIT_COUNT(shopId.ToString());
            var shopVisit = WebHelper.GetCookie(cookieKey);
            var nowTicks = DateTime.Now.Date.Ticks;
            if (!string.IsNullOrWhiteSpace(shopVisit))
            {
                var statisticTimestamp = Decrypt(shopVisit);//解密
                long ticks = 0;
                if (!long.TryParse(statisticTimestamp, out ticks))
                {//格式异常
                    return;
                }
                if (ticks >= nowTicks)
                {//有今天的统计cookie，说明已经统计
                    return;
                }
            }
            var nowDate = new DateTime(nowTicks);
            //没有今天的统计cookie，则统计数加1
            Task.Factory.StartNew(() =>
            {
                lock (_shopVisitLocker)
                {
                    if (!_shopVisitLockerDict.Keys.Contains(shopId))
                    {
                        _shopVisitLockerDict.Add(shopId, new object());
                    }
                }
                lock (_shopVisitLockerDict[shopId])
                {
                    string key = CommonConst.MALL_SHOP_VISIT_COUNT(shopId.ToString());
                    int count = 1;
                    if (Core.Cache.Exists(key))
                    {
                        count = Core.Cache.Get<int>(key);
                        count++;
                    }
                    if (count >= CommonConst.MALL_SHOP_VISIT_COUNT_MAX)
                    {
                        _StatisticsService.AddShopVisitUser(nowDate, shopId, count);
                        count = 0;
                    }
                    Core.Cache.Insert(key, count);
                }
            });

            //加密
            shopVisit = Encrypt(nowTicks.ToString());
            WebHelper.SetCookie(cookieKey, shopVisit);
        }

        /// <summary>
        /// 统计店铺访问人数(cookie机制)
        /// 按天统计，一天内计一次
        /// </summary>
        /// <param name="shopBranchId"></param>
        public static void StatisticShopBranchVisitUserCount(long shopId,long shopBranchId)
        {
            var cookieKey = CommonConst.MALL_SHOPBRANCH_VISIT_COUNT(shopBranchId.ToString());
            var shopVisit = WebHelper.GetCookie(cookieKey);
            var nowTicks = DateTime.Now.Date.Ticks;
            if (!string.IsNullOrWhiteSpace(shopVisit))
            {
                var statisticTimestamp = Decrypt(shopVisit);//解密
                long ticks = 0;
                if (!long.TryParse(statisticTimestamp, out ticks))
                {//格式异常
                    return;
                }
                if (ticks >= nowTicks)
                {//有今天的统计cookie，说明已经统计
                    return;
                }
            }
            var nowDate = new DateTime(nowTicks);
            //没有今天的统计cookie，则统计数加1
            Task.Factory.StartNew(() =>
            {
                lock (_shopVisitLocker)
                {
                    if (!_shopVisitLockerDict.Keys.Contains(shopBranchId))
                    {
                        _shopVisitLockerDict.Add(shopBranchId, new object());
                    }
                }
                lock (_shopVisitLockerDict[shopBranchId])
                {
                    string key = CommonConst.MALL_SHOPBRANCH_VISIT_COUNT(shopBranchId.ToString());
                    int count = 1;
                    if (Core.Cache.Exists(key))
                    {
                        count = Core.Cache.Get<int>(key);
                        count++;
                    }
                    if (count >= CommonConst.MALL_SHOPBRANCH_VISIT_COUNT_MAX)
                    {
                        _StatisticsService.AddShopBranchVisitUser(nowDate, shopId, shopBranchId, count);
                        count = 0;
                    }
                    Core.Cache.Insert(key, count);
                }
            });

            //加密
            shopVisit = Encrypt(nowTicks.ToString());
            WebHelper.SetCookie(cookieKey, shopVisit);
        }

        /// <summary>
        /// 统计商品访问人数（cookie机制）
        /// </summary>
        /// <param name="pid"></param>
        public static void StatisticProductVisitUserCount(long pid, long shopId)
        {
            var cookieKey = CommonConst.MALL_PRODUCT_VISIT_COUNT_COOKIE;
            var cookieValue = WebHelper.GetCookie(cookieKey);
            var nowTicks = DateTime.Now.Date.Ticks;
            if (!string.IsNullOrWhiteSpace(cookieValue))
            {
                cookieValue = Decrypt(cookieValue);//解密 格式："时间|商品ID,商品ID,"
                string[] strArray = cookieValue.Split('|');
                if (strArray.Length < 2)//格式异常
                    return;
                var statisticTimestamp = strArray[0];//时间戳
                long ticks = 0;
                if (!long.TryParse(statisticTimestamp, out ticks))
                {//格式异常，为非法数据
                    return;
                }
                if (ticks > nowTicks)
                {//时间戳比当前的大，为非法数据
                    return;
                }
                if (ticks == nowTicks)
                {//时间戳为当天，看是否浏览过些商品
                    if (strArray[1].Contains(pid.ToString() + ","))
                    {//当天已浏览过，不统计
                        return;
                    }
                    cookieValue = cookieValue + pid.ToString() + ",";
                }
                else
                {//没有当天cookie，直接组装数据
                    cookieValue = nowTicks.ToString() + "|" + pid.ToString() + ",";
                }
            }
            else
            {
                cookieValue = nowTicks.ToString() + "|" + pid.ToString() + ",";
            }
            var nowDate = new DateTime(nowTicks);
            //没有今天的统计cookie，则统计数加1
            Task.Factory.StartNew(() =>
            {
                lock (_productVisitLocker)
                {
                    if (!_productVisitLockerDict.Keys.Contains(pid))
                    {
                        _productVisitLockerDict.Add(pid, new object());
                    }
                }
                lock (_productVisitLockerDict[pid])
                {
                    string key = CommonConst.MALL_PRODUCT_VISITUSER_COUNT(pid.ToString());
                    int count = 1;
                    if (Core.Cache.Exists(key))
                    {
                        count = Core.Cache.Get<int>(key);
                        count++;
                    }
                    if (count >= CommonConst.MALL_PRODUCT_VISITUSER_COUNT_MAX)
                    {
                        _StatisticsService.AddProductVisitUser(nowDate, pid, shopId, count);
                        count = 0;
                    }
                    Core.Cache.Insert(key, count);
                }
            });

            //加密
            cookieValue = Encrypt(cookieValue);
            WebHelper.SetCookie(cookieKey, cookieValue);
        }
        /// <summary>
        /// 统计商品访问量
        /// </summary>
        /// <param name="pid"></param>
        public static void StatisticProductVisitCount(long pid, long shopId)
        {
            Task.Factory.StartNew(() =>
            {
                lock (_productVisitLocker)
                {
                    if (!_productVisitLockerDict.Keys.Contains(pid))
                    {
                        _productVisitLockerDict.Add(pid, new object());
                    }
                }
                lock (_productVisitLockerDict[pid])
                {
                    string key = CommonConst.MALL_PRODUCT_VISIT_COUNT(pid.ToString());
                    int count = 1;
                    if (Core.Cache.Exists(key))
                    {
                        count = Core.Cache.Get<int>(key);
                        count++;
                    }
                    if (count >= CommonConst.MALL_PRODUCT_VISIT_COUNT_MAX)
                    {
                        _StatisticsService.AddProductVisit(DateTime.Now.Date, pid, shopId, count);
                        count = 0;
                    }
                    Core.Cache.Insert(key, count);
                }

            });
        }
        #endregion 前台统计

        #region 辅助方法
        public static string Encrypt(string value, string key = "KFIOS")
        {
            string text = string.Empty;
            try
            {
                string plainText = value;
                text = Core.Helper.SecureHelper.AESEncrypt(plainText, key);
                text = Core.Helper.SecureHelper.EncodeBase64(text);
                return text;
            }
            catch (Exception ex)
            {
                Core.Log.Error("StatisticApplication加密异常：", ex);
                throw;
            }
        }
        public static string Decrypt(string value, string key = "KFIOS")
        {
            string plainText = string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    value = System.Web.HttpUtility.UrlDecode(value);
                    value = Core.Helper.SecureHelper.DecodeBase64(value);
                    plainText = Core.Helper.SecureHelper.AESDecrypt(value, key);//解密
                }
            }
            catch (Exception ex)
            {
                Core.Log.Error("StatisticApplication解密异常：", ex);
            }
            return plainText;
        }
        #endregion 辅助方法


        /// <summary>
        /// 取商品统计
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ProductStatisticModel> GetProductSales(ProductStatisticQuery query)
        {
            var end = query.EndDate;
            query.EndDate = query.EndDate.AddDays(1);//日期末尾
            var data = _StatisticsService.GetProductVisits(query);

            foreach (var item in data.Models)
            {
                item.StartDate = query.StartDate;
                item.EndDate = end;
            }
            return data;
        }
        /// <summary>
        /// 取商品销售分类统计
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="shop"></param>
        /// <returns></returns>
        public static List<ProductCategoryStatistic> GetProductCategorySales(long shop, DateTime begin, DateTime end)
        {
            end = end.Date.AddDays(1);//日期末尾
            //获取统计原始数据
            var source = _StatisticsService.GetProductCategoryStatistic(shop, begin, end);

            //将分类统一为顶级分类
            var categoryAll = CategoryApplication.GetCategories();
            foreach (var item in source)
            {
                var top = CategoryApplication.GetTopCategory(categoryAll, item.CategoryId);
                item.CategoryId = top.Id;
                item.CategoryName = top.Name;
            }

            //合并分类
            var result = source
                .GroupBy(p => p.CategoryId)
                .Select(item => new ProductCategoryStatistic
                {
                    CategoryId = item.Key,
                    CategoryName = item.FirstOrDefault().CategoryName,
                    SaleAmounts = item.Sum(p => p.SaleAmounts),
                    SaleCounts = item.Sum(p => p.SaleCounts)
                }).ToList();

            //计算总和
            var totalAmount = result.Sum(e => e.SaleAmounts);
            if (totalAmount == 0) totalAmount = 1;
            var totalCount = result.Sum(e => e.SaleCounts);
            if (totalCount == 0) totalCount = 1;

            //计算占比率
            foreach (var item in result)
            {
                item.AmountRate = item.AmountRate / totalAmount;
                item.CountRate = (decimal)item.SaleCounts / totalCount;
            }
            return result;
        }

        /// <summary>
        /// 平台交易统计
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static TradeStatisticModel GetPlatTradeStatistic(DateTime startDate, DateTime endDate)
        {
            var models = _StatisticsService.GetPlatVisits(startDate.Date, endDate.Date.AddDays(1));
            var tradeModel = new TradeStatisticModel
            {
                OrderUserCount = models.Sum(e => e.OrderUserCount),
                OrderCount = models.Sum(e => e.OrderCount),
                OrderProductCount = models.Sum(e => e.OrderProductCount),
                OrderAmount = models.Sum(e => e.OrderAmount),
                OrderPayUserCount = models.Sum(e => e.OrderPayUserCount),
                OrderPayCount = models.Sum(e => e.OrderPayCount),
                SaleCounts = models.Sum(e => e.SaleCounts),
                SaleAmounts = models.Sum(e => e.SaleAmounts),
                VisitCounts = models.Sum(e => e.VisitCounts)
            };
            tradeModel.ChartModelPayAmounts = GetChartDataModel(startDate, endDate, models, item => item.SaleAmounts);
            tradeModel.ChartModelPayPieces = GetChartDataModel(startDate, endDate, models, item => item.SaleCounts);
            tradeModel.ChartModelPayUsers = GetChartDataModel(startDate, endDate, models, item => item.OrderPayUserCount);

            //TODO:FG 比率计算公式与模型内计算公式不一致
            tradeModel.ChartModelOrderConversionsRates = GetChartDataModel(startDate, endDate, models,
                item =>
                {
                    if (item.VisitCounts > 0) return Math.Round(Convert.ToDecimal(item.OrderUserCount) / item.VisitCounts * 100, 2);
                    else return 0;
                });
            tradeModel.ChartModelPayConversionsRates = GetChartDataModel(startDate, endDate, models,
                item =>
                {
                    if (item.OrderUserCount > 0) return Math.Round(Convert.ToDecimal(item.OrderPayUserCount) / item.OrderUserCount * 100, 2);
                    else return 0;
                }
                );
            tradeModel.ChartModelTransactionConversionRate = GetChartDataModel(startDate, endDate, models,
                item =>
                {
                    if (item.VisitCounts > 0) return Math.Round(Convert.ToDecimal(item.OrderPayUserCount) / item.VisitCounts * 100, 2);
                    else return 0;
                });
            return tradeModel;
        }
        /// <summary>
        /// 平台交易统计(按天)
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static List<TradeStatisticModel> GetPlatTradeStatisticItems(DateTime begin, DateTime end)
        {
            return _StatisticsService.GetPlatVisits(begin.Date, end.Date.AddDays(1));
        }
        /// <summary>
        /// 图表对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="dt1"></param>
        /// <param name="dt2"></param>
        /// <param name="dataSource"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        static LineChartDataModel<T> GetChartDataModel<T>(DateTime dt1, DateTime dt2, IEnumerable<TradeStatisticModel> dataSource, Func<TradeStatisticModel, T> selector) where T : struct
        {
            LineChartDataModel<T> chartData = new LineChartDataModel<T>();
            chartData.XAxisData = GenerateStringByDate(dt1, dt2);//X轴数据（日期）
            chartData.SeriesData = GetSeriesData(dt1, dt2, dataSource, selector);
            return chartData;
        }

        /// <summary>
        /// Y轴
        /// </summary>
        /// <typeparam name="T">返回的值类型</typeparam>
        /// <param name="name"></param>
        /// <param name="dt1"></param>
        /// <param name="dt2"></param>
        /// <param name="dataSource"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        static List<ChartSeries<T>> GetSeriesData<T>(DateTime dt1, DateTime dt2, IEnumerable<TradeStatisticModel> dataSource, Func<TradeStatisticModel, T> selector)
            where T : struct
        {
            int day = (dt2 - dt1).Days + 1;
            int count = 0;
            var seriesData = new ChartSeries<T>()
            {
                Data = new T[day]
            };
            while (dt1 <= dt2)
            {
                var selectObj = dataSource.Where(e => e.Date.Date == dt1.Date).Select(selector);
                if (selectObj != null)
                    seriesData.Data[count] = selectObj.FirstOrDefault();
                else
                    seriesData.Data[count] = default(T);
                dt1 = dt1.AddDays(1);
                count++;
            }
            return new List<ChartSeries<T>>() { seriesData };
        }

        /// <summary>
        /// 店铺交易统计
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="shop"></param>
        /// <returns></returns>
        public static TradeStatisticModel GetShopTradeStatistic(long shop, long? shopbranchId, DateTime begin, DateTime end,int type=0)
        {
            var models = new List<TradeStatisticModel>();

            if (type == 0)
            {
                models = _StatisticsService.GetShopVisits(shop, shopbranchId, begin, end.AddDays(1));
            }                
            else
            {
                models = _StatisticsService.GetShopVisitsByRealTime(shop, shopbranchId, begin, end);
            }

            var tradeModel = new TradeStatisticModel
            {
                OrderUserCount = models.Sum(e => e.OrderUserCount),
                OrderCount = models.Sum(e => e.OrderCount),
                OrderProductCount = models.Sum(e => e.OrderProductCount),
                OrderAmount = models.Sum(e => e.OrderAmount),
                OrderPayUserCount = models.Sum(e => e.OrderPayUserCount),
                OrderPayCount = models.Sum(e => e.OrderPayCount),
                SaleCounts = models.Sum(e => e.SaleCounts),
                SaleAmounts = models.Sum(e => e.SaleAmounts),
                VisitCounts = models.Sum(e => e.VisitCounts),
                OrderRefundCount = models.Sum(e => e.OrderRefundCount),
                OrderRefundAmount = models.Sum(e => e.OrderRefundAmount),
                OrderRefundProductCount = models.Sum(e => e.OrderRefundProductCount)
            };
            #region TDO:ZYF 重构图表数据
            var mList = new List<TradeStatisticModel>();
            var mGroup = models.GroupBy(t => t.Date).ToList();
            foreach(var g in mGroup)
            {
                var m = new TradeStatisticModel
                {
                    Date = g.Key,
                    OrderUserCount = g.Sum(e => e.OrderUserCount),
                    OrderCount = g.Sum(e => e.OrderCount),
                    OrderProductCount = g.Sum(e => e.OrderProductCount),
                    OrderAmount = g.Sum(e => e.OrderAmount),
                    OrderPayUserCount = g.Sum(e => e.OrderPayUserCount),
                    OrderPayCount = g.Sum(e => e.OrderPayCount),
                    SaleCounts = g.Sum(e => e.SaleCounts),
                    SaleAmounts = g.Sum(e => e.SaleAmounts),
                    VisitCounts = g.Sum(e => e.VisitCounts),
                    OrderRefundCount = g.Sum(e => e.OrderRefundCount),
                    OrderRefundAmount = g.Sum(e => e.OrderRefundAmount),
                    OrderRefundProductCount = g.Sum(e => e.OrderRefundProductCount)
                };
                mList.Add(m);
            }
            #endregion
            tradeModel.ChartModelPayAmounts = GetChartDataModel(begin, end, mList, item => item.SaleAmounts);
            tradeModel.ChartModelPayPieces = GetChartDataModel(begin, end, mList, item => item.SaleCounts);
            tradeModel.ChartModelPayUsers = GetChartDataModel(begin, end, mList, item => item.OrderPayUserCount);
            //TODO:FG 比率计算公式与模型内计算公式不一致
            tradeModel.ChartModelOrderConversionsRates = GetChartDataModel(begin, end, mList,
                item =>
                {
                    if (item.VisitCounts > 0) return Math.Round(Convert.ToDecimal(item.OrderUserCount) / item.VisitCounts * 100, 2);
                    else return 0;
                });
            tradeModel.ChartModelPayConversionsRates = GetChartDataModel(begin, end, mList,
                item =>
                {
                    if (item.OrderUserCount > 0) return Math.Round(Convert.ToDecimal(item.OrderPayUserCount) / item.OrderUserCount * 100, 2);
                    else return 0;
                });
            tradeModel.ChartModelTransactionConversionRate = GetChartDataModel(begin, end, mList,
                item =>
                {
                    if (item.VisitCounts > 0) return Math.Round(Convert.ToDecimal(item.OrderPayUserCount) / item.VisitCounts * 100, 2);
                    else return 0;
                });
            //今日销售额
            tradeModel.TodaySaleAmount = OrderAndSaleStatisticsApplication.GetTodaySaleAmount(shop, shopbranchId);
            return tradeModel;
        }

        /// <summary>
        /// 取店铺交易统计(按天)
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="shop"></param>
        /// <returns></returns>
        public static List<TradeStatisticModel> GetShopTradeStatisticItem(long shop, long? shopbranchId, DateTime begin, DateTime end)
        {
            return _StatisticsService.GetShopVisits(shop, shopbranchId, begin, end.AddDays(1));
        }

        /// <summary>
        /// 订单区域分析图
        /// </summary>
        /// <param name="dimension"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static MapChartDataModel<decimal> GetAreaOrderChart(SaleDimension dimension, int year, int month)
        {
            DateTime begin = new DateTime(year, month, 1);
            DateTime end = begin.AddMonths(1).AddDays(-1);
            return GetAreaOrderChart(dimension, begin, end);
        }

        public static MapChartDataModel<decimal> GetAreaOrderChart(SaleDimension dimension, DateTime begin, DateTime end)
        {
            var source = new List<ChartDataItem<long, decimal>>();
            switch (dimension)
            {
                case SaleDimension.Count:
                    source = _StatisticsService.GetAreaMapByOrderCount(begin, end.AddDays(1))
                        .Select(item => new ChartDataItem<long, decimal>
                        {
                            ItemKey = item.ItemKey,
                            ItemValue = item.ItemValue
                        }).ToList();
                    break;
                case SaleDimension.Amount:
                    source = _StatisticsService.GetAreaMapByOrderAmount(begin, end.AddDays(1));
                    break;
            }

            var map = new MapChartDataModel<decimal>
            {
                Data = source.Select(item =>
                {
                    var province = RegionApplication.GetRegion(item.ItemKey, Region.RegionLevel.Province);
                    return new MapChartDataItem<decimal>
                    {
                        Name = province.ShortName,
                        Value = item.ItemValue
                    };
                }).ToList()
            };
            map.MaxValue = source.Max(p => p.ItemValue);
            map.MinValue = source.Min(p => p.ItemValue);
            map.TotalValue = source.Sum(p => p.ItemValue);
            return map;
        }

        #region 会员新增统计
        /// <summary>
        /// 获取新增会员统计
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static LineChartDataModel<int> GetNewMemberChartByMonth(int year, int month)
        {
            //ps 2018-1-1 ~ 2018-1-31
            DateTime thisMonth = new DateTime(year, month, 1);
            DateTime thisMonthEnd = thisMonth.AddMonths(1).AddDays(-1);

            //ps 2017-12-1 ~ 2017-12-31
            DateTime prevMonth = thisMonth.AddMonths(-1);
            DateTime prevMonthEnd = prevMonth.AddMonths(1).AddDays(-1);

            var chart = new LineChartDataModel<int>
            {
                XAxisData = GenerateIndexBySize(31)
            };

            //本月新增数据
            var thisChart = GetMemberChart(string.Format("{0}月新增会员", thisMonth.Month), thisMonth, thisMonthEnd);
            chart.SeriesData.Add(thisChart);

            //上月新增数据
            var prevChart = GetMemberChart(string.Format("{0}月新增会员", prevMonth.Month), prevMonth, prevMonthEnd);
            chart.SeriesData.Add(prevChart);
            return chart;
        }
        public static LineChartDataModel<int> GetNewMemberChartByRange(DateTime begin, DateTime end)
        {
            var chart = new LineChartDataModel<int>
            {
                XAxisData = GenerateStringByDate(begin, end)
            };

            //区间内新增会员
            var name = string.Format("{0}至{1}新增会员", begin.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));
            var data = GetMemberChart(name, begin, end);
            chart.SeriesData.Add(data);
            return chart;
        }
        /// <summary>
        /// 获取会员新增曲线
        /// </summary>
        /// <param name="name"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static ChartSeries<int> GetMemberChart(string name, DateTime begin, DateTime end)
        {
            var data = _StatisticsService.GetMemberChart(begin, end.AddDays(1));//时间为当日末尾
            var chart = GetChartByDate(data, name, begin, end);
            return chart;
        }
        #endregion

        #region 商品销售排行统计
        public static LineChartDataModel<int> GetProductSaleRankingChart(long shop, int year, int month, int week, SaleDimension dimension, int rankSize)
        {
            DateTime begin = new DateTime(year, month, 1);
            DateTime end = begin.AddMonths(1).AddDays(-1);
            if (week > 0)
            {
                begin = DateTimeHelper.GetStartDayOfWeeks(year, month, week);
                end = begin.AddDays(6);
            }
            return GetProductSaleRankingChart(shop, begin, end, dimension, rankSize);
        }

        public static LineChartDataModel<int> GetProductSaleRankingChart(long shop, DateTime begin, DateTime end, SaleDimension dimension, int rankSize)
        {
            switch (dimension)
            {
                case SaleDimension.Count:
                    return GetProductRankingBySaleCount(shop, begin, end, 15);
                case SaleDimension.Amount:
                    return GetProductRankingBySaleAmount(shop, begin, end, 15);
                case SaleDimension.Visti:
                    return GetProductRankingByVisti(shop, begin, end, 15);
                default:
                    return GetProductRankingBySaleCount(shop, begin, end, 15);
            }
        }
        private static LineChartDataModel<int> GetProductRankingBySaleCount(long shop, DateTime begin, DateTime end, int rankSize)
        {
            var model = new LineChartDataModel<int>()
            {
                XAxisData = GenerateIndexBySize(rankSize),
                ExpandProp = GenerateIndexBySize(rankSize),
            };
            int index = 0;
            var source = _StatisticsService.GetProductChartBySaleCount(shop, begin, end, rankSize);

            var name = string.Format("商品销售额Top" + rankSize.ToString() + "    " + begin.ToString("yyyy-MM-dd") + "至" + end.ToString("yyyy-MM-dd"));
            var series = new ChartSeries<int>
            {
                Name = name,
                Data = new int[rankSize]
            };

            foreach (var item in source)
            {
                series.Data[index] = item.ItemValue;
                model.ExpandProp[index] = item.Expand;
                index++;
            }
            model.SeriesData.Add(series);
            return model;
        }
        private static LineChartDataModel<int> GetProductRankingBySaleAmount(long shop, DateTime begin, DateTime end, int rankSize)
        {
            var model = new LineChartDataModel<int>()
            {
                XAxisData = GenerateIndexBySize(rankSize),
                ExpandProp = GenerateIndexBySize(rankSize),
            };
            int index = 0;
            var source = _StatisticsService.GetProductChartBySaleAmount(shop, begin, end, rankSize);

            var name = string.Format("商品销售额Top" + rankSize.ToString() + "    " + begin.ToString("yyyy-MM-dd") + "至" + end.ToString("yyyy-MM-dd"));
            var series = new ChartSeries<int>
            {
                Name = name,
                Data = new int[rankSize]
            };

            foreach (var item in source)
            {
                if (index >= rankSize)
                    break;
                series.Data[index] = (int)item.ItemValue;
                model.ExpandProp[index] = item.Expand;
                index++;
            }
            model.SeriesData.Add(series);
            return model;
        }
        private static LineChartDataModel<int> GetProductRankingByVisti(long shop, DateTime begin, DateTime end, int rankSize)
        {
            var model = new LineChartDataModel<int>()
            {
                XAxisData = GenerateIndexBySize(rankSize),
                ExpandProp = GenerateIndexBySize(rankSize),
            };
            int index = 0;
            var source = _StatisticsService.GetProductChartByVisti(shop, begin, end, rankSize);

            var name = string.Format("商品访问量Top" + rankSize.ToString() + "    " + begin.ToString("yyyy-MM-dd") + "至" + end.ToString("yyyy-MM-dd"));
            var series = new ChartSeries<int>
            {
                Name = name,
                Data = new int[rankSize]
            };

            foreach (var item in source)
            {
                if (index >= rankSize) break;
                series.Data[index] = (int)item.ItemValue;
                model.ExpandProp[index] = item.Expand;
                index++;
            }
            model.SeriesData.Add(series);
            return model;
        }
        #endregion

        #region 店铺新增统计
        public static LineChartDataModel<int> GetNewsShopChartByMonth(int year, int month)
        {
            //ps 2018-1-1 ~ 2018-1-31
            DateTime thisMonth = new DateTime(year, month, 1);
            DateTime thisMonthEnd = thisMonth.AddMonths(1).AddDays(-1);
            //ps 2017-12-1 ~ 2017-12-31
            DateTime prevMonth = thisMonth.AddMonths(-1);
            DateTime prevMonthEnd = prevMonth.AddMonths(1).AddDays(-1);
            var chart = new LineChartDataModel<int>
            {
                XAxisData = GenerateIndexBySize(31)
            };


            //本月新增数据
            var thisChart = GetNewShopChart(string.Format("{0}月新增店铺", thisMonth.Month), thisMonth, thisMonthEnd);
            chart.SeriesData.Add(thisChart);

            //上月新增数据
            var prevChart = GetNewShopChart(string.Format("{0}月新增店铺", prevMonth.Month), prevMonth, prevMonthEnd);
            chart.SeriesData.Add(prevChart);
            return chart;
        }

        public static LineChartDataModel<int> GetNewsShopChartByDate(DateTime begin, DateTime end)
        {
            var chart = new LineChartDataModel<int>
            {
                XAxisData = GenerateStringByDate(begin, end)
            };

            //区间内新增会员
            var name = string.Format("{0}至{1}新增店铺", begin.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));
            var data = GetNewShopChart(name, begin, end);//截止日期为End末尾
            chart.SeriesData.Add(data);
            return chart;
        }

        private static ChartSeries<int> GetNewShopChart(string name, DateTime begin, DateTime end)
        {
            var data = _StatisticsService.GetNewShopChart(begin, end.AddDays(1));//时间为当日末尾
            var chart = GetChartByDate(data, name, begin, end);
            return chart;
        }
        #endregion

        #region 店铺销量统计
        public static LineChartDataModel<int> GetShopSaleCountChartByMonth(long shopId, int year, int month)
        {
            //ps 2018-1-1 ~ 2018-1-31
            DateTime thisMonth = new DateTime(year, month, 1);
            DateTime thisMonthEnd = thisMonth.AddMonths(1).AddDays(-1);

            //ps 2017-12-1 ~ 2017-12-31
            DateTime prevMonth = thisMonth.AddMonths(-1);
            DateTime prevMonthEnd = prevMonth.AddMonths(1).AddDays(-1);

            var chart = new LineChartDataModel<int>
            {
                XAxisData = GenerateIndexBySize(31)
            };

            var chart1 = GetShopChartBySaleCount(string.Format("{0}月店铺总销量", thisMonth.Month), shopId, thisMonth, thisMonthEnd);
            chart.SeriesData.Add(chart1);
            var chart2 = GetShopChartBySaleCount(string.Format("{0}月店铺总销量", prevMonth.Month), shopId, prevMonth, prevMonthEnd);
            chart.SeriesData.Add(chart2);
            return chart;
        }
        public static LineChartDataModel<int> GetShopSaleCountChartByRange(long shop, string title, DateTime begin, DateTime end)
        {
            var chart = new LineChartDataModel<int>
            {
                XAxisData = GenerateStringByDate(begin, end)
            };

            //区间内门店销量
            var name = string.Format("{0}至{1}门店销量", begin.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));
            var data = GetShopChartBySaleCount(name, shop, begin, end);//截止日期为End末尾
            chart.SeriesData.Add(data);
            return chart;
        }
        private static ChartSeries<int> GetShopChartBySaleCount(string name, long shop, DateTime begin, DateTime end)
        {
            var data = _StatisticsService.GetShopBySaleCount(shop, begin, end.AddDays(1));//时间为当日末尾
            var chart = GetChartByDate(data, name, begin, end);
            return chart;
        }
        #endregion

        #region 店铺流量统计
        public static LineChartDataModel<int> GetShopFlowChart(long shop, int year, int month)
        {
            //ps 2018-1-1 ~ 2018-1-31
            DateTime thisMonth = new DateTime(year, month, 1);
            DateTime thisMonthEnd = thisMonth.AddMonths(1).AddDays(-1);

            //ps 2017-12-1 ~ 2017-12-31
            DateTime prevMonth = thisMonth.AddMonths(-1);
            DateTime prevMonthEnd = prevMonth.AddMonths(1).AddDays(-1);

            var chart = new LineChartDataModel<int>
            {
                XAxisData = GenerateIndexBySize(31)
            };

            var chart1 = GetShopFlowChart(shop, string.Format("{0}月店铺总流量", thisMonth.Month), thisMonth, thisMonthEnd);
            chart.SeriesData.Add(chart1);
            var chart2 = GetShopFlowChart(shop, string.Format("{0}月店铺总流量", prevMonth.Month), prevMonth, prevMonthEnd);
            if (chart1.Data.Length - chart2.Data.Length > 0)
            {
                int span = chart1.Data.Length - chart2.Data.Length;
                var newData = new int[chart2.Data.Length + span];
                for (int i = 0; i < chart2.Data.Length; i++)
                {
                    newData[i] = chart2.Data[i];
                }
                chart2.Data = newData;
            }
            chart.SeriesData.Add(chart2);
            return chart;
        }

        public static LineChartDataModel<int> GetShopFlowChart(long shop, DateTime begin, DateTime end)
        {
            var chart = new LineChartDataModel<int>
            {
                XAxisData = GenerateStringByDate(begin, end)
            };
            DateTime prevMonth = begin.AddMonths(-1);
            DateTime prevMonthEnd = prevMonth.AddMonths(1).AddDays(-1);
            var chart1 = GetShopFlowChart(shop, string.Format("{0}月店铺总流量", begin.Month), begin, end);
            chart.SeriesData.Add(chart1);
            var chart2 = GetShopFlowChart(shop, string.Format("{0}月店铺总流量", prevMonth.Month), prevMonth, prevMonthEnd);
            if (chart1.Data.Length - chart2.Data.Length > 0)
            {
                int span = chart1.Data.Length - chart2.Data.Length;
                var newData = new int[chart2.Data.Length + span];
                for (int i = 0; i < chart2.Data.Length; i++)
                {
                    newData[i] = chart2.Data[i];
                }
                chart2.Data = newData;
            }
            chart.SeriesData.Add(chart2);

            return chart;
        }

        private static ChartSeries<int> GetShopFlowChart(long shop, string name, DateTime begin, DateTime end)
        {
            var data = _StatisticsService.GetShopFlowChart(shop, begin, end.AddDays(1));//时间为当日末尾
            var chart = GetChartByDate(data, name, begin, end);
            return chart;
        }
        #endregion

        #region 店铺转化率统计
        public static LineChartDataModel<decimal> GetDealConversionRateChart(long shop, int year, int month)
        {
            //ps 2018-1-1 ~ 2018-1-31
            DateTime thisMonth = new DateTime(year, month, 1);
            DateTime thisMonthEnd = thisMonth.AddMonths(1).AddDays(-1);
            //ps 2017-12-1 ~ 2017-12-31
            DateTime prevMonth = thisMonth.AddMonths(-1);
            DateTime prevMonthEnd = prevMonth.AddMonths(1).AddDays(-1);
            var chart = new LineChartDataModel<decimal>
            {
                XAxisData = GenerateIndexBySize(31)
            };

            //本月新增数据
            var thisChart = GetDealConversionRateChart(string.Format("{0}月成交转换率", thisMonth.Month), shop, thisMonth, thisMonthEnd);
            chart.SeriesData.Add(thisChart);

            //上月新增数据
            var prevChart = GetDealConversionRateChart(string.Format("{0}月成交转换率", prevMonth.Month), shop, prevMonth, prevMonthEnd);
            chart.SeriesData.Add(prevChart);
            return chart;
        }

        private static ChartSeries<decimal> GetDealConversionRateChart(string name, long shop, DateTime begin, DateTime end)
        {
            var data = _StatisticsService.GetConversionRate(shop, begin, end.AddDays(1));//时间为当日末尾
            var chart = GetChartByDate(data, name, begin, end);
            return chart;
        }
        #endregion

        #region  店铺排行

        public static LineChartDataModel<int> GetShopRankingChart(int year, int month, int week, SaleDimension dimension, int rankSize)
        {
            DateTime begin = new DateTime(year, month, 1);
            DateTime end = begin.AddMonths(1).AddDays(-1);
            if (week > 0)
            {
                begin = DateTimeHelper.GetStartDayOfWeeks(year, month, week);
                end = begin.AddDays(6);
            }
            return GetShopRankingChart(begin, end, dimension, rankSize);
        }
        public static LineChartDataModel<int> GetShopRankingChart(DateTime start, DateTime end, SaleDimension dimension, int rankSize)
        {
            if (dimension == SaleDimension.Count)
                return GetShopRankingChartByCount(start, end, 15);
            else
                return GetShopRankingChartByAmount(start, end, 15);
        }

        /// <summary>
        /// 初始化按照销售额维度获取前N店铺排行
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="rankSize"></param>
        private static LineChartDataModel<int> GetShopRankingChartByAmount(DateTime begin, DateTime end, int rankSize)
        {
            var model = new LineChartDataModel<int>()
            {
                XAxisData = GenerateIndexBySize(rankSize),
                ExpandProp = GenerateIndexBySize(rankSize),
            };
            int index = 0;
            var source = _StatisticsService.GetShopRankingBySaleAmount(begin, end, rankSize);

            var name = string.Format("店铺销售额Top" + rankSize.ToString() + "    " + begin.ToString("yyyy-MM-dd") + "至" + end.ToString("yyyy-MM-dd"));
            var series = new ChartSeries<int>
            {
                Name = name,
                Data = new int[rankSize]
            };

            foreach (var item in source)
            {
                series.Data[index] = (int)item.ItemValue;
                model.ExpandProp[index] = item.Expand;
                index++;
            }
            model.SeriesData.Add(series);
            return model;
        }
        /// <summary>
        /// 初始化按照订单量维度获取前N店铺排行
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="rankSize"></param>
        private static LineChartDataModel<int> GetShopRankingChartByCount(DateTime begin, DateTime end, int rankSize)
        {
            var model = new LineChartDataModel<int>()
            {
                XAxisData = GenerateIndexBySize(rankSize),
                ExpandProp = GenerateIndexBySize(rankSize),
            };
            int index = 0;
            var source = _StatisticsService.GetShopRankingByOrderCount(begin, end, rankSize);

            var name = string.Format("店铺订单量Top" + rankSize.ToString() + "    " + begin.ToString("yyyy-MM-dd") + "至" + end.ToString("yyyy-MM-dd"));
            var series = new ChartSeries<int>
            {
                Name = name,
                Data = new int[rankSize]
            };

            foreach (var item in source)
            {
                series.Data[index] = item.ItemValue;
                model.ExpandProp[index] = item.Expand;
                index++;
            }
            model.SeriesData.Add(series);
            return model;
        }
        #endregion

        /// <summary>
        /// 生成表格数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        private static ChartSeries<T> GetChartByDate<T>(List<ChartDataItem<DateTime, T>> source, string name, DateTime beginDate, DateTime endDate)
        {
            //以日期为基准
            beginDate = beginDate.Date;
            endDate = endDate.Date;

            var span = endDate - beginDate;
            var day = (int)span.TotalDays + 1;
            var result = new ChartSeries<T> { Name = name, Data = new T[day] };

            var index = 0;
            for (var date = beginDate; date <= endDate; date = date.AddDays(1), index++)
            {
                var item = source.FirstOrDefault(p => p.ItemKey.Date == date);
                if (item != null) result.Data[index] = item.ItemValue;
                else result.Data[index] = default(T);
            }
            return result;
        }
        private static string[] GenerateIndexBySize(int size)
        {
            var result = new List<string>();
            for (int i = 1; i <= size; i++)
                result.Add(i.ToString());
            return result.ToArray();
        }
        private static string[] GenerateStringByDate(DateTime begin, DateTime end)
        {
            var result = new List<string>();
            for (var date = begin.Date; date <= end; date = date.AddDays(1))
                result.Add(date.ToString("MM-dd"));
            return result.ToArray();
        }

        #region 订单数量汇总
        public static OrderStatisticsItem GetOrderCount(OrderCountStatisticsQuery query)
        {
            return GetService<IOrderService>().GetOrderCountStatistics(query);
        }
        /// <summary>
        /// 获取平台数据汇总
        /// </summary>
        /// <returns></returns>
        public static PlatConsoleModel GetPlatformConsole()
        {
            var today = DateTime.Now.Date;
            var yesterday = DateTime.Now.Date.AddDays(-1);
            var model = new PlatConsoleModel();
            //待处理投诉数量
            model.Complaints = ComplaintApplication.GetOrderComplaintCount(new ComplaintQuery { Status = OrderComplaintInfo.ComplaintStatus.Dispute });
            //商品评论总数(包含隐藏)
            model.ProductComments = CommentApplication.GetCommentCount(new ProductCommentQuery { ShowHidden = true });
            //咨询总数
            model.ProductConsultations = ConsultationApplication.GetConsultationCount(new ConsultationQuery());
            //今日新增会员
            var memberQuery = new MemberQuery { RegistTimeStart = today };
            model.TodayMemberIncrease = MemberApplication.GetMemberCount(memberQuery);
            //待审核品牌申请
            var applyQuery = new BrandApplyQuery { AuditStatus = ShopBrandApplyInfo.BrandAuditStatus.UnAudit };
            model.WaitForAuditingBrands = BrandApplication.GetShopBrandApplyCount(applyQuery);
            //待审核限时购 
            var limitQuery = new LimitTimeQuery { Status = (int)FlashSaleInfo.FlashSaleStatus.WaitForAuditing };
            model.WaitForAuditingLimitTimeBuy = LimitTimeApplication.GetFlashSaleCount(limitQuery);
            //门店待提现
            var shopWithQuery = new WithdrawQuery { Status = WithdrawStaus.WatingAudit };
            model.ShopCashNumber = BillingApplication.GetShopWithDrawCount(shopWithQuery);
            //礼品订单代发货数
            var giftsQuery = new GiftsOrderQuery { Status = GiftOrderInfo.GiftOrderStatus.WaitDelivery };
            model.GiftSend = GiftsOrderApplication.GetOrderCount(giftsQuery);
            //会员待提现
            var memberWithQuery = new ApplyWithDrawQuery { withDrawStatus = ApplyWithdrawInfo.ApplyWithdrawStatus.WaitConfirm };
            model.Cash = MemberCapitalApplication.GetApplyWithDrawCount(memberWithQuery);


            #region 订单相关数据汇总
            //今日销售额
            var orderQuery = new OrderCountStatisticsQuery
            {
                IsPayed = true,
                OrderDateBegin = today,
                Fields = new List<OrderCountStatisticsFields> {
                    OrderCountStatisticsFields.ActualPayAmount
                }
            };
            model.TodaySaleAmount = GetOrderCount(orderQuery).TotalActualPayAmount;
            //待支付数
            orderQuery = new OrderCountStatisticsQuery
            {
                OrderOperateStatus = OrderInfo.OrderOperateStatus.WaitPay,
                Fields = new List<OrderCountStatisticsFields> {
                    OrderCountStatisticsFields.OrderCount
                }
            };
            model.WaitPayTrades = GetOrderCount(orderQuery).OrderCount;
            //代发货数
            orderQuery.OrderOperateStatus = OrderInfo.OrderOperateStatus.WaitDelivery;
            model.WaitDeliveryTrades = GetOrderCount(orderQuery).OrderCount;
            //完成订单数
            orderQuery.OrderOperateStatus = OrderInfo.OrderOperateStatus.Finish;
            model.OrderCounts = GetOrderCount(orderQuery).OrderCount;
            #endregion

            #region 店铺相关数据汇总
            var shopQuery = new ShopQuery { Status = ShopInfo.ShopAuditStatus.Open, Stage = ShopInfo.ShopStage.Finish };
            //店铺总数
            model.ShopNum = ShopApplication.GetShopCount(shopQuery);
            //今日新增店铺数
            shopQuery.CreateDateBegin = today;
            model.TodayShopIncrease = ShopApplication.GetShopCount(shopQuery);
            //昨日新增店铺数
            shopQuery.CreateDateBegin = yesterday;
            shopQuery.CreateDateEnd = today;
            model.YesterdayShopIncrease = ShopApplication.GetShopCount(shopQuery);
            //待审核店铺数
            shopQuery = new ShopQuery { Status = ShopInfo.ShopAuditStatus.WaitAudit };
            model.WaitAuditShops = ShopApplication.GetShopCount(shopQuery);
            //过期店铺数
            //shopQuery = new ShopQuery { ExpiredDateEnd = today };
            shopQuery = new ShopQuery { Status = ShopInfo.ShopAuditStatus.HasExpired };
            model.ExpiredShops = ShopApplication.GetShopCount(shopQuery);
            #endregion

            #region 商品相关数据汇总
            var productQuery = new ProductQuery();
            //总商品数
            model.ProductNum = ProductManagerApplication.GetProductCount(productQuery);
            //在售商品数
            productQuery.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
            productQuery.AuditStatus = new[] { ProductInfo.ProductAuditStatus.Audited };
            model.OnSaleProducts = ProductManagerApplication.GetProductCount(productQuery);
            //待审核商品数
            productQuery.AuditStatus = new[] { ProductInfo.ProductAuditStatus.WaitForAuditing };
            productQuery.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
            model.WaitForAuditingProducts = ProductManagerApplication.GetProductCount(productQuery);
            #endregion

            #region 售后相关数据汇总
            var refundQuery = new RefundQuery
            {
                AuditStatus = OrderRefundInfo.OrderRefundAuditStatus.Audited,
                ConfirmStatus = OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm,
                RefundModes = new List<OrderRefundInfo.OrderRefundMode> {
                    OrderRefundInfo.OrderRefundMode.OrderItemRefund,
                    OrderRefundInfo.OrderRefundMode.OrderRefund
                }
            };
            model.RefundTrades = RefundApplication.GetOrderRefundsCount(refundQuery);
            refundQuery.RefundModes = new List<OrderRefundInfo.OrderRefundMode> { OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund };
            model.OrderWithRefundAndRGoods = RefundApplication.GetOrderRefundsCount(refundQuery);
            #endregion

            return model;
        }
        public static SellerConsoleModel GetSellerConsoleModel(long shopId)
        {
            var model = new SellerConsoleModel();

            #region 门店基本信息
            var shop = ShopApplication.GetShop(shopId);
            model.ShopName = shop.ShopName;
            model.ShopEndDate = shop.EndDate.Value;
            model.ShopFreight = shop.Freight;

            var grade = ShopApplication.GetShopGrade(shop.GradeId);
            model.ShopGrade = grade.Name;
            model.ImageLimit = grade.ImageLimit;
            model.ProductLimit = grade.ProductLimit;

            var spaceUsage = GetService<IShopService>().GetShopUsageSpace(shop.Id);
            model.ProductImages = spaceUsage;
            #endregion

            //待审核品牌申请
            var applyQuery = new BrandApplyQuery { ShopId = shopId, AuditStatus = ShopBrandApplyInfo.BrandAuditStatus.UnAudit };
            model.BrandApply = BrandApplication.GetShopBrandApplyCount(applyQuery);
            //待回复评论数
            model.ProductComments = CommentApplication.GetCommentCount(new ProductCommentQuery { ShopId = shopId, IsReply = false, ShowHidden = true });//首页统计这里统计隐藏评论
            //待回复咨询数
            model.ProductConsultations = ConsultationApplication.GetConsultationCount(new ConsultationQuery { ShopID = shopId, IsReply = false });
            //待处理投诉数量
            model.Complaints = ComplaintApplication.GetOrderComplaintCount(new ComplaintQuery { ShopId = shopId, Status = OrderComplaintInfo.ComplaintStatus.WaitDeal });

            #region 售后相关数据汇总
            var refundQuery = new RefundQuery
            {
                ShopId = shopId,
                AuditStatusList = new List<OrderRefundInfo.OrderRefundAuditStatus> {
                     OrderRefundInfo.OrderRefundAuditStatus.WaitAudit,
                      OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving,
                },
                RefundModes = new List<OrderRefundInfo.OrderRefundMode> {
                    OrderRefundInfo.OrderRefundMode.OrderItemRefund,
                    OrderRefundInfo.OrderRefundMode.OrderRefund
                }
            };
            //待处理退款
            model.RefundTrades = RefundApplication.GetOrderRefundsCount(refundQuery);
            //待处理退货
            refundQuery.RefundModes = new List<OrderRefundInfo.OrderRefundMode> { OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund };
            model.RefundAndRGoodsTrades = RefundApplication.GetOrderRefundsCount(refundQuery);
            #endregion

            #region 商品相关数据汇总
            var productQuery = new ProductQuery { ShopId = shopId };

            //店铺总商品数
            model.ProductsCount = ProductManagerApplication.GetProductCount(productQuery);

            //仓库中商品数
            productQuery.SaleStatus = ProductInfo.ProductSaleStatus.InDraft;
            productQuery.NotIncludedInDraft = false;//它在实体里默认不包含草稿箱，则这里默认去掉
            model.ProductsInDraft = ProductManagerApplication.GetProductCount(productQuery);

            productQuery.NotIncludedInDraft = true;//还原
            //在售商品数
            productQuery.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
            productQuery.AuditStatus = new[] { ProductInfo.ProductAuditStatus.Audited };
            model.OnSaleProducts = ProductManagerApplication.GetProductCount(productQuery);

            //审核失败商品数
            productQuery = new ProductQuery { ShopId = shopId };
            productQuery.AuditStatus = new[] { ProductInfo.ProductAuditStatus.AuditFailed };
            model.AuditFailureProducts = ProductManagerApplication.GetProductCount(productQuery);

            //违规下架商品数
            productQuery.AuditStatus = new[] { ProductInfo.ProductAuditStatus.InfractionSaleOff };
            model.InfractionSaleOffProducts = ProductManagerApplication.GetProductCount(productQuery);

            //仓库中商品数
            productQuery.SaleStatus = ProductInfo.ProductSaleStatus.InStock;
            productQuery.AuditStatus = null;
            model.InStockProducts = ProductManagerApplication.GetProductCount(productQuery);

            //待审核商品数
            productQuery.AuditStatus = new[] { ProductInfo.ProductAuditStatus.WaitForAuditing };
            productQuery.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
            model.WaitForAuditingProducts = ProductManagerApplication.GetProductCount(productQuery);

            //商品的总评论数
            model.ProductsEvaluation = CommentApplication.GetCommentCount(new ProductCommentQuery { ShopId = shopId, ShowHidden = true });//首页统计这里统计隐藏评论
            #endregion

            #region 订单相关数据汇总
            //待支付数
            var orderQuery = new OrderCountStatisticsQuery
            {
                ShopId = shopId,
                OrderOperateStatus = OrderInfo.OrderOperateStatus.WaitPay,
                Fields = new List<OrderCountStatisticsFields> {
                    OrderCountStatisticsFields.OrderCount
                }
            };
            model.WaitPayTrades = GetOrderCount(orderQuery).OrderCount;
            //代发货数
            orderQuery.OrderOperateStatus = OrderInfo.OrderOperateStatus.WaitDelivery;
            model.WaitDeliveryTrades = GetOrderCount(orderQuery).OrderCount;
            //完成订单数
            orderQuery.OrderOperateStatus = null;
            model.OrderCounts = GetOrderCount(orderQuery).OrderCount;
            #endregion
            return model;
        }
        /// <summary>
        /// 会员订单统计
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="isOngoing">是否待处理售后</param>
        /// <param name="isVirtual">是否区别虚拟订单，默认null不区分，false只查询实体订单</param>
        /// <returns></returns>
        public static OrderCountStatistics GetMemberOrderStatistic(long userId, bool isOngoing = false,bool? isVirtual=null)
        {
            var query = new OrderCountStatisticsQuery { UserId = userId, IsVirtual = isVirtual };
            var result = GetOrderCountStatistics(query);

            //待发货订单 减掉拼团订单
            //var fight = GetService<IOrderService>().GetFightGroupOrderCountByUser(userId);
            //result.WaitingForDelivery = result.WaitingForDelivery - fight;

            //待收货
            //result.WaitingForRecieve += result.WaitingForSelfPickUp;
            result.WaitingForSelfPickUp = 0;

            //待处理售后
            result.RefundCount = RefundApplication.GetOrderRefundsCount(new RefundQuery
            {
                UserId = userId,
                IsOngoing = isOngoing
                //ConfirmStatus = OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm
            });

            return result;
        }
        public static OrderCountStatistics GetSellerOrderStatistic(long shop)
        {
            var query = new OrderCountStatisticsQuery { ShopId = shop };
            return GetOrderCountStatistics(query);
        }
        public static OrderCountStatistics GetBranchOrderStatistic(long branch)
        {
            var query = new OrderCountStatisticsQuery { ShopBranchId = branch };
            return GetOrderCountStatistics(query);
        }
        private static OrderCountStatistics GetOrderCountStatistics(OrderCountStatisticsQuery orderQuery)
        {
            var result = new OrderCountStatistics();

            //所有订单
            orderQuery.Fields = new List<OrderCountStatisticsFields> { OrderCountStatisticsFields.OrderCount };
            result.OrderCount = GetOrderCount(orderQuery).OrderCount;

            //待付款订单
            orderQuery.OrderOperateStatus = OrderInfo.OrderOperateStatus.WaitPay;
            result.WaitingForPay = GetOrderCount(orderQuery).OrderCount;

            //待发货订单
            orderQuery.OrderOperateStatus = OrderInfo.OrderOperateStatus.WaitDelivery;
            result.WaitingForDelivery = GetOrderCount(orderQuery).OrderCount;

            //待收货数量
            orderQuery.OrderOperateStatus = OrderInfo.OrderOperateStatus.WaitReceiving;
            result.WaitingForRecieve = GetOrderCount(orderQuery).OrderCount;

            //待自提数量
            orderQuery.OrderOperateStatus = OrderInfo.OrderOperateStatus.WaitSelfPickUp;
            result.WaitingForSelfPickUp = GetOrderCount(orderQuery).OrderCount;

            //未评论订单
            orderQuery.OrderOperateStatus = OrderInfo.OrderOperateStatus.Finish;
            orderQuery.IsCommented = false;
            result.WaitingForComments = GetOrderCount(orderQuery).OrderCount;
            return result;
        }
        #endregion

    }
}

