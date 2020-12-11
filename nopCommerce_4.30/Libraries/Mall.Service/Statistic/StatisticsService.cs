using Mall.CommonModel;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Mall.Core;

namespace Mall.Service
{
    public class StatisticsService : ServiceBase, IStatisticsService
    {
        public QueryPageModel<ProductStatisticModel> GetProductVisits(ProductStatisticQuery query)
        {
            string sql = "SELECT V.ProductId,P.ProductName,SUM(V.SaleAmounts)as SaleAmounts,SUM(V.SaleCounts)as SaleCounts,SUM(V.PayUserCounts)as PayUserCounts  ";
            string countSql = " select count(1) from ( SELECT count(1)  FROM Mall_productvisti V ";
            countSql += " left join Mall_product P on P.Id=V.ProductId   ";
            sql += " ,SUM(V.VisitUserCounts)as VisitUserCounts ";
            sql += " ,SUM(V.VistiCounts)as VistiCounts ";
            sql += " ,(SUM(V.PayUserCounts)/SUM(V.VisitUserCounts))as Conversion ";
            sql += " ,(case  when SUM(V.VisitUserCounts)=0 and SUM(V.PayUserCounts)>0  then 100 else SUM(V.PayUserCounts)/SUM(V.VisitUserCounts) end )as _Conversion ";
            sql += " FROM Mall_productvisti V left join Mall_product P on P.Id=V.ProductId  ";
            sql += string.Format(" where V.Date>='{0}' and V.Date<'{1}' ", query.StartDate, query.EndDate);
            countSql += string.Format(" where V.Date>='{0}' and V.Date<'{1}' ", query.StartDate, query.EndDate);
            if (query.ShopId.HasValue)
            {
                sql += string.Format(" and V.ShopId={0} ", query.ShopId.Value);
                countSql += string.Format(" and V.ShopId={0} ", query.ShopId.Value);
            }
            sql += " group by V.ProductId ";
            countSql += " group by V.ProductId )T";

            switch (query.Sort.ToLower())
            {
                case "visticounts":
                    if (query.IsAsc)
                        sql += " order by VistiCounts";
                    else
                        sql += " order by VistiCounts desc ";
                    break;
                case "visitusercounts":
                    if (query.IsAsc)
                        sql += " order by VisitUserCounts";
                    else
                        sql += " order by VisitUserCounts desc ";
                    break;
                case "payusercounts":
                    if (query.IsAsc)
                        sql += " order by PayUserCounts";
                    else
                        sql += " order by PayUserCounts desc ";
                    break;
                case "singlepercentconversion":
                    if (query.IsAsc)
                        sql += " order by _Conversion";
                    else
                        sql += " order by _Conversion desc ";
                    break;
                case "salecounts":
                    if (query.IsAsc)
                        sql += " order by SaleCounts";
                    else
                        sql += " order by SaleCounts desc ";
                    break;
                case "saleamounts":
                    if (query.IsAsc)
                        sql += " order by SaleAmounts";
                    else
                        sql += " order by SaleAmounts desc ";
                    break;
                default:
                    sql += " order by ProductId desc ";
                    break;
            }
            sql += GetSearchPage(query);
            Core.Log.Info(sql);
            Core.Log.Info(countSql);
            var model = DbFactory.Default.Query<ProductStatisticModel>(sql).ToList();
            return new QueryPageModel<ProductStatisticModel>
            {
                Models = model,
                Total = DbFactory.Default.ExecuteScalar<int>(countSql)
            };
        }


        private string GetSearchPage(ProductStatisticQuery query)
        {
            return string.Format(" LIMIT {0},{1} ", (query.PageNo - 1) * query.PageSize, query.PageSize);
        }

