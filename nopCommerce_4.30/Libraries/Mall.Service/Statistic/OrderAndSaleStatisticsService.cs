using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Mall.Service
{
    /// <summary>
    /// 订单和销售统计
    /// </summary>
    public class OrderAndSaleStatisticsService : ServiceBase, IOrderAndSaleStatisticsService
    {
        /// <summary>
        /// 获取会员沉睡日
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public int GetUserSleepDays(long userid)
        {
            var last = DbFactory.Default.Get<OrderInfo>()
                .Where(p => p.UserId == userid && p.PayDate.ExIsNotNull())
                .OrderByDescending(p => p.Id)
                .FirstOrDefault();
            if (last != null)
                return (int)(DateTime.Now - last.OrderDate).TotalDays;
            return -1;
        }


        /// <summary>
        /// 获取店铺下门店销售额（按天排序）
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="branchShopId"></param>
        /// <returns></returns>
        public List<BranchShopDayFeat> GetDayAmountSale(DateTime start, DateTime end, long branchShopId)
        {
            var en = end.AddDays(1);
            var orders = DbFactory.Default
                .Get<OrderInfo>()
                .Where(a => a.ShopBranchId == branchShopId && a.PayDate.ExIsNotNull() && a.PayDate.Value > start &&
                    a.PayDate.Value < en && a.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay &&
                    a.OrderStatus != OrderInfo.OrderOperateStatus.Close);
            var result = orders.ToList().GroupBy(a => a.PayDate.Value.Date).Select(a => new BranchShopDayFeat()
            {
                BranchShopId = branchShopId,
                SaleAmount = a.Sum(x => (decimal?)x.ActualPayAmount).GetValueOrDefault(),
                Day = a.Key
            });
            var day = (end - start).Days;
            List<BranchShopDayFeat> list = new List<BranchShopDayFeat>();
            for (int i = 0; i <= day; i++)
            {
                BranchShopDayFeat m = new BranchShopDayFeat();
                m.BranchShopId = branchShopId;
                var time = start.Date.AddDays(i);
                m.Day = time;
                var ex = result.Where(a => a.Day.Date == time).FirstOrDefault();
                if (ex != null)
                {
                    m.SaleAmount = ex.SaleAmount;
                    m.Day = ex.Day.Date;
                }
                list.Add(m);
            }
            return list.OrderByDescending(a => a.Day).ToList();
        }

        /// <summary>
        /// 获取门店在某天的销售排行 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="shopId"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        public int GetRank(DateTime date, long shopId, decimal Amount)
        {
            return DbFactory.Default
                .Get<OrderInfo>()
                .Select(n => n.Id)
                .Where(n => n.ShopId == shopId && n.ShopBranchId.ExIfNull(0) > 0 && n.OrderDate.Date == date)
                .GroupBy(n => n.ShopBranchId)
                .Having(n => n.ActualPayAmount.ExSum() > Amount)
                .Count() + 1;
        }


        public QueryPageModel<BranchShopFeat> GetBranchShopFeat(BranchShopFeatsQuery query)
        {
            if (query.StartDate > query.EndDate)
            {
                throw new MallException("时间段异常：开始时间大于结束时间！");
            }
            //var orders = Context.OrderInfo.Where(p => p.ShopId == query.ShopId && p.ShopBranchId.HasValue && p.ShopBranchId.Value!=0);
            var orders = DbFactory.Default.Get<OrderInfo>().Where(p => p.ShopId == query.ShopId && p.ShopBranchId.ExIsNotNull() && p.ShopBranchId != 0 &&
                p.OrderStatus != OrderInfo.OrderOperateStatus.Close);
            if (query.StartDate.HasValue)
            {
                orders.Where(p => p.PayDate >= query.StartDate);
            }
            if (query.EndDate.HasValue)
            {
                orders.Where(p => p.PayDate < query.EndDate);
            }
   
            orders.GroupBy(a => a.ShopBranchId).Select(b => new
            {
                BranchShopId = b.ShopBranchId,
                ShopId = query.ShopId,
                SaleAmount = b.ActualPayAmount.ExSum(),
                Rank = b.ExRowNo()
            }).OrderByDescending(x => "SaleAmount");
        
            var datas = orders.ToPagedList<BranchShopFeat>(query.PageNo, query.PageSize);

            //foreach (var t in datas)
            //{
            //    index++;
            //    t.Rank += index;
            //}
            QueryPageModel<BranchShopFeat> pageModel = new QueryPageModel<BranchShopFeat>()
            {
                Models = datas,
                Total = datas.TotalRecordCount
            };
            return pageModel;
        }




        public SaleStatistics GetSaleStatisticsByShop(long shopId, DateTime? startTime, DateTime? endTime)
        {
            if (startTime > endTime)
            {
                throw new MallException("时间段异常：开始时间大于结束时间！");
            }
            var saleStatistics = new SaleStatistics();
            saleStatistics.DealCount = 0;
            saleStatistics.SaleCount = 0;
            saleStatistics.SalesVolume = 0;

            //临时变量
            long _saleCount = 0;
            long _dealCount = 0;
            decimal _salesVolume = 0;
            decimal _refundRate = 0;
            int _payOrderCount = 0;
            int _refundOrderCount = 0;


            #region 当前数据

            //统计已付款的订单
            //var orders = Context.OrderInfo.Where(p => p.ShopId == shopId);
            var orders = DbFactory.Default.Get<OrderInfo>().Where(p => p.ShopId == shopId);
            if (startTime.HasValue)
            {
                orders.Where(p => p.PayDate >= startTime);
            }
            if (endTime.HasValue)
            {
                orders.Where(p => p.PayDate <= endTime);
            }
            //已付款的订单（包括关闭的）用于计算退款率
            var payOrders = orders.Where(p => p.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay).ToList();
            var payOrderIds = payOrders.Select(e => e.Id).ToList();
            var dealOrders = payOrders.Where(p => p.OrderStatus != OrderInfo.OrderOperateStatus.Close);
            _payOrderCount += payOrders.Count();
            var orderIds = dealOrders.Select(p => p.Id).ToList();
            //var orderItems = Context.OrderItemInfo.Where(p => orderIds.Contains(p.OrderId));
            var orderItems = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId.ExIn(orderIds));

            //_saleCount = (int)orderItems.ToList().Sum(p => (long?)p.Quantity).GetValueOrDefault(0);
            _saleCount = orderItems.Sum<long>(p => p.Quantity);
            _dealCount = dealOrders.Count();
            //_salesVolume = dealOrders.Sum(p => (decimal?)p.ActualPayAmount).GetValueOrDefault(0);
            _salesVolume = dealOrders.Sum(p => p.ActualPayAmount);

            //var orderRefunds = Context.OrderRefundInfo.Where(p => orderIds.Contains(p.OrderId) && p.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed).ToList();
            var orderRefunds = DbFactory.Default.Get<OrderRefundInfo>().Where(p => p.OrderId.ExIn(orderIds) && p.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed).ToList();
            if (orderRefunds.Count() > 0)
            {
                //_saleCount = _saleCount - (int)orderRefunds.Sum(p => (long?)p.ReturnQuantity).GetValueOrDefault(0);
                _saleCount = _saleCount - orderRefunds.Sum(p => (long)p.ReturnQuantity);
            }

            saleStatistics.DealCount += _dealCount;
            saleStatistics.SaleCount += _saleCount;
            saleStatistics.SalesVolume += _salesVolume;
            saleStatistics.RefundRate += _refundRate;

            #endregion

            //退款率
            //在筛选时间内退款成功的订单
            //var refunds = Context.OrderRefundInfo.Where(p => payOrderIds.Contains(p.OrderId) && p.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed);
            var refunds = DbFactory.Default.Get<OrderRefundInfo>().Where(p => p.OrderId.ExIn(payOrderIds) && p.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed);

            if (startTime.HasValue)
            {
                refunds.Where(p => p.ManagerConfirmDate >= startTime);
            }
            if (endTime.HasValue)
            {
                refunds.Where(p => p.ManagerConfirmDate <= endTime);
            }
            _refundOrderCount = refunds.Select(p => p.OrderId).Distinct().Count();
            if (_payOrderCount > 0)
            {
                _refundRate = Math.Round(_refundOrderCount / (decimal)_payOrderCount, 2);
            }
            saleStatistics.RefundRate = _refundRate;

            //计算参数
            if (saleStatistics.DealCount > 0)
            {
                saleStatistics.OrderPrice = saleStatistics.SalesVolume / saleStatistics.DealCount;
                saleStatistics.OrderAverage = (decimal)saleStatistics.SaleCount / saleStatistics.DealCount;
                if (saleStatistics.SaleCount > 0)
                {
                    saleStatistics.OrderItemPrice = saleStatistics.SalesVolume / saleStatistics.SaleCount;
                }
            }

            return saleStatistics;
        }


        #region 会员购买商品类别冗余统计


        /// <summary>
        /// 会员购买商品类别添加
        /// </summary>
        /// <param name="model"></param>
        public void AddMemberBuyCategory(MemberBuyCategoryInfo model)
        {
            //Context.MemberBuyCategoryInfo.Add(model);
            //Context.SaveChanges();
            DbFactory.Default.Add(model);
        }


        /// <summary>
        /// 会员购买商品类别查询
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MemberBuyCategoryInfo GetMemberBuyCategory(long categoryId, long userId)
        {
            //return Context.MemberBuyCategoryInfo.Where(p => p.CategoryId == categoryId && p.UserId == userId).FirstOrDefault();
            return DbFactory.Default.Get<MemberBuyCategoryInfo>().Where(p => p.CategoryId == categoryId && p.UserId == userId).FirstOrDefault();
        }

        /// <summary>
        /// 会员购买商品类别删除
        /// </summary>
        /// <param name="id"></param>
        public void DeleteMemberBuyCategory(long id)
        {
            //Context.MemberBuyCategoryInfo.Remove(item => item.Id == id);
            //Context.SaveChanges();
            DbFactory.Default.Del<MemberBuyCategoryInfo>(item => item.Id == id);
        }

        /// <summary>
        /// 会员购买商品类别修改
        /// </summary>
        /// <param name="model"></param>
        public void UpdateMemberBuyCategory(MemberBuyCategoryInfo model)
        {
            //var m = Context.MemberBuyCategoryInfo.Where(p => p.Id == model.Id).FirstOrDefault();
            //m.OrdersCount = model.OrdersCount;
            //Context.SaveChanges();
            DbFactory.Default.Set<MemberBuyCategoryInfo>().Set(n => n.OrdersCount, model.OrdersCount).Where(p => p.Id == model.Id).Succeed();
        }
        #endregion
        /// <summary>
        /// 商家、门店今日销售额统计
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="branchShopId"></param>
        /// <returns></returns>
        public decimal GetTodaySaleAmount(long shopId, long? branchShopId)
        {
            var orders = DbFactory.Default.Get<OrderInfo>().Where(p => p.ShopId == shopId && p.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay && p.OrderStatus != OrderInfo.OrderOperateStatus.Close);
            if (branchShopId.HasValue)
            {
                orders.Where(e => e.ShopBranchId == branchShopId.Value);
            }
            var startDate = DateTime.Now.Date;
            var endDate = DateTime.Now.Date.AddDays(1);
            orders.Where(e => e.PayDate >= startDate && e.PayDate < endDate);
            var amount = orders.Sum(e => e.ActualPayAmount);
            return amount;
        }

        public SaleStatistics GetBranchShopSaleStatistics(long branchShopId, DateTime? startTime, DateTime? endTime)
        {
            if (startTime > endTime)
            {
                throw new MallException("时间段异常：开始时间大于结束时间！");
            }
            var saleStatistics = new SaleStatistics();
            saleStatistics.DealCount = 0;
            saleStatistics.SaleCount = 0;
            saleStatistics.SalesVolume = 0;

            //临时变量
            long _saleCount = 0;
            long _dealCount = 0;
            decimal _salesVolume = 0;
            decimal _refundRate = 0;
            int _payOrderCount = 0;
            int _refundOrderCount = 0;


            #region 当前数据

            //统计已付款的订单
            //var orders = Context.OrderInfo.Where(p => p.ShopBranchId == branchShopId);
            var orders = DbFactory.Default.Get<OrderInfo>().Where(p => p.ShopBranchId == branchShopId);
            if (startTime.HasValue)
            {
                orders.Where(p => p.PayDate >= startTime);
            }
            if (endTime.HasValue)
            {
                orders.Where(p => p.PayDate <= endTime);
            }
            //已付款的订单（包括关闭的）用于计算退款率
            var payOrders = orders.Where(p => p.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay).ToList();
            var payOrderIds = payOrders.Select(e => e.Id).ToList();
            var dealOrders = payOrders.Where(p => p.OrderStatus != OrderInfo.OrderOperateStatus.Close);
            var orderIds = dealOrders.Select(p => p.Id).ToList();
            _payOrderCount += payOrders.Count();
            //var orderItems = Context.OrderItemInfo.Where(p => orderIds.Contains(p.OrderId));
            var orderItems = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId.ExIn(orderIds));

            //_saleCount = (int)orderItems.ToList().Sum(p => (long?)p.Quantity).GetValueOrDefault(0);
            _saleCount = orderItems.Sum<long>(p => p.Quantity);
            _dealCount = dealOrders.Count();
            //_salesVolume = dealOrders.Sum(p => (decimal?)p.ActualPayAmount).GetValueOrDefault(0);
            _salesVolume = dealOrders.Sum(p => p.ActualPayAmount);

            //var orderRefunds = Context.OrderRefundInfo.Where(p => orderIds.Contains(p.OrderId) && p.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed).ToList();
            var orderRefunds = DbFactory.Default.Get<OrderRefundInfo>().Where(p => p.OrderId.ExIn(orderIds) && p.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed).ToList();
            if (orderRefunds.Count() > 0)
            {
                //_saleCount = _saleCount - (int)orderRefunds.Sum(p => (long?)p.ReturnQuantity).GetValueOrDefault(0);
                _saleCount = _saleCount - orderRefunds.Sum(p => (long)p.ReturnQuantity);
            }

            saleStatistics.DealCount += _dealCount;
            saleStatistics.SaleCount += _saleCount;
            saleStatistics.SalesVolume += _salesVolume;
            saleStatistics.RefundRate += _refundRate;

            #endregion

            //退款率
            //在筛选时间内退款成功的订单
            var refunds = DbFactory.Default.Get<OrderRefundInfo>().Where(p => p.OrderId.ExIn(payOrderIds) && p.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed);

            if (startTime.HasValue)
            {
                refunds.Where(p => p.ManagerConfirmDate >= startTime);
            }
            if (endTime.HasValue)
            {
                refunds.Where(p => p.ManagerConfirmDate <= endTime);
            }
            _refundOrderCount = refunds.Select(p => p.OrderId).Distinct().Count();
            if (_payOrderCount > 0)
            {
                _refundRate = Math.Round((decimal)_refundOrderCount / (decimal)_payOrderCount, 2);
            }
            saleStatistics.RefundRate = _refundRate;

            //计算参数
            if (saleStatistics.DealCount > 0)
            {
                saleStatistics.OrderPrice = saleStatistics.SalesVolume / saleStatistics.DealCount;
                saleStatistics.OrderAverage = (decimal)saleStatistics.SaleCount / saleStatistics.DealCount;
                if (saleStatistics.SaleCount > 0)
                {
                    saleStatistics.OrderItemPrice = saleStatistics.SalesVolume / saleStatistics.SaleCount;
                }
            }

            return saleStatistics;
        }
    }
}