        public List<ProductCategoryStatistic> GetProductCategoryStatistic(long shop, DateTime begin, DateTime end)
        {
            var db = DbFactory.Default.Get<ProductVistiInfo>()
                    .LeftJoin<ProductInfo>((v, p) => v.ProductId == p.Id)
                    .Where(p => p.Date >= begin && p.Date < end)
                    .GroupBy<ProductInfo>(p => p.CategoryId)
                    .Select<ProductInfo>(p => new { CategoryId = p.CategoryId })
                    .Select(p => new
                    {
                        SaleAmounts = p.SaleAmounts.ExSum(),
                        SaleCounts = p.SaleCounts.ExSum()
                    });
            if (shop > 0)
                db.Where(p => p.ShopId == shop);
            return db.ToList<ProductCategoryStatistic>();
        }
        public List<TradeStatisticModel> GetShopVisits(long shop, long? shopbranchId, DateTime begin, DateTime end)
        {
            var db = DbFactory.Default.Get<ShopVistiInfo>().Where(e => e.Date >= begin.Date && e.Date < end.Date);
            if (shop > 0)
                db = db.Where(p => p.ShopId == shop);
            if (shopbranchId.HasValue)
                db = db.Where(p => p.ShopBranchId == shopbranchId);
            return db.ToList().Select(item => new TradeStatisticModel
            {
                Date = item.Date,
                VisitCounts = item.VistiCounts,
                OrderAmount = item.OrderAmount,
                OrderCount = item.OrderCount,
                OrderPayCount = item.OrderPayCount,
                OrderPayUserCount = item.OrderPayUserCount,
                OrderProductCount = item.OrderProductCount,
                OrderUserCount = item.OrderUserCount,
                SaleAmounts = item.SaleAmounts,
                SaleCounts = item.SaleCounts,
                StatisticFlag = item.StatisticFlag,
                OrderRefundCount =(int) item.OrderRefundCount,
                OrderRefundProductCount = item.OrderRefundProductCount,
                OrderRefundAmount = item.OrderRefundAmount
            }).ToList();
        }
        public List<TradeStatisticModel> GetShopVisitsByRealTime(long shop, long? shopbranchId, DateTime begin, DateTime end)
        {
            if (begin > end)
            {
                throw new MallException("时间段异常：开始时间大于结束时间！");
            }
            var models = new List<TradeStatisticModel>();
            var model = new TradeStatisticModel() {
                Date = begin
            };
            
            //取浏览人数
            var db = DbFactory.Default.Get<ShopVistiInfo>().Where(e => e.Date >= begin && e.Date < end);
            if (shop > 0)
                db = db.Where(p => p.ShopId == shop);
            if (shopbranchId.HasValue)
                db = db.Where(p => p.ShopBranchId == shopbranchId);
            model.VisitCounts = db.Sum<long>(d => d.VistiCounts);//浏览人数

            #region 下单量统计
            var orders = DbFactory.Default.Get<OrderInfo>().Where(e => e.OrderDate >= begin && e.OrderDate < end);
            if (shop > 0)
                orders = orders.Where(o => o.ShopId == shop);
            if (shopbranchId.HasValue)
                orders = orders.Where(o => o.ShopBranchId == shopbranchId);
            var orderList = orders.ToList();
            model.OrderCount = orderList.Count();//订单数
            model.OrderUserCount = orderList.Select(e => e.UserId).Distinct().Count();//下单人数
            model.OrderAmount = orderList.Sum(e => e.TotalAmount);//下单金额
            var orderids = orderList.Select(p => p.Id).ToList<long>();
            long orderProductQuantity = DbFactory.Default.Get<OrderItemInfo>()
                .Where(p => p.OrderId.ExIn(orderids))
                .Sum<long>(p => p.Quantity);
            model.OrderProductCount = orderProductQuantity;//下单件数
            #endregion

            #region 付款量统计
            var payOrders = DbFactory.Default.Get<OrderInfo>().Where(e => e.PayDate >= begin && e.PayDate < end);
            if (shop > 0)
                payOrders = payOrders.Where(o => o.ShopId == shop);
            if (shopbranchId.HasValue)
                payOrders = payOrders.Where(o => o.ShopBranchId == shopbranchId);
            var payOrderList = payOrders.ToList();
            model.OrderPayUserCount = payOrderList.Select(e => e.UserId).Distinct().Count();//付款人数
            model.OrderPayCount = payOrderList.Count();//付款订单数
            model.SaleAmounts = payOrderList.Sum(e => e.TotalAmount);//付款金额
            orderids = payOrderList.Select(p => p.Id).ToList<long>();
            orderProductQuantity = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId.ExIn(orderids)).Sum<long>(p => p.Quantity);
            model.SaleCounts = orderProductQuantity;//付款下单件数
            #endregion

            #region 退款量统计
            var refunds = DbFactory.Default.Get<OrderRefundInfo>().Where(p => p.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed);
            if (shop > 0)
                refunds = refunds.Where(r => r.ShopId == shop);
            if (shopbranchId.HasValue)
            {
                var ids = DbFactory.Default.Get<OrderInfo>().Where(o => o.ShopBranchId == shopbranchId.Value).Select(i => i.Id).ToList<long>();
                refunds = refunds.Where(r => r.OrderId.ExIn(ids));
            }
            refunds = refunds.Where(p => p.ManagerConfirmDate >= begin && p.ManagerConfirmDate <= end);

            var refundList = refunds.ToList();
            model.OrderRefundProductCount = refundList.Sum(p => (long)p.ReturnQuantity);//退款件数
            model.OrderRefundAmount = refundList.Sum(p => (decimal)p.Amount);//退款金额
                        
            var _refundOrderCount = refundList.Select(p => p.OrderId).Distinct().Count();
            model.OrderRefundCount = _refundOrderCount;//退款订单数
            #endregion

            models.Add(model);
            return models;
        }

        public List<TradeStatisticModel> GetPlatVisits(DateTime begin, DateTime end)
        {
            return DbFactory.Default.Get<PlatVisitInfo>().Where(e => e.Date >= begin && e.Date < end).ToList<TradeStatisticModel>();
        }
        /// <summary>
        /// 获取商品销量排行数据
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="rankSize"></param>
        /// <returns>[ProductName:SaleCount]</returns>
        public List<ChartDataItem<long, int>> GetProductChartBySaleCount(long shop, DateTime begin, DateTime end, int rankSize)
        {
            var data = DbFactory.Default
                .Get<ProductVistiInfo>()
                .LeftJoin<ProductInfo>((pv, p) => pv.ProductId == p.Id)
                .Where(pv => pv.Date >= begin && pv.Date < end)
                .Where<ProductInfo>(p => p.ShopId == shop && p.IsDeleted == false)
                .Select(pv => new { ItemKey = pv.ProductId, ItemValue = pv.SaleCounts.ExSum() })
                .Select<ProductInfo>(p => new { Expand = p.ProductName })
                .GroupBy(n => n.ProductId)
                .OrderByDescending(n => "ItemValue")
                .Take(rankSize);
            if (shop > 0)
                data.Where(p => p.ShopId == 0);
            return data.ToList<ChartDataItem<long, int>>();
        }
        /// <summary>
        /// 获取商品销售额排行数据
        /// </summary>
        /// <param name="shop">为0 不筛选</param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="rankSize"></param>
        /// <returns></returns>
        public List<ChartDataItem<long, decimal>> GetProductChartBySaleAmount(long shop, DateTime begin, DateTime end, int rankSize)
        {
            var data = DbFactory.Default
                 .Get<ProductVistiInfo>()
                 .LeftJoin<ProductInfo>((pv, p) => pv.ProductId == p.Id)
                 .Where(pv => pv.Date >= begin && pv.Date < end)
                 .Where<ProductInfo>(p => p.IsDeleted == false)
                 .Select(pv => new { ItemKey = pv.ProductId, ItemValue = pv.SaleAmounts.ExSum() })
                 .Select<ProductInfo>(p => new { Expand = p.ProductName })
                 .GroupBy(n => n.ProductId)
                 .OrderByDescending(n => "ItemValue");
            if (shop > 0)
                data.Where(p => p.ShopId == shop);
            return data.ToList<ChartDataItem<long, decimal>>();
        }
        /// <summary>
        /// 获取商品访问量排行数据
        /// </summary>
        /// <returns></returns>
        public List<ChartDataItem<long, long>> GetProductChartByVisti(long shop, DateTime begin, DateTime end, int rankSize)
        {
            var data = DbFactory.Default
              .Get<ProductVistiInfo>()
              .LeftJoin<ProductInfo>((pv, p) => pv.ProductId == p.Id)
              .Where(pv => pv.Date >= begin && pv.Date < end)
              .Where<ProductInfo>(p => p.IsDeleted == false)
              .Select(pv => new { ItemKey = pv.ProductId, ItemValue = pv.VistiCounts.ExSum() })
              .Select<ProductInfo>(p => new { Expand = p.ProductName })
              .GroupBy(n => n.ProductId)
              .OrderByDescending(n => "ItemValue");
            if (shop > 0)
                data.Where(p => p.ShopId == shop);
            return data.ToList<ChartDataItem<long, long>>();
        }
        /// <summary>
        /// 获取店铺销量数据
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<ChartDataItem<DateTime, int>> GetShopBySaleCount(long shop, DateTime begin, DateTime end)
        {
            return DbFactory.Default
                .Get<ShopVistiInfo>()
                .Where(p => p.ShopId == shop && p.Date >= begin && p.Date < end)
                .Select(m => new { ItemKey = m.Date, ItemValue = m.SaleCounts.ExSum() })
                .GroupBy(m => new { m.Date })
                .ToList<ChartDataItem<DateTime, int>>();
        }
        /// <summary>
        /// 获取店铺流量数据
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<ChartDataItem<DateTime, int>> GetShopFlowChart(long shop, DateTime begin, DateTime end)
        {
            return DbFactory.Default.Get<ShopVistiInfo>()
                .Where(p => p.ShopId == shop && p.Date >= begin && p.Date < end)
                .Select(m => new { ItemKey = m.Date, ItemValue = m.VistiCounts.ExSum() })
                .GroupBy(m => m.Date)
                .ToList<ChartDataItem<DateTime, int>>();
        }
        /// <summary>
        /// 获取门店转化率(SaleCounts/VistiCounts)
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<ChartDataItem<DateTime, decimal>> GetConversionRate(long shop, DateTime begin, DateTime end)
        {
            return DbFactory.Default.Get<ShopVistiInfo>()
                .Where(m => m.ShopId == shop && m.Date >= begin && m.Date < end)
             .Select(m => new { m.Date, m.VistiCounts, m.SaleCounts, })
             .GroupBy(m => new { m.Date.Date })
             .ToList<dynamic>()
             .Select(item => new ChartDataItem<DateTime, decimal>
             {
                 ItemKey = (DateTime)item.Date,
                 ItemValue = item.VistiCounts > 0 ? ((decimal)item.SaleCounts) / item.VistiCounts : 0
             }).ToList();
        }
        /// <summary>
        /// 获取新增店铺数据
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<ChartDataItem<DateTime, int>> GetNewShopChart(DateTime begin, DateTime end)
        {
            var data = DbFactory.Default.Get<ShopInfo>().Where(m => m.CreateDate >= begin && m.CreateDate < end && m.Stage == ShopInfo.ShopStage.Finish)
               .Select(m => new { ItemKey = m.CreateDate, ItemValue = m.ExCount(false) })
               .GroupBy(m => new { m.CreateDate.Date })
               .ToList<ChartDataItem<DateTime, int>>();
            return data;
        }
        /// <summary>
        /// 获取新增会员数据
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<ChartDataItem<DateTime, int>> GetMemberChart(DateTime begin, DateTime end)
        {
            var data = DbFactory.Default.Get<MemberInfo>().Where(m => m.CreateDate >= begin && m.CreateDate < end)
            .Select(m => new { ItemKey = m.CreateDate, ItemValue = m.ExCount(false) })
            .GroupBy(m => new { m.CreateDate.Date })
            .ToList<ChartDataItem<DateTime, int>>();
            return data;
        }
        /// <summary>
        /// 获取店铺订单量排行数据
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="rankSize"></param>
        /// <returns>[ShopId,OrderCount,ShopName]</returns>
        public List<ChartDataItem<long, int>> GetShopRankingByOrderCount(DateTime begin, DateTime end, int rankSize)
        {
            return DbFactory.Default.Get<OrderInfo>()
              .LeftJoin<ShopInfo>((oi, si) => oi.ShopId == si.Id)
              .Where(o => o.OrderDate >= begin && o.OrderDate < end
                && o.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay
                && o.OrderStatus != OrderInfo.OrderOperateStatus.Close)
              .GroupBy(n => n.ShopId)
              .Select(o => new { ItemKey = o.ShopId, ItemValue = o.ExCount(false) })
              .Select<ShopInfo>(s => new { Expand = s.ShopName })
              .OrderByDescending(o => "ItemValue")
              .ToList<ChartDataItem<long, int>>();
        }
        /// <summary>
        /// 获取店铺销售额排行数据
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="rankSize"></param>
        /// <returns>[ShopId,SaleAmount,ShopName]</returns>
        public List<ChartDataItem<long, decimal>> GetShopRankingBySaleAmount(DateTime begin, DateTime end, int rankSize)
        {
            return DbFactory.Default
              .Get<ShopVistiInfo>()
              .LeftJoin<ShopInfo>((v, s) => v.ShopId == s.Id)
              .Where(p => p.Date >= begin && p.Date < end)
              .GroupBy(s => s.ShopId)
              .Select(p => new { ItemKey = p.ShopId, ItemValue = p.SaleAmounts.ExSum() })
              .Select<ShopInfo>(s => new { Expand = s.ShopName })
              .OrderByDescending(n => "ItemValue")
              .Take(rankSize)
              .ToList<ChartDataItem<long, decimal>>();
        }
        /// <summary>
        /// 获取区域订单量
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<ChartDataItem<long, int>> GetAreaMapByOrderCount(DateTime begin, DateTime end)
        {
            //TODO:FG [BUG]自提订单需设置TopRegionId字段
            var exclude = new[] { OrderInfo.OrderOperateStatus.Close, OrderInfo.OrderOperateStatus.WaitPay };
            return DbFactory.Default.Get<OrderInfo>()
                .Where(p => p.OrderDate >= begin && p.OrderDate < end && p.OrderStatus.ExNotIn(exclude) && p.TopRegionId > 0)
                .GroupBy(p => p.TopRegionId)
                .Select(p => new { ItemKey = p.TopRegionId, ItemValue = p.ExCount(false) })
                .ToList<ChartDataItem<long, int>>();
        }
        /// <summary>
        /// 获取区域订单金额
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<ChartDataItem<long, decimal>> GetAreaMapByOrderAmount(DateTime begin, DateTime end)
        {
            //TODO:FG [BUG]自提订单需设置TopRegionId字段
            var exclude = new[] { OrderInfo.OrderOperateStatus.Close, OrderInfo.OrderOperateStatus.WaitPay };
            return DbFactory.Default.Get<OrderInfo>()
                .Where(p => p.OrderDate >= begin && p.OrderDate < end && p.OrderStatus.ExNotIn(exclude) && p.TopRegionId > 0)
                .GroupBy(p => p.TopRegionId)
                .Select(p => new { ItemKey = p.TopRegionId, ItemValue = (p.ProductTotalAmount + p.Freight + p.Tax).ExSum() })
                .ToList<ChartDataItem<long, decimal>>();
        }
        public void AddPlatVisitUser(DateTime date, long number)
        {
            //var platVisit = Context.PlatVisitsInfo.FirstOrDefault(e => e.Date == dt);
            var platVisit = DbFactory.Default.Get<PlatVisitInfo>().Where(e => e.Date == date).FirstOrDefault();
            if (platVisit == null)
            {
                platVisit = new PlatVisitInfo();
                platVisit.Date = date;
                platVisit.VisitCounts = 1;
                //Context.PlatVisitsInfo.Add(platVisit);
                DbFactory.Default.Add(platVisit);
            }
            else
            {
                platVisit.VisitCounts += number;
                DbFactory.Default.Update(platVisit);
            }
            //Context.SaveChanges();
        }
        public void AddShopVisitUser(DateTime date, long shop, long number)
        {
            date = date.Date;
            var success = DbFactory.Default.Set<ShopVistiInfo>()
                .Where(p => p.Date == date && p.ShopId == shop)
                .Set(s => s.VistiCounts, v => v.VistiCounts + number)
                .Succeed();
            if (!success)
            {
                DbFactory.Default.Add(new ShopVistiInfo
                {
                    Date = date,
                    VistiCounts = number,
                    ShopId = shop,
                    ShopBranchId = 0
                });
            }
        }

        public void AddShopBranchVisitUser(DateTime date, long shopId, long shopBranchId, long num)
        {
            date = date.Date;
            var success = DbFactory.Default.Set<ShopVistiInfo>()
                .Where(p => p.Date == date && p.ShopId == shopId && p.ShopBranchId == shopBranchId)
                .Set(s => s.VistiCounts, v => v.VistiCounts + num)
                .Succeed();
            if (!success)
            {
                DbFactory.Default.Add(new ShopVistiInfo
                {
                    Date = date,
                    VistiCounts = num,
                    ShopId = shopId,
                    ShopBranchId = shopBranchId
                });
            }
        }

        public void AddProductVisitUser(DateTime date, long product, long shop, long num)
        {
            date = date.Date;
            var success = DbFactory.Default.Set<ProductVistiInfo>()
               .Where(p => p.Date == date && p.ProductId == product && p.ShopId == shop)
               .Set(p => p.VisitUserCounts, n => n.VisitUserCounts + num)
               .Succeed();
            if (!success)
            {
                DbFactory.Default.Add(new ProductVistiInfo
                {
                    Date = date,
                    ShopId = shop,
                    ProductId = product,
                    VistiCounts = num
                });
            }
        }
        public void AddProductVisit(DateTime date, long product, long shop, long num)
        {
            date = date.Date;
            var success = DbFactory.Default.Set<ProductVistiInfo>()
                .Where(p => p.Date == date && p.ProductId == product && p.ShopId == shop)
                .Set(p => p.VistiCounts, n => n.VistiCounts + num)
                .Succeed();
            if (!success)
            {
                DbFactory.Default.Add(new ProductVistiInfo
                {
                    Date = date,
                    ShopId = shop,
                    ProductId = product,
                    VistiCounts = num
                });
            }
        }
    }
}
