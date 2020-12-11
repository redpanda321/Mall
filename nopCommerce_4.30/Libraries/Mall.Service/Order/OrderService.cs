using Mall.Application;
using Mall.CommonModel;
using Mall.CommonModel.Delegates;
using Mall.Core;
using Mall.Core.Plugins.Message;
using Mall.Core.Plugins.Payment;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.ServiceProvider;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NetRube.Data;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Service
{
    public class OrderService : ServiceBase, IOrderService
    {
        #region 静态字段
        private static readonly System.Security.Cryptography.RandomNumberGenerator _randomPickupCode = System.Security.Cryptography.RandomNumberGenerator.Create();
        private const string PAY_BY_CAPITAL_PAYMENT_ID = "预存款支付";
        private const string PAY_BY_OFFLINE_PAYMENT_ID = "线下收款";
        private const string PAY_BY_INTEGRAL_PAYMENT_ID = "积分支付";
        #endregion

        #region 字段
        private IFightGroupService _iFightGroupService;
        private IShopService _iShopService;
        private IShippingAddressService _iShippingAddressService;
        private IRegionService _iRegionService;
        private IShopBranchService _iShopBranchService;
        private IAppMessageService _iAppMessageService;
        private IShopBonusService _iShopBonusService;
        private IProductLadderPriceService _iProductLadderPriceService;
        private ICommentService _iCommentService;
        //private ITypeService _iTypeService;
        #endregion

        #region 构造函数
        public OrderService()
        {
            _iShopBonusService = Instance<IShopBonusService>.Create;
            _iFightGroupService = Instance<IFightGroupService>.Create;
            _iShopService = Instance<IShopService>.Create;
            _iShippingAddressService = Instance<IShippingAddressService>.Create;
            _iRegionService = Instance<IRegionService>.Create;
            _iShopBranchService = Instance<IShopBranchService>.Create;
            _iAppMessageService = Instance<IAppMessageService>.Create;
            _iProductLadderPriceService = Instance<IProductLadderPriceService>.Create;
            _iCommentService = Instance<ICommentService>.Create;
        }
        #endregion

        #region 属性
        public event OrderPaySuccessed OnOrderPaySuccessed;
        #endregion

        #region 方法
        #region 获取订单相关 done
        public QueryPageModel<OrderInfo> GetOrders<Tout>(OrderQuery query, Expression<Func<OrderInfo, Tout>> sort = null)
        {
            var flag = SiteSettingApplication.SiteSettings.IsOpenHistoryOrder;
            GetBuilder<OrderInfo> orders = null;
            var history = DbFactory.MongoDB.AsQueryable<OrderInfo>();
            if (query.Status != OrderInfo.OrderOperateStatus.History)
            {
                orders = DbFactory.Default.Get<OrderInfo>();
            }
            history = ToWhere(orders, history, query);

            var temp = 0;
            var rets = new List<OrderInfo>();
            if (sort == null)
            {
                if (null != orders)
                    orders.OrderByDescending(item => item.OrderDate);
                history = history.OrderByDescending(item => item.OrderDate);
            }
            else
            {
                if (null != orders)
                    orders.OrderBy((Expression)sort);
                history = history.OrderBy(sort);
            }
            if (null == orders)
            {
                if (flag)//就算orders查不到数据，但如果没开启历史订单，应该也不能执行
                {
                    temp = history.Count();
                    rets = history.Skip((query.PageNo - 1) * query.PageSize).Take(query.PageSize).ToList();
                }
            }
            else
            {
                var result = orders.ToPagedList(query.PageNo, query.PageSize);
                rets = result;
                temp = result.TotalRecordCount;
            }

            var total = temp + (!flag || query.Status == OrderInfo.OrderOperateStatus.History || query.Operator != Operator.None ? 0 : history.Count(item => item.Id > 0));
            if (flag && rets.Count < query.PageSize && query.Status != OrderInfo.OrderOperateStatus.History && query.Operator == Operator.None)//开启历史记录才从mongodb获取
            {
                query.PageNo -= (int)Math.Ceiling((float)(temp / query.PageSize));
                temp = history.Count();
                var historyorders = history.OrderByDescending(item => item.OrderDate).Skip((query.PageNo - 1) * query.PageSize).Take(query.PageSize).ToList();

                if (rets.Count > 0)
                {
                    rets.AddRange(historyorders.Take(query.PageSize - rets.Count));
                }
                else
                {
                    rets.AddRange(historyorders);
                }
            }
            var orderIds = rets.Select(p => p.Id).ToList();
            var allOrderItems = DbFactory.Default.Get<OrderItemInfo>(p => p.OrderId.ExIn(orderIds)).ToList();

            foreach (var orderInfo in rets)
            {
                var orderitems = allOrderItems.Where(p => p.OrderId == orderInfo.Id).ToList();
                if (orderitems.Count <= 0)
                {
                    orderitems = GetOrderItemsByOrderId(orderInfo.Id);
                }
                foreach (var itemInfo in orderitems)
                {
                    var typeInfo = DbFactory.Default
                        .Get<TypeInfo>()
                        .InnerJoin<ProductInfo>((ti, pi) => ti.Id == pi.TypeId && pi.Id == itemInfo.ProductId)
                        .FirstOrDefault();
                    var prodata = DbFactory.Default
                        .Get<ProductInfo>().Where(pi => pi.Id == itemInfo.ProductId).FirstOrDefault();
                    itemInfo.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    itemInfo.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    itemInfo.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                    if (prodata != null)
                    {
                        itemInfo.ColorAlias = !string.IsNullOrWhiteSpace(prodata.ColorAlias) ? prodata.ColorAlias : itemInfo.ColorAlias;
                        itemInfo.SizeAlias = !string.IsNullOrWhiteSpace(prodata.SizeAlias) ? prodata.SizeAlias : itemInfo.SizeAlias;
                        itemInfo.VersionAlias = !string.IsNullOrWhiteSpace(prodata.VersionAlias) ? prodata.VersionAlias : itemInfo.VersionAlias;
                    }
                }
            }
            var pageModel = new QueryPageModel<OrderInfo>()
            {
                Models = rets,
                Total = total
            };

            return pageModel;
        }

        /// <summary>
        /// 分页获取订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<OrderInfo> GetOrders(OrderQuery query)
        {
            var flag = SiteSettingApplication.SiteSettings.IsOpenHistoryOrder;
            GetBuilder<OrderInfo> orders = null;
            var history = DbFactory.MongoDB.AsQueryable<OrderInfo>();
            if (query.Status != OrderInfo.OrderOperateStatus.History)
            {
                orders = DbFactory.Default.Get<OrderInfo>();
            }
            history = ToWhere(orders, history, query);
            var temp = 0;
            var data = new List<OrderInfo>();
            if (orders == null)
            {
                if (flag)
                {
                    temp = history.Count();
                    data = history.OrderByDescending(n => n.OrderDate).Skip((query.PageNo - 1) * query.PageSize).Take(query.PageSize).ToList();
                }
            }
            else
            {
                var result = orders.OrderByDescending(n => n.OrderDate).ToPagedList(query.PageNo, query.PageSize);
                data = result;
                temp = result.TotalRecordCount;
            }

            var total = temp + (!flag || query.Status == OrderInfo.OrderOperateStatus.History || query.Operator != Operator.None ? 0 : history.Count(item => item.Id > 0));
            if (flag && data.Count < query.PageSize && query.Status != OrderInfo.OrderOperateStatus.History && query.Operator == Operator.None)//开启历史记录才从mongodb获取
            {
                query.PageNo -= (int)Math.Ceiling((float)(temp / query.PageSize));
                temp = history.Count();
                var historyorders = history.OrderByDescending(item => item.OrderDate).Skip((query.PageNo - 1) * query.PageSize).Take(query.PageSize).ToList();

                if (data.Count > 0)
                {
                    data.AddRange(historyorders.Take(query.PageSize - data.Count));
                }
                else
                {
                    data.AddRange(historyorders);
                }
            }

            return new QueryPageModel<OrderInfo>()
            {
                Models = data,
                Total = total
            };

        }

        public QueryPageModel<VerificationRecordInfo> GetVerificationRecords(VerificationRecordQuery query)
        {
            var db = WhereBuilder(query);
            db = db.OrderByDescending(p => p.VerificationTime);

            var data = db.ToPagedList(query.PageNo, query.PageSize);

            return new QueryPageModel<VerificationRecordInfo>()
            {
                Models = data,
                Total = data.TotalRecordCount
            };
        }

        private GetBuilder<VerificationRecordInfo> WhereBuilder(VerificationRecordQuery query)
        {
            var db = DbFactory.Default.Get<VerificationRecordInfo>();
            if (query.ShopBranchId.HasValue)
            {
                var ordersql = DbFactory.Default
                    .Get<OrderInfo>()
                    .Where<OrderInfo, VerificationRecordInfo>((si, pi) => si.Id == pi.OrderId && si.ShopBranchId == query.ShopBranchId.Value);
                db.Where(p => p.ExExists(ordersql));
            }
            if (query.ShopId.HasValue)
            {
                var ordersql = DbFactory.Default
                    .Get<OrderInfo>()
                    .Where<OrderInfo, VerificationRecordInfo>((si, pi) => si.Id == pi.OrderId && si.ShopId == query.ShopId.Value);
                db.Where(p => p.ExExists(ordersql));
            }

            if (!string.IsNullOrWhiteSpace(query.OrderId))
            {
                var _where = PredicateExtensions.False<VerificationRecordInfo>();
                _where = _where.Or(p => p.VerificationCodeIds.Contains(string.Format(",{0},", query.OrderId)));

                var orderIdRange = GetOrderIdRange(query.OrderId);
                if (orderIdRange != null)
                {
                    var min = orderIdRange[0];
                    if (orderIdRange.Length == 2)
                    {
                        var max = orderIdRange[1];
                        _where = _where.Or(item => item.OrderId >= min && item.OrderId <= max);
                    }
                    else
                        _where = _where.Or(item => item.OrderId == min);
                }
                db.Where(_where);
            }
            return db;
        }

        /// <summary>
        /// 获取订单列表(忽略分页)
        /// </summary>
        /// <param name="orderQuery"></param>
        /// <returns></returns>
        public List<OrderInfo> GetAllOrders(OrderQuery orderQuery)
        {
            GetBuilder<OrderInfo> orders = null;
            var flag = SiteSettingApplication.SiteSettings.IsOpenHistoryOrder;
            var history = DbFactory.MongoDB.AsQueryable<OrderInfo>();
            if (orderQuery.Status != OrderInfo.OrderOperateStatus.History)
            {
                orders = DbFactory.Default.Get<OrderInfo>().OrderByDescending(p => p.OrderDate);
            }
            history = ToWhere(orders, history, orderQuery).OrderByDescending(p => p.OrderDate);
            if (null == orders && flag) return history.ToList();
            return orders.ToList();
        }

        /// <summary>
        /// 获取增量订单
        /// </summary>
        /// <param name="orderQuery"></param>
        /// <returns></returns>
        public QueryPageModel<OrderInfo> GetOrdersByLastModifyTime(OrderQuery orderQuery)
        {
            var orders = DbFactory.Default.Get<OrderInfo>();
            if (orderQuery.ShopId.HasValue)
                orders.Where(n => n.ShopId == orderQuery.ShopId.Value);
            if (!string.IsNullOrEmpty(orderQuery.ShopName))
                orders.Where(n => n.ShopName.Contains(orderQuery.ShopName));
            if (!string.IsNullOrEmpty(orderQuery.UserName))
                orders.Where(n => n.UserName.Contains(orderQuery.UserName));
            if (orderQuery.UserId.HasValue)
                orders.Where(n => n.UserId == orderQuery.UserId.Value);
            if (!string.IsNullOrEmpty(orderQuery.PaymentTypeName))
                orders.Where(n => n.PaymentTypeName.Contains(orderQuery.PaymentTypeName));
            if (!string.IsNullOrEmpty(orderQuery.PaymentTypeGateway))
                orders.Where(n => n.PaymentTypeGateway.Contains(orderQuery.PaymentTypeGateway));
            if (orderQuery.IgnoreSelfPickUp.HasValue)
            {
                if (orderQuery.IgnoreSelfPickUp.Value)
                {
                    orders.Where(p => p.DeliveryType != DeliveryType.SelfTake);
                }
            }
            if (!string.IsNullOrEmpty(orderQuery.SearchKeyWords))
            {
                long result;
                bool IsNumber = long.TryParse(orderQuery.SearchKeyWords, out result);
                var productname = DbFactory.Default
                    .Get<OrderItemInfo>()
                    .Where<OrderInfo>((oii, oi) => oii.OrderId == oi.Id && oii.ProductName.Contains(orderQuery.SearchKeyWords));
                if (IsNumber)
                {
                    var productid = DbFactory.Default
                        .Get<OrderItemInfo>()
                        .Where<OrderInfo>((oii, oi) => oii.OrderId == oi.Id && oii.ProductId == result);
                    orders.Where(n => n.Id == result || n.ExExists(productid) || n.ExExists(productname));
                }
                else
                {
                    orders.Where(n => n.ExExists(productname));
                }
            }
            if (orderQuery.Commented.HasValue)
            {
                var commented = orderQuery.Commented.Value;
                if (commented)
                {

                    var sub = DbFactory.Default
                        .Get<OrderCommentInfo>()
                        .Where<OrderInfo>((oci, oi) => oci.OrderId == oi.Id);
                    orders.Where(p => p.ExExists(sub));
                }
                else
                {
                    var sub = DbFactory.Default
                        .Get<OrderCommentInfo>()
                        .Where<OrderInfo>((oci, oi) => oci.OrderId == oi.Id);

                    orders.Where(p => p.ExNotExists(sub));
                }
            }
            var orderIdRange = GetOrderIdRange(orderQuery.OrderId);
            if (orderIdRange != null)
            {
                var min = orderIdRange[0];
                if (orderIdRange.Length == 2)
                {
                    var max = orderIdRange[1];
                    orders.Where(item => item.Id >= min && item.Id <= max);
                }
                else
                    orders.Where(item => item.Id == min);
            }

            if (orderQuery.Commented.HasValue && !orderQuery.Commented.Value)
            {
                var pc = DbFactory.Default
                    .Get<ProductCommentInfo>()
                    .LeftJoin<OrderItemInfo>((pci, oii) => pci.SubOrderId == oii.Id)
                    .Select<OrderItemInfo>(n => n.OrderId)
                    .Distinct()
                    .ToList<long>();
                orders.Where(item => item.Id.ExNotIn(pc));
            }
            //订单类型
            if (orderQuery.OrderType.HasValue)
            {
                var platform = orderQuery.OrderType.Value.ToEnum(PlatformType.PC);
                orders.Where(item => item.Platform == platform);
            }
            if (orderQuery.Status.HasValue)
            {
                switch (orderQuery.Status.Value)
                {
                    case OrderInfo.OrderOperateStatus.UnComment:
                        //TODO:FG 查询待优化
                        var comments = DbFactory.Default.Get<OrderCommentInfo>().Select(p => p.OrderId);
                        orders.Where(d => d.Id.ExNotIn(comments) && d.OrderStatus == OrderInfo.OrderOperateStatus.Finish);
                        break;
                    case OrderInfo.OrderOperateStatus.WaitDelivery:
                        var fgordids = DbFactory.Default
                               .Get<FightGroupOrderInfo>()
                               .Where(d => d.JoinStatus != 4)
                               .Select(d => d.OrderId.ExIfNull(0))
                               .ToList<long>();

                        //处理拼团的情况
                        orders.Where(d => d.OrderStatus == orderQuery.Status && d.Id.ExNotIn(fgordids));
                        break;
                    case OrderInfo.OrderOperateStatus.History:
                        break;
                    default:
                        orders.Where(d => d.OrderStatus == orderQuery.Status);
                        break;
                }


                if (orderQuery.MoreStatus != null)
                {
                    foreach (var stitem in orderQuery.MoreStatus)
                    {
                        orders.Where(d => d.OrderStatus == stitem);
                    }
                }
            }

            if (orderQuery.PaymentType != OrderInfo.PaymentTypes.None)
            {
                orders.Where(item => item.PaymentType == orderQuery.PaymentType);
            }

            //开始结束时间
            if (orderQuery.StartDate.HasValue)
            {
                DateTime sdt = orderQuery.StartDate.Value;
                orders.Where(d => d.LastModifyTime >= sdt);
            }
            if (orderQuery.EndDate.HasValue)
            {
                DateTime edt = orderQuery.EndDate.Value.AddDays(1).AddSeconds(-1);
                orders.Where(d => d.LastModifyTime <= edt);
            }

            switch (orderQuery.Sort.ToLower())
            {
                case "commentcount":

                    break;
            }
            var rets = orders.OrderByDescending(item => item.OrderDate).ToPagedList(orderQuery.PageNo, orderQuery.PageSize);

            var pageModel = new QueryPageModel<OrderInfo>()
            {
                Models = rets,
                Total = rets.TotalRecordCount
            };
            return pageModel;
        }

        public List<OrderInfo> GetOrders(IEnumerable<long> ids)
        {
            return DbFactory.Default.Get<OrderInfo>().Where(item => item.Id.ExIn(ids)).ToList();
        }

        public OrderInfo GetOrder(long orderId, long userId)
        {
            var result = DbFactory.Default.Get<OrderInfo>().Where(a => a.Id == orderId && a.UserId == userId).FirstOrDefault();
            if (null == result)
            {
                var flag = SiteSettingApplication.SiteSettings.IsOpenHistoryOrder;
                if (flag)
                {
                    result = DbFactory.MongoDB.AsQueryable<OrderInfo>().Where(a => a.Id == orderId && a.UserId == userId).FirstOrDefault();
                    //result.OrderItemInfo = DbFactory.MongoDB.AsQueryable<OrderItemInfo>().Where(a => a.OrderId == orderId).ToList();
                }
            }
            if (result != null && result.OrderStatus >= OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                CalculateOrderItemRefund(orderId);
            }
            return result;
        }

        public OrderInfo GetOrder(long orderId)
        {
            var result = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == orderId).FirstOrDefault();
            if (null == result)
            {
                var flag = SiteSettingApplication.SiteSettings.IsOpenHistoryOrder;
                if (flag)
                {
                    result = DbFactory.MongoDB.AsQueryable<OrderInfo>().FirstOrDefault(p => p.Id == orderId);
                    //result.OrderItemInfo = DbFactory.MongoDB.AsQueryable<OrderItemInfo>().Where(a => a.OrderId == orderId).ToList();
                }
            }
            return result;
        }
        /// <summary>
        /// 是否存在订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopId">店铺Id,0表示不限店铺</param>
        /// <returns></returns>
        public bool IsExistOrder(long orderId, long shopId = 0)
        {
            var sql = DbFactory.Default.Get<OrderInfo>().Where(d => d.Id == orderId);
            if (shopId > 0)
            {
                sql = sql.Where(d => d.ShopId == shopId);
            }
            return sql.Exist();
        }

        public OrderInfo GetFirstFinishedOrderForSettlement()
        {
            var order = DbFactory.Default
                .Get<OrderInfo>()
                .Where(c => c.OrderStatus == OrderInfo.OrderOperateStatus.Finish)
                .OrderBy(c => c.FinishDate)
                .FirstOrDefault();
            return order;
        }

        public List<OrderPayInfo> GetOrderPay(long id)
        {
            return DbFactory.Default.Get<OrderPayInfo>().Where(item => item.PayId == id).ToList();
        }

        public List<OrderInfo> GetTopOrders(int top, long userId)
        {
            return DbFactory.Default.Get<OrderInfo>().Where(a => a.UserId == userId)
                .OrderByDescending(a => a.OrderDate).Take(top).ToList();
        }

        public List<OrderComplaintInfo> GetOrderComplaintByOrders(List<long> orders)
        {
            return DbFactory.Default.Get<OrderComplaintInfo>().Where(p => p.OrderId.ExIn(orders)).ToList();
        }
        public int GetFightGroupOrderCountByUser(long userId)
        {
            return DbFactory.Default
                .Get<OrderInfo>()
                .InnerJoin<FightGroupOrderInfo>((oi, fgoi) => oi.Id == fgoi.OrderId)
                .Where(n => n.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery)
                .Where<FightGroupOrderInfo>(n => n.JoinStatus < 4 && n.OrderUserId == userId)
                .Select(n => n.Id)
                .Count();
        }

        public int GetOrderTotalProductCount(long order)
        {
            return DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId == order).Sum<int>(p => p.Quantity);
        }

        /// <summary>
        /// 根据订单项id获取订单项
        /// </summary>
        /// <param name="orderItemIds"></param>
        /// <returns></returns>
        public List<OrderItemInfo> GetOrderItemsByOrderItemId(IEnumerable<long> orderItemIds)
        {
            var itemlist = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.Id.ExIn(orderItemIds)).ToList();
            foreach(var orderitem in itemlist)
            {
                TypeInfo typeInfo = DbFactory.Default.Get<TypeInfo>().InnerJoin<ProductInfo>((ti, pi) => ti.Id == pi.TypeId && pi.Id == orderitem.ProductId).FirstOrDefault();
                ProductInfo prodata = DbFactory.Default.Get<ProductInfo>().Where(pi => pi.Id == orderitem.ProductId).FirstOrDefault();
                orderitem.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                orderitem.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                orderitem.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                if (prodata != null)
                {
                    orderitem.ColorAlias = !string.IsNullOrWhiteSpace(prodata.ColorAlias) ? prodata.ColorAlias : orderitem.ColorAlias;
                    orderitem.SizeAlias = !string.IsNullOrWhiteSpace(prodata.SizeAlias) ? prodata.SizeAlias : orderitem.SizeAlias;
                    orderitem.VersionAlias = !string.IsNullOrWhiteSpace(prodata.VersionAlias) ? prodata.VersionAlias : orderitem.VersionAlias;
                }
            }
            return itemlist;
        }

        /// <summary>
        /// 根据订单id获取订单项
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public List<OrderItemInfo> GetOrderItemsByOrderId(long orderId)
        {
            var orderitems = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId == orderId).ToList();
            var flag = SiteSettingApplication.SiteSettings.IsOpenHistoryOrder;
            if (flag)
            {
                orderitems.AddRange(DbFactory.MongoDB.AsQueryable<OrderItemInfo>().Where(p => p.OrderId == orderId));
            }
            return orderitems;
        }


        public List<OrderOperationLogInfo> GetOrderLogs(long order)
        {
            return DbFactory.Default.Get<OrderOperationLogInfo>(p => p.OrderId == order).ToList();
        }
        /// <summary>
        /// 根据订单id获取订单项
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public List<OrderItemInfo> GetOrderItemsByOrderId(IEnumerable<long> orderIds)
        {
            var rets = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId.ExIn(orderIds)).ToList();
            var flag = SiteSettingApplication.SiteSettings.IsOpenHistoryOrder;
            if (flag)
            {
                rets.AddRange(DbFactory.MongoDB.AsQueryable<OrderItemInfo>().Where(p => orderIds.Contains(p.OrderId)));
            }
            return rets;
        }

        /// <summary>
        /// 获取订单的评论数
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public Dictionary<long, long> GetOrderCommentCount(IEnumerable<long> orderIds)
        {
            return DbFactory.Default
                .Get<OrderCommentInfo>()
                .Where(p => p.OrderId.ExIn(orderIds))
                .GroupBy(p => p.OrderId)
                .Select(p => new { p.OrderId, total = p.ExCount(false) })
                .ToList<dynamic>()
                .ToDictionary<dynamic, long, long>(p => p.OrderId, p => p.total);
        }

        public SKUInfo GetSkuByID(string skuid)
        {
            return DbFactory.Default.Get<SKUInfo>().Where(p => p.Id == skuid).FirstOrDefault();
        }

        /// <summary>
        /// 根据订单项id获取售后记录
        /// </summary>
        /// <param name="orderItemIds"></param>
        /// <returns></returns>
        public List<OrderRefundInfo> GetOrderRefunds(IEnumerable<long> orderItemIds)
        {
            return DbFactory.Default.Get<OrderRefundInfo>().Where(p => p.OrderItemId.ExIn(orderItemIds)).ToList();
        }

        /// <summary>
        /// 获取商品已购数（包括限时购、拼团）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public Dictionary<long, long> GetProductBuyCount(long userId, IEnumerable<long> productIds)
        {
            return DbFactory.Default
                .Get<OrderItemInfo>()
                .LeftJoin<OrderInfo>((oii, oi) => oii.OrderId == oi.Id)
                .Where(p => p.ProductId.ExIn(productIds))
                .Where<OrderInfo>(p => p.UserId == userId && p.OrderStatus != OrderInfo.OrderOperateStatus.Close)
                .GroupBy(p => p.ProductId)
                .Select(p => new { p.ProductId, total = p.Quantity.ExSum() })
                .ToList<dynamic>()
                .ToDictionary<dynamic, long, long>(p => p.Key, p => p.total);
        }
        /// <summary>
        /// 获取限时购商品已购数
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public Dictionary<long, long> GetProductBuyCountLimitBuy(long userId, IEnumerable<long> productIds)
        {
            return DbFactory.Default
                .Get<OrderItemInfo>()
                .LeftJoin<OrderInfo>((oii, oi) => oii.OrderId == oi.Id)
                .Where(p => p.ProductId.ExIn(productIds) && p.IsLimitBuy == true)
                .Where<OrderInfo>(p => p.UserId == userId && p.OrderStatus != OrderInfo.OrderOperateStatus.Close)
                .GroupBy(p => p.ProductId)
                .Select(p => new { p.ProductId, total = p.Quantity.ExSum() })
                .ToList<dynamic>()
                .ToDictionary<dynamic, long, long>(p => p.ProductId, p => p.total);
        }
        /// <summary>
        /// 获取非限时购商品已购数
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public Dictionary<long, long> GetProductBuyCountNotLimitBuy(long userId, IEnumerable<long> productIds)
        {
            return DbFactory.Default.Get<OrderItemInfo>()
                .Where(p => p.ProductId.ExIn(productIds) && p.IsLimitBuy == false)
                .LeftJoin<OrderInfo>((oii, oi) => oii.OrderId == oi.Id)
                .Where<OrderInfo>(p => p.UserId == userId && p.OrderStatus != OrderInfo.OrderOperateStatus.Close)
                .GroupBy(p => p.ProductId)
                .Select(p => new { p.ProductId, total = p.Quantity.ExSum() })
                .ToList<dynamic>()
                .ToDictionary<dynamic, long, long>(p => p.ProductId, p => (long)p.total);
        }

        #endregion 获取订单相关 done

        #region 创建订单相关  done
        public List<OrderInfo> CreateOrder(OrderCreateModel model)
        {
            CheckWhenCreateOrder(model);

            //创建SKU列表对象
            var orderSkuInfos = GetOrderSkuInfo(model);

            //发票保存
            if (model.Invoice == InvoiceType.OrdinaryInvoices)
            {
                AddInvoiceTitle(model.InvoiceTitle, model.InvoiceCode, model.CurrentUser.Id);
            }

            //创建订单额外对象
            var additional = CreateAdditional(orderSkuInfos, model);

            //创建订单列表对象
            var infos = GetOrderInfos(orderSkuInfos, model, additional);

            //处理积分比例
            var siteset = SiteSettingApplication.SiteSettings;
            var IntegralExchange = ServiceProvider.Instance<IMemberIntegralService>.Create.GetIntegralChangeRule();
            if (IntegralExchange == null && model.Integral > 0)
            {
                throw new MallException("不可以使用积分");
            }
            if (model.Integral > 0)
            {
                var maxuseint = Math.Ceiling(decimal.Round((infos.Sum(d => d.OrderAmount) * siteset.IntegralDeductibleRate) / (decimal)100, 2, MidpointRounding.AwayFromZero) * IntegralExchange.IntegralPerMoney);
                if (model.Integral > maxuseint)
                {
                    throw new MallException("不可以超过积分可抵扣最高比例 " + siteset.IntegralDeductibleRate + "%");
                }
            }
            ShippingAddressInfo receiveAddress = null;
            if (model.ReceiveAddressId > 0)
            {
                receiveAddress = DbFactory.Default.Get<ShippingAddressInfo>().Where(d => d.Id == model.ReceiveAddressId).FirstOrDefault();
            }

            DbFactory.Default
                .InTransaction(() =>
                {
                    var ordersql = new Sql();
                    var orderitemsql = new Sql();
                    //int i = 0;
                    //int j = 0;
                    #region 组装Sql
                    foreach (var order in infos)
                    {
                        ordersql = new Sql();
                        //补用户收货地址经纬度
                        if (model.ReceiveAddressId > 0 && receiveAddress != null)
                        {
                            order.ReceiveLongitude = receiveAddress.Longitude;
                            order.ReceiveLatitude = receiveAddress.Latitude;
                        }

                        ordersql.Append("INSERT INTO `Mall_order`(");
                        ordersql.Append("`Id`,`OrderStatus`, `OrderDate`, `CloseReason`, `ShopId`, `ShopName`, `SellerPhone`, `SellerAddress`, `SellerRemark`, ");
                        ordersql.Append("`SellerRemarkFlag`, `UserId`, `UserName`, `UserRemark`, `ShipTo`, `CellPhone`, `TopRegionId`, `RegionId`, `RegionFullName`, `Address`, `ExpressCompanyName`, ");
                        ordersql.Append("`Freight`, `ShipOrderNumber`, `ShippingDate`, `IsPrinted`, `PaymentTypeName`, `PaymentTypeGateway`, `PaymentType`, `GatewayOrderId`, `PayRemark`, `PayDate`, ");
                        ordersql.Append("`Tax`, `FinishDate`, `ProductTotalAmount`, `RefundTotalAmount`, `CommisTotalAmount`, `RefundCommisAmount`, `ActiveType`, `Platform`, ");
                        ordersql.Append("`DiscountAmount`, `IntegralDiscount`, `CapitalAmount`,`OrderType`, `OrderRemarks`, `LastModifyTime`, `DeliveryType`, `ShopBranchId`, `PickupCode`, ");
                        ordersql.Append("`TotalAmount`, `ActualPayAmount`, `FullDiscount`, `ReceiveLatitude`, `ReceiveLongitude`, `CouponId` ");
                        ordersql.Append(") VALUES(");
                        ordersql.Append("@0, @1, @2, @3, @4,", order.Id, order.OrderStatus.GetHashCode(), order.OrderDate, order.CloseReason, order.ShopId);
                        ordersql.Append("@0, @1, @2, @3,", order.ShopName, order.SellerPhone, order.SellerAddress, order.SellerRemark);
                        ordersql.Append("@0,@1,@2,@3,@4,", order.SellerRemarkFlag, order.UserId, order.UserName, order.UserRemark, order.ShipTo);
                        ordersql.Append("@0,@1,@2,@3,@4,@5,", order.CellPhone, order.TopRegionId, order.RegionId, order.RegionFullName, order.Address, order.ExpressCompanyName);
                        ordersql.Append("@0,@1,@2,@3,@4,", order.Freight, order.ShipOrderNumber, order.ShippingDate, order.IsPrinted, order.PaymentTypeName);
                        ordersql.Append("@0,@1,@2,@3,@4, ", order.PaymentTypeGateway, order.PaymentType.GetHashCode(), order.GatewayOrderId, order.PayRemark, order.PayDate);
                        ordersql.Append("@0,@1,", order.Tax, order.FinishDate);
                        ordersql.Append("@0,@1,@2,@3,@4,@5,", order.ProductTotalAmount, order.RefundTotalAmount, order.CommisTotalAmount, order.RefundCommisAmount, order.ActiveType, order.Platform);
                        ordersql.Append("@0,@1,@2,@3,", order.DiscountAmount, order.IntegralDiscount, order.CapitalAmount, order.OrderType);
                        ordersql.Append("@0,@1,@2,@3,@4,", order.OrderRemarks, order.LastModifyTime, order.DeliveryType, order.ShopBranchId, order.PickupCode);
                        ordersql.Append("@0,@1,@2,@3,@4,@5 ); ", order.TotalAmount, order.ActualPayAmount, order.FullDiscount, order.ReceiveLatitude, order.ReceiveLongitude, order.CouponId);

                        DbFactory.Default.Execute(ordersql);

                        foreach (var orderitem in order.OrderItemInfo)
                        {
                            orderitemsql = new Sql();
                            orderitemsql.Append("INSERT INTO `Mall_orderitem` (");
                            orderitemsql.Append("`OrderId`, `ShopId`, `ProductId`, `SkuId`, `SKU`, `Quantity`, `ReturnQuantity`, `CostPrice`, `SalePrice`, `DiscountAmount`, `RealTotalPrice`, `RefundPrice`, ");
                            orderitemsql.Append("`ProductName`, `Color`, `Size`, `Version`, `ThumbnailsUrl`, `CommisRate`, `EnabledRefundAmount`, `IsLimitBuy`, `FlashSaleId`, `EnabledRefundIntegral`, `CouponDiscount`, `FullDiscount`,`EffectiveDate`");
                            orderitemsql.Append(") VALUES(");
                            orderitemsql.Append("@0,@1,@2,@3,@4,", orderitem.OrderId, orderitem.ShopId, orderitem.ProductId, orderitem.SkuId, orderitem.SKU);
                            orderitemsql.Append("@0,@1,@2,@3,@4,", orderitem.Quantity, orderitem.ReturnQuantity, orderitem.CostPrice, orderitem.SalePrice, orderitem.DiscountAmount);
                            orderitemsql.Append("@0,@1,@2,@3,@4, ", orderitem.RealTotalPrice, orderitem.RefundPrice, orderitem.ProductName, orderitem.Color, orderitem.Size);
                            orderitemsql.Append("@0,@1,@2,@3,@4,@5,", orderitem.Version, orderitem.ThumbnailsUrl, orderitem.CommisRate, orderitem.EnabledRefundAmount, orderitem.IsLimitBuy,orderitem.FlashSaleId);
                            orderitemsql.Append("@0,@1,@2,@3 ); ", orderitem.EnabledRefundIntegral, orderitem.CouponDiscount, orderitem.FullDiscount, orderitem.EffectiveDate);

                            DbFactory.Default.Execute(orderitemsql);
                        }
                    }
                    #endregion

                    if (model.CartItemIds != null && model.CartItemIds.Any() && !model.IsVirtual)
                    {
                        var cartItemIds = model.CartItemIds.ToArray();
                        DbFactory.Default.Del<ShoppingCartInfo>().Where(item => item.Id.ExIn(cartItemIds) && item.UserId == model.CurrentUser.Id).Succeed();

                        Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(model.CurrentUser.Id));
                        Cache.Remove(CacheKeyCollection.CACHE_CART(model.CurrentUser.Id));
                    }

                    //优惠券状态改变
                    if (model.CouponIdsStr != null && model.CouponIdsStr.Count() > 0)
                    {
                        UseCoupon(infos.ToList(), additional.BaseCoupons.ToList(), model.CurrentUser.Id);
                    }

                    foreach (var info in infos)
                    {
                        //预存款一次性支付完成后写入支付记录
                        if (null != info.OrderPay)
                        {
                            DbFactory.Default.Add(info.OrderPay);
                            GenerateBonus(info);
                        }

                        if (info.OrderType == OrderInfo.OrderTypes.FightGroup)
                        {
                            //处理拼团
                            var ifgser = ServiceProvider.Instance<IFightGroupService>.Create;
                            ifgser.AddOrder(model.ActiveId, info.Id, model.CurrentUser.Id, model.GroupId);
                        }

                        //减少库存
                        foreach (var orderItem in info.OrderItemInfo)
                        {
                            if (info.DeliveryType == CommonModel.DeliveryType.SelfTake ||
                                info.DeliveryType == CommonModel.DeliveryType.ShopStore || (info.OrderType == OrderInfo.OrderTypes.Virtual && model.IsShopbranchOrder))
                                UpdateShopBranchSku(info.ShopBranchId, orderItem.SkuId, -(int)orderItem.Quantity);
                            else
                            {
                                DbFactory.Default.Set<SKUInfo>().Set(n => n.Stock, n => n.Stock - orderItem.Quantity).Where(n => n.Id == orderItem.SkuId).Succeed();
                            }

                            //限时购扣减活动库存
                            if (model.IslimitBuy)
                            {
                                DbFactory.Default.Set<FlashSaleDetailInfo>().Set(n => n.TotalCount, n => n.TotalCount - orderItem.Quantity).Where(n => n.SkuId == orderItem.SkuId).Succeed();
                            }
                        }
                        //处理分销
                        if (SiteSettingApplication.SiteSettings.DistributionIsEnable)
                        {
                            ServiceProvider.Instance<IDistributionService>.Create.TreatedOrderDistribution(info.Id, SiteSettingApplication.SiteSettings.DistributionCanSelfBuy, SiteSettingApplication.SiteSettings.DistributionMaxLevel);
                        }
                    }

                    #region 系统处理订单自动分配门店
                    if (SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore)//如果开启门店授权
                    {
                        var shipAddressInfo = additional.Address;//获取当前订单收货地址对象
                        ShopBranchQuery query = null;
                        foreach (var p in infos)
                        {
                            if (p.OrderType == OrderInfo.OrderTypes.Virtual) continue;//虚拟订单不支持自动分配门店
                            if (p.ShopBranchId == 0)//如果订单之前不属于任何门店，则系统尝试匹配
                            {
                                var shopInfo = _iShopService.GetShop(p.ShopId);
                                bool autoAllotOrder = shopInfo != null && shopInfo.AutoAllotOrder;//每个订单所属商家的后台是否开启订单自动分配
                                if (!autoAllotOrder) continue;

                                if (shipAddressInfo != null)
                                {
                                    query = new ShopBranchQuery()
                                    {
                                        ShopId = p.ShopId,
                                        FromLatLng = string.Format("{0},{1}", shipAddressInfo.Latitude, shipAddressInfo.Longitude),
                                        StreetId = p.RegionId//街道或区县(有的只有3级)
                                    };
                                    var parentAreaInfo = _iRegionService.GetRegion(p.RegionId, Region.RegionLevel.Town);//判断当前区域是否为第四级
                                    if (parentAreaInfo != null && parentAreaInfo.ParentId > 0)
                                        query.DistrictId = parentAreaInfo.ParentId;
                                    else
                                    {
                                        query.DistrictId = query.StreetId;
                                        query.StreetId = 0;
                                    }
                                    var orderitems = DbFactory.Default.Get<OrderItemInfo>(o => o.OrderId == p.Id).ToList();
                                    var obj = _iShopBranchService.GetAutoMatchShopBranch(query, orderitems.Select(x => x.SkuId).ToArray(), orderitems.Select(y => (int)y.Quantity).ToArray());
                                    if (obj != null && obj.Id > 0)
                                    {
                                        foreach (var orderItem in orderitems)
                                        {
                                            UpdateShopBranchSku(obj.Id, orderItem.SkuId, -Core.Helper.TypeHelper.ObjectToInt(orderItem.Quantity));
                                            UpdateSKUStockInOrder(orderItem.SkuId, orderItem.Quantity);
                                        }
                                        UpdateOrderShopBranch(p.Id, obj.Id);
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    var orderIds = infos.Select(p => p.Id);
                    //限时购销量
                    if (model.IsCashOnDelivery && model.IslimitBuy)
                    {
                        ServiceProvider.Instance<ILimitTimeBuyService>.Create.IncreaseSaleCount(orderIds.ToList());
                    }

                    //发送微信消息  提交订单
                    if (!string.IsNullOrEmpty(model.formId))
                    {
                        var orderId = string.Join(",", orderIds);
                        WXAppletFormDataInfo info = new WXAppletFormDataInfo();
                        info.EventId = Convert.ToInt64(MessageTypeEnum.OrderCreated);
                        info.EventTime = DateTime.Now;
                        info.EventValue = orderId;
                        info.ExpireTime = DateTime.Now.AddDays(7);
                        info.FormId = model.formId;
                        Instance<IWXMsgTemplateService>.Create.AddWXAppletFromData(info);
                    }
                    //减少积分
                    DeductionIntegral(model.CurrentUser, orderIds, model.Integral);
                    //减少预存款
                    DeductionCapital(model.CurrentUser, infos, model.Capital);

                    #region 处理订单发票记录
                    foreach (var info in infos)
                    {
                        if (info.OrderInvoice != null)
                        {
                            DbFactory.Default.Add<OrderInvoiceInfo>(info.OrderInvoice);
                        }
                    }
                    #endregion
                });

            //发送短信通知
            Task.Factory.StartNew(() =>
            {
                SendMessage(infos);
                var orderids = infos.Where(d => (d.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp) || d.TotalAmount - d.CapitalAmount <= 0).Select(d => d.Id).ToList();
                if (orderids.Count > 0)
                {
                    PaySucceed(orderids, PAY_BY_CAPITAL_PAYMENT_ID, DateTime.Now);
                }
            });
          
          
            return infos;
        }

        /// <summary>
        /// 支付完生成红包
        /// </summary>
        private void GenerateBonus(OrderInfo o)
        {
            var port = Core.Helper.WebHelper.GetPort();
            string url = Core.Helper.WebHelper.GetScheme() + "://" + Core.Helper.WebHelper.GetHost() + (string.IsNullOrEmpty(port) ? "" : ":" + port) + "/m-weixin/shopbonus/index/";
            var shopBonus = _iShopBonusService.GetByShopId(o.ShopId);
            if (shopBonus == null)
            {
                return;
            }
            if (shopBonus.GrantPrice <= o.OrderTotalAmount)
            {
                _iShopBonusService.GenerateBonusDetail(shopBonus, o.Id, url);
            }
        }

        /// <summary>
        /// 设置订单的优惠券金额分摊到每个子订单
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="Coupon"></param>
        /// <returns></returns>
        private void SetActualItemPrice(OrderInfo info)
        {
            var t = info.OrderItemInfo;
            if (t == null || t.Count < 1)
            {
                t = DbFactory.Default.Get<OrderItemInfo>().Where(a => a.OrderId == info.Id).ToList();
            }
            decimal couponDiscount = 0;
            var num = t.Count();
            List<long> couponProducts = new List<long>();
            decimal coupontotal = 0;
            bool singleCal = false;
            if (info.CouponId > 0)
            {
                var coupon = DbFactory.Default.Get<CouponInfo>().Where(p => p.Id == info.CouponId).FirstOrDefault();
                if (coupon != null && coupon.UseArea == 1)
                {
                    singleCal = true;
                    couponProducts = DbFactory.Default
                        .Get<CouponProductInfo>()
                        .Where(p => p.CouponId == coupon.Id)
                        .Select(p => p.ProductId)
                        .ToList<long>();
                    foreach (var p in t)
                    {
                        if (couponProducts.Contains(p.ProductId))
                        {
                            coupontotal += p.RealTotalPrice - p.FullDiscount;
                        }
                    }
                }
            }
            for (var i = 0; i < t.Count(); i++)
            {
                var _item = t[i];
                if (i < num - 1)
                {
                    if (singleCal)
                    {
                        if (couponProducts.Contains(_item.ProductId))
                            _item.CouponDiscount = Math.Round((_item.RealTotalPrice - _item.FullDiscount) / coupontotal * info.DiscountAmount, 2);
                    }
                    else
                    {
                        _item.CouponDiscount = GetItemCouponDisCount(_item.RealTotalPrice - _item.FullDiscount, info.ProductTotalAmount - info.FullDiscount, info.DiscountAmount);
                    }
                    couponDiscount += _item.CouponDiscount;
                }
                else
                {
                    if ((singleCal && couponProducts.Contains(_item.ProductId)) || !singleCal)
                        _item.CouponDiscount = info.DiscountAmount - couponDiscount;
                }
            }
        }

        public long SaveOrderPayInfo(IEnumerable<OrderPayInfo> model, Core.PlatformType platform)
        {
            //只有一个订单就取第一个订单号，否则生成一个支付订单号
            //var orderid = long.Parse(model.FirstOrDefault().OrderId.ToString() + ((int)platform).ToString());
            var payid = GetOrderPayId();
            DbFactory.Default
                .InTransaction(() =>
                {
                    foreach (var pay in model)
                    {
                        var orderPayInfo = DbFactory.Default.Get<OrderPayInfo>().Where(item => item.PayId == payid && item.OrderId == pay.OrderId).FirstOrDefault();
                        if (orderPayInfo == null)
                        {
                            orderPayInfo = new OrderPayInfo
                            {
                                OrderId = pay.OrderId,
                                PayId = payid
                            };
                            DbFactory.Default.Add(orderPayInfo);
                        }
                    }
                });
            return payid;
        }

        /// <summary>
        /// 根据订单id获取OrderPayInfo
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public List<OrderPayInfo> GetOrderPays(IEnumerable<long> orderIds)
        {
            return DbFactory.Default.Get<OrderPayInfo>().Where(p => p.OrderId.ExIn(orderIds)).ToList();
        }

        #endregion 创建订单相关  done

        #region 订单操作 done

        // 商家发货
        public OrderInfo SellerSendGood(long orderId, string sellerName, string companyName, string shipOrderNumber)
        {
            OrderInfo order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == orderId).FirstOrDefault();
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                throw new MallException("只有待发货状态的订单才能发货");
            }
            if (!CanSendGood(orderId))
            {
                throw new MallException("拼团完成后订单才可以发货");
            }
            order.OrderStatus = OrderInfo.OrderOperateStatus.WaitReceiving;
            order.ExpressCompanyName = companyName;
            order.ShipOrderNumber = shipOrderNumber;
            order.ShippingDate = DateTime.Now;
            order.LastModifyTime = DateTime.Now;

            //处理订单退款
            var refund = DbFactory.Default
                .Get<OrderRefundInfo>()
                .Where(d => d.OrderId == orderId && d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund
                    && d.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit)
                .FirstOrDefault();
            if (refund != null)
            {
                //自动拒绝退款申请
                ServiceProvider.Instance<IRefundService>.Create.SellerDealRefund(refund.Id, OrderRefundInfo.OrderRefundAuditStatus.UnAudit, "商家已发货", sellerName);
            }
            DbFactory.Default.Update(order);

            AddOrderOperationLog(orderId, sellerName, "商家发货");

            return order;
        }

        /// <summary>
        /// 门店发货
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="deliveryType">配送方式（2店员配送或1快递配送）</param>
        /// <param name="shopkeeperName">发货人（门店管理员账号名称）</param>
        /// <param name="companyName">快递公司</param>
        /// <param name="shipOrderNumber">快递单号</param>
        /// <returns></returns>
        public OrderInfo ShopSendGood(long orderId, int deliveryType, string shopkeeperName, string companyName, string shipOrderNumber)
        {
            OrderInfo order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == orderId).FirstOrDefault();
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                throw new MallException("只有待发货状态的订单才能发货");
            }
            if (!CanSendGood(orderId))
            {
                throw new MallException("拼团完成后订单才可以发货");
            }
            order.OrderStatus = OrderInfo.OrderOperateStatus.WaitReceiving;
            if (deliveryType == 2)
            {
                order.DeliveryType = CommonModel.DeliveryType.ShopStore;
            }
            else if (deliveryType == CommonModel.DeliveryType.CityExpress.GetHashCode())
            {
                order.DeliveryType = CommonModel.DeliveryType.CityExpress;
                order.DadaStatus = DadaStatus.WaitOrder.GetHashCode();
            }
            else
            {
                order.DeliveryType = CommonModel.DeliveryType.Express;
            }
            order.ExpressCompanyName = companyName;
            order.ShipOrderNumber = shipOrderNumber;
            order.ShippingDate = DateTime.Now;
            order.LastModifyTime = DateTime.Now;

            //处理订单退款
            var refund = DbFactory.Default
                .Get<OrderRefundInfo>()
                .Where(d => d.OrderId == orderId && d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund &&
                    d.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit)
                .FirstOrDefault();

            if (refund != null)
            {
                //自动拒绝退款申请
                ServiceProvider.Instance<IRefundService>.Create.SellerDealRefund(refund.Id, OrderRefundInfo.OrderRefundAuditStatus.UnAudit, "门店已发货", shopkeeperName);
            }

            DbFactory.Default.Update(order);

            AddOrderOperationLog(orderId, shopkeeperName, "门店发货");

            return order;
        }

        /// <summary>
        /// 判断订单是否在申请售后
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool IsOrderAfterService(long orderId)
        {
            var refund = DbFactory.Default
                .Get<OrderRefundInfo>()
                .Where(d => d.OrderId == orderId && d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund &&
                    d.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit)
                .Exist();
            return refund;
        }

        /// <summary>
        /// 修改快递信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="companyName"></param>
        /// <param name="shipOrderNumber"></param>
        /// <returns></returns>
        public OrderInfo UpdateExpress(long orderId, string companyName, string shipOrderNumber)
        {
            var order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == orderId).FirstOrDefault();

            order.ExpressCompanyName = companyName;
            order.ShipOrderNumber = shipOrderNumber;
            order.ShippingDate = DateTime.Now;
            order.LastModifyTime = DateTime.Now;

            DbFactory.Default.Update(order);

            return order;
        }

        // 商家更新收货地址
        public void SellerUpdateAddress(long orderId, string sellerName, string shipTo, string cellPhone, int topRegionId, int regionId, string regionFullName, string address)
        {
            OrderInfo order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == orderId).FirstOrDefault();
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                throw new MallException("只有待付款或待发货状态的订单才能修改收货地址");
            }

            order.ShipTo = shipTo;
            order.CellPhone = cellPhone;
            order.TopRegionId = topRegionId;
            order.RegionId = regionId;
            order.RegionFullName = regionFullName;
            order.Address = address;
            DbFactory.Default.Update(order);
            AddOrderOperationLog(orderId, sellerName, "商家修改订单的收货地址");
        }

        // 会员确认订单
        public void MembeConfirmOrder(long orderId, string memberName)
        {
            OrderInfo order = DbFactory.Default.Get<OrderInfo>().Where(a => a.Id == orderId && a.UserName == memberName).FirstOrDefault();

            if (order.OrderStatus == OrderInfo.OrderOperateStatus.Finish)
            {
                throw new MallException("该订单已经确认过!");
            }
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitReceiving && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
            {
                throw new MallException("订单状态发生改变，请重新刷页面操作!");
            }
            DbFactory.Default
                .InTransaction(() =>
                {
                    this.SetStateToConfirm(order);
                    order.LastModifyTime = DateTime.Now;
                    order.DadaStatus = DadaStatus.Finished.GetHashCode();
                    if (order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery)
                    {//货到付款的订单，在会员确认收货时，计算实付金额
                        order.ActualPayAmount = order.OrderTotalAmount;
                    }
                    DbFactory.Default.Update(order);

                    //会员确认收货后，不会马上给积分，得需要过了售后维权期才给积分,（虚拟商品除外）
                    if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                    {
                        var member = DbFactory.Default.Get<MemberInfo>().Where(a => a.UserName == memberName).FirstOrDefault();
                        AddIntegral(member, order.Id, order.TotalAmount - order.RefundTotalAmount);//增加积分
                    }
                    AddOrderOperationLog(orderId, memberName, "会员确认收货");
                    UpdateProductVistiOrderCount(orderId);

                    if (order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery)
                    {
                        var orderItems = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId == order.Id).ToList();
                        UpdateProductVisti(orderItems);
                        ServiceProvider.Instance<IDistributionService>.Create.TreatedOrderDistributionBrokerage(order.Id, true);
                        //货到付款订单，在确认时，写入待结算
                        WritePendingSettlnment(order);
                    }
                    else
                    {
                        //更新待结算订单完成时间
                        UpdatePendingSettlnmentFinishDate(orderId, DateTime.Now);
                    }
                });
        }
        /// <summary>
        /// 门店核销订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="managerName"></param>
        public void ShopBranchConfirmOrder(long orderId, long shopBranchId, string managerName)
        {
            OrderInfo order = DbFactory.Default.Get<OrderInfo>().Where(a => a.Id == orderId && a.ShopBranchId == shopBranchId).FirstOrDefault();
            if (order == null)
            {
                throw new MallException("处理订单错误，请确认参数正确");
            }
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
            {
                throw new MallException("只有待自提状态的订单才能进行核销操作");
            }
            var result = DbFactory.Default
                .InTransaction(() =>
                {
                    if (order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery)
                    {
                        order.PayDate = DateTime.Now;
                    }
                    var currDate = DateTime.Now;
                    order.OrderStatus = OrderInfo.OrderOperateStatus.Finish;
                    order.FinishDate = currDate;
                    order.LastModifyTime = currDate;
                    DbFactory.Default.Update(order);

                    //会员确认收货后，不会马上给积分，得需要过了售后维权期才给积分(虚拟商品除外)
                    if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                    {
                        var member = DbFactory.Default.Get<MemberInfo>().Where(a => a.Id == order.UserId).FirstOrDefault();
                        AddIntegral(member, order.Id, order.TotalAmount - order.RefundTotalAmount);//增加积分
                    }
                    AddOrderOperationLog(orderId, managerName, "门店核销订单");
                    UpdateProductVistiOrderCount(orderId);

                    if (order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery)
                    {
                        var orderItems = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId == order.Id).ToList();
                        UpdateProductVisti(orderItems);
                        ServiceProvider.Instance<IDistributionService>.Create.TreatedOrderDistributionBrokerage(order.Id, true);
                        //货到付款的订单，在确认时，写入待结算
                        WritePendingSettlnment(order);
                    }
                    else
                    {
                        //更新待结算订单完成时间
                        UpdatePendingSettlnmentFinishDate(orderId, currDate);
                    }
                    //处理订单退款
                    var refund = DbFactory.Default
                        .Get<OrderRefundInfo>()
                        .Where(d => d.OrderId == orderId && d.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund &&
                            d.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit)
                        .ToList();
                    if (refund != null && refund.Count > 0)
                    {
                        foreach (var item in refund)
                        {
                            //自动拒绝退款申请
                            ServiceProvider.Instance<IRefundService>.Create.SellerDealRefund(item.Id, OrderRefundInfo.OrderRefundAuditStatus.UnAudit, "门店已发货", managerName);
                        }
                    }

                });

            //发送消息
            if (result)
            {
                var orderItem = DbFactory.Default.Get<OrderItemInfo>(p => p.OrderId == order.Id).FirstOrDefault();
                //新增微信短信邮件消息推送
                var orderMessage = new MessageOrderInfo();
                orderMessage.UserName = order.UserName;
                orderMessage.OrderId = order.Id.ToString();
                orderMessage.ShopId = order.ShopId;
                orderMessage.ShopName = order.ShopName;
                if (order.ShopBranchId > 0)
                {
                    var shopbranch = DbFactory.Default.Get<ShopBranchInfo>(s => s.Id == order.ShopBranchId).FirstOrDefault();
                    if (shopbranch != null)
                        orderMessage.ShopName = shopbranch.ShopBranchName;
                }
                orderMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;
                orderMessage.TotalMoney = order.OrderTotalAmount;
                orderMessage.ProductName = orderItem.ProductName;
                orderMessage.RefundAuditTime = DateTime.Now;
                orderMessage.PickupCode = order.PickupCode;
                orderMessage.FinishDate = order.FinishDate.Value;
                if (order.Platform == PlatformType.WeiXinSmallProg)
                {
                    orderMessage.MsgOrderType = MessageOrderType.Applet;
                }
                Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnAlreadyVerification(order.UserId, orderMessage));

            }
        }
        // 平台关闭订单
        public void PlatformCloseOrder(long orderId, string managerName, string closeReason = "")
        {
            OrderInfo order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == orderId).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(closeReason))
            {
                closeReason = "平台取消订单";
            }
            DbFactory.Default
                .InTransaction(() =>
                {
                    order.CloseReason = closeReason;
                    order.LastModifyTime = DateTime.Now;
                    this.CloseOrder(order);

                    ReturnStock(order);
                    DbFactory.Default.Update(order);

                    //拼团处理
                    if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
                    {
                        SetFightGroupOrderStatus(orderId, FightGroupOrderJoinStatus.BuildFailed);
                    }

                    var member = DbFactory.Default.Get<MemberInfo>().Where(a => a.Id == order.UserId).FirstOrDefault();
                    CancelIntegral(member, order.Id, order.IntegralDiscount);  //取消订单增加积分
                    if (order.CapitalAmount > 0)
                        CancelCapital(member, order, order.CapitalAmount);//退回预存款

                    if (order.CouponId > 0)
                        ReturnCoupon(member, order);//退回优惠卷

                    if (order.CouponId <= 0 && order.DiscountAmount > 0)
                        ReturnShopBonus(order);//退回代金红包
                    AddOrderOperationLog(orderId, managerName, closeReason);
                    //分销处理
                    ServiceProvider.Instance<IDistributionService>.Create.RemoveBrokerageByOrder(order.Id);
                });
        }

        // 商家关闭订单
        public void SellerCloseOrder(long orderId, string sellerName)
        {
            OrderInfo order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == orderId).FirstOrDefault();
            if (order == null)
            {
                throw new MallException("错误的订单编号！");
            }
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                throw new MallException("拼团订单，不可以手动取消！");
            }
            if (order.OrderStatus == OrderInfo.OrderOperateStatus.Close)
            {
                throw new MallException("您的订单已被取消了，不需再重复操作！");
            }
            if (order.PaymentType != OrderInfo.PaymentTypes.CashOnDelivery
                && (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp))
            {
                throw new MallException("订单已付款，不能取消订单！");
            }
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay && order.PaymentType != OrderInfo.PaymentTypes.CashOnDelivery ||
                order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                throw new MallException("您的订单状态已发生变化，不能进行取消操作！");
            }
            DbFactory.Default
                .InTransaction(() =>
                {
                    order.CloseReason = "商家取消订单";
                    order.OrderStatus = OrderInfo.OrderOperateStatus.Close;
                    order.LastModifyTime = DateTime.Now;
                    ReturnStock(order);
                    DbFactory.Default.Update(order);

                    //拼团处理
                    if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
                    {
                        SetFightGroupOrderStatus(orderId, FightGroupOrderJoinStatus.BuildFailed);
                    }
                    //分销处理
                    ServiceProvider.Instance<IDistributionService>.Create.RemoveBrokerageByOrder(order.Id);
                    var member = DbFactory.Default.Get<MemberInfo>().Where(a => a.Id == order.UserId).FirstOrDefault();
                    CancelIntegral(member, order.Id, order.IntegralDiscount);  //取消订单增加积分
                    if (order.CapitalAmount > 0)
                        CancelCapital(member, order, order.CapitalAmount);//退回预存款

                    if (order.CouponId > 0)
                        ReturnCoupon(member, order);//退回优惠卷

                    if (order.CouponId <= 0 && order.DiscountAmount > 0)
                        ReturnShopBonus(order);//退回代金红包
                    AddOrderOperationLog(orderId, sellerName, order.CloseReason);
                });
        }

        // 会员关闭订单
        public void MemberCloseOrder(long orderId, string memberName)
        {
            OrderInfo order = DbFactory.Default.Get<OrderInfo>().Where(a => a.Id == orderId && a.UserName == memberName).FirstOrDefault();

            if (order == null)
            {
                throw new MallException("该订单不属于该用户！");
            }

            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                throw new MallException("拼团订单，不可以手动取消！");
            }
            if (order.OrderStatus == OrderInfo.OrderOperateStatus.Close)
            {
                throw new MallException("您的订单已被取消了，不需再重复操作！");
            }
            if (order.PaymentType != OrderInfo.PaymentTypes.CashOnDelivery
                && (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp))
            {
                throw new MallException("订单已付款，不能取消订单！");
            }
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay && order.PaymentType != OrderInfo.PaymentTypes.CashOnDelivery ||
                order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                throw new MallException("您的订单状态已发生变化，不能进行取消操作！");
            }
            DbFactory.Default
                .InTransaction(() =>
                {
                    string closeReason = "会员取消订单";
                    this.CloseOrder(order);
                    ReturnStock(order);
                    order.CloseReason = closeReason;
                    order.LastModifyTime = DateTime.Now;
                    DbFactory.Default.Update(order);
                    var member = DbFactory.Default.Get<MemberInfo>().Where(a => a.Id == order.UserId).FirstOrDefault();
                    CancelIntegral(member, order.Id, order.IntegralDiscount);  //取消订单增加积分
                    if (order.CapitalAmount > 0)
                        CancelCapital(member, order, order.CapitalAmount);//退回预存款
                    if (order.CouponId > 0)
                        ReturnCoupon(member, order);//退回优惠卷

                    if (order.CouponId <= 0 && order.DiscountAmount > 0)
                        ReturnShopBonus(order);//退回代金红包

                    AddOrderOperationLog(orderId, memberName, closeReason);
                    //分销处理
                    ServiceProvider.Instance<IDistributionService>.Create.RemoveBrokerageByOrder(order.Id);

                });
        }

        /// <summary>
        /// 用户申请关闭订单（需审核才能真正关闭） 去掉申请取消订单状态
        /// </summary>

        public void MemberApplyCloseOrder(long orderId, string memberName, bool isBackStock = false)
        {
            OrderInfo order = DbFactory.Default.Get<OrderInfo>().Where(a => a.Id == orderId && a.UserName == memberName).FirstOrDefault();
            if (order == null)
            {
                throw new MallException("该订单不属于该用户！");
            }
            DbFactory.Default
                .InTransaction(() =>
                {
                    string closeReason = "会员取消订单";
                    // order.OrderStatus = OrderInfo.OrderOperateStatus.CloseByUser;

                    if (isBackStock)
                    {
                        ReturnStock(order);
                    }
                    DbFactory.Default.Update(order);
                    var member = DbFactory.Default.Get<MemberInfo>().Where(a => a.Id == order.UserId).FirstOrDefault();
                    CancelIntegral(member, order.Id, order.IntegralDiscount);  //取消订单增加积分
                    if (order.CapitalAmount > 0)
                        CancelCapital(member, order, order.CapitalAmount);//退回预存款
                    AddOrderOperationLog(orderId, memberName, closeReason);
                });
        }

        /// <summary>
        /// 是否超过售后期
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns>true 不可售后 false 可以售后</returns>
        public bool IsRefundTimeOut(long orderId)
        {
            var order = DbFactory.Default.Get<OrderInfo>().Where(a => a.Id == orderId).FirstOrDefault();
            return IsRefundTimeOut(order);
        }

        public bool IsRefundTimeOut(OrderInfo order)
        {
            var result = true;
            if (order != null)
            {
                result = false;   //默认可以售后
                switch (order.OrderStatus)
                {
                    case OrderInfo.OrderOperateStatus.Close:
                        result = true;
                        break;

                        //case OrderInfo.OrderOperateStatus.CloseByUser:
                        //    result = true;
                        //    break;
                }
                if (order.OrderStatus == OrderInfo.OrderOperateStatus.Finish)
                {
                    result = false;
                    if (order.FinishDate.HasValue)
                    {
                        DateTime EndSalesReturn = order.FinishDate.Value.AddDays(SiteSettingApplication.SiteSettings.SalesReturnTimeout);
                        if (EndSalesReturn <= DateTime.Now)
                        {
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        // 设置订单快递信息
        public void SetOrderExpressInfo(long shopId, string expressName, string startCode, IEnumerable<long> orderIds)
        {
            var express = DbFactory.Default.Get<ExpressInfoInfo>().Where(e => e.Name.Contains(expressName)).FirstOrDefault();
            if (express == null)
            {
                throw new MallException("快递公司不存在");
            }
            if (!express.CheckExpressCodeIsValid(startCode))
            {
                throw new MallException("起始快递单号格式不正确");
            }

            var orders = DbFactory.Default.Get<OrderInfo>().Where(item => item.ShopId == shopId && item.Id.ExIn(orderIds)).ToList();
            var orderedOrders = orderIds.Select(item => orders.FirstOrDefault(t => item == t.Id)).Where(item => item != null);

            int i = 0;
            string code = string.Empty;
            var shopShipper = DbFactory.Default.Get<ShopShipperInfo>().Where(e => e.ShopId == shopId && e.IsDefaultSendGoods == true).FirstOrDefault();
            if (shopShipper == null)
            {
                throw new MallException("未设置默认发货地址");
            }
            string sendFullAddress = ServiceProvider.Instance<IRegionService>.Create.GetFullName(shopShipper.RegionId) + " " + shopShipper.Address;

            DbFactory.Default
                .InTransaction(() =>
                {
                    foreach (var order in orderedOrders)
                    {
                        if (i++ == 0)
                        {
                            code = startCode;
                        }
                        else
                        {
                            code = express.NextExpressCode(expressName, code);
                        }
                        order.ShipOrderNumber = code;
                        order.ExpressCompanyName = express.Name;
                        order.SellerPhone = shopShipper.TelPhone;
                        order.SellerAddress = sendFullAddress;
                        DbFactory.Default.Update(order);
                    }
                });
        }
        /// <summary>
        /// 设置订单商家备注
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="mark"></param>
        public void SetOrderSellerRemark(long orderId, string mark)
        {
            var orderdata = DbFactory.Default.Get<OrderInfo>().Where(d => d.Id == orderId).FirstOrDefault();
            if (orderdata == null)
            {
                throw new MallException("错误的订单编号");
            }
            orderdata.SellerRemark = mark;
            DbFactory.Default.Update(orderdata);
        }

        //商家更新金额
        public void SellerUpdateItemDiscountAmount(long orderItemId, decimal discountAmount, string sellerName)
        {
            OrderItemInfo item = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.Id == orderItemId).FirstOrDefault();
            var order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == item.OrderId).FirstOrDefault();
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                throw new MallException("拼团订单不可以改价");
            }

            item.DiscountAmount += discountAmount;
            item.RealTotalPrice = this.GetRealTotalPrice(order, item, discountAmount);
            if ((order.OrderTotalAmount - order.CapitalAmount - order.Freight - discountAmount) <= 0)
            {
                throw new MallException("优惠金额异常，改价不可以使订单为零元或负数订单！");
            }
            DbFactory.Default
                .InTransaction(() =>
                {
                    DbFactory.Default.Update(item);
                    item = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.Id == orderItemId).FirstOrDefault();
                    order.ProductTotalAmount = order.ProductTotalAmount - discountAmount;
                    order.TotalAmount = order.OrderTotalAmount;
                    order.CommisTotalAmount = DbFactory.Default.Get<OrderItemInfo>().Where(i => i.OrderId == item.OrderId).Sum(i => i.RealTotalPrice * i.CommisRate);

                    SetActualItemPrice(order);         //平摊订单的优惠券金额
                    order.LastModifyTime = DateTime.Now;
                    DbFactory.Default.Update(order);

                    AddOrderOperationLog(item.OrderId, sellerName, "商家修改订单商品的优惠金额");
                });
        }

        public void SellerUpdateOrderFreight(long orderId, decimal freight)
        {
            OrderInfo order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == orderId).FirstOrDefault();
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                throw new MallException("拼团订单不可以改价");
            }
            this.SetFreight(order, freight);
            order.LastModifyTime = DateTime.Now;
            DbFactory.Default.Update(order);
        }

        // 平台确认订单支付
        public void PlatformConfirmOrderPay(long orderId, string payRemark, string managerName)
        {
            OrderInfo order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == orderId).FirstOrDefault();
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay)
            {
                throw new MallException("只有待付款状态的订单才能进行付款操作");
            }
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                if (!FightGroupOrderCanPay(orderId))
                {
                    throw new MallException("拼团订单的状态为不可付款状态");
                }
            }
            PaySucceed(new List<long> { orderId }, PAY_BY_OFFLINE_PAYMENT_ID, DateTime.Now, paymentType: OrderInfo.PaymentTypes.Offline, payRemark: payRemark);

            AddOrderOperationLog(orderId, managerName, "平台确认收到订单货款");
        }

        // 更新用户订单数
        public void UpdateMemberOrderInfo(long userId, decimal addOrderAmount = 0, int addOrderCount = 1)
        {
            var member = DbFactory.Default.Get<MemberInfo>().Where(item => item.Id == userId).FirstOrDefault();
            member.OrderNumber += addOrderCount;//变更订单数
            member.Expenditure += addOrderAmount;//变更总金额
            DbFactory.Default.Update(member);
        }

        // 订单支付成功
        public void PaySucceed(IEnumerable<long> orderIds, string paymentId, DateTime payTime, string payNo = null
            , long payId = 0, OrderInfo.PaymentTypes paymentType = OrderInfo.PaymentTypes.Online
            , string payRemark = "")
        {

            var orders = DbFactory.Default.Get<OrderInfo>().Where(item => item.Id.ExIn(orderIds)).ToList();
            string PaymentTypeName = paymentId;
            bool isOnlinePay = paymentType == OrderInfo.PaymentTypes.Online;
            bool isPlugPay = isOnlinePay
                && paymentId != PAY_BY_CAPITAL_PAYMENT_ID
                && paymentId != PAY_BY_INTEGRAL_PAYMENT_ID
                && paymentId != PAY_BY_OFFLINE_PAYMENT_ID;
            if (isPlugPay)
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(paymentId);
                PaymentTypeName = payment.PluginInfo.DisplayName;
            }
            var orderItems = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId.ExIn(orderIds)).ToList();
            var isSendMsg = false;
            foreach (var order in orders)
            {
                //判断货到付款订单是否预存款全额抵扣
                var isCash = order.CapitalAmount >= order.TotalAmount && order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery;
                if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay || isCash)
                {
                    var orderPayInfo = DbFactory.Default.Get<OrderPayInfo>().Where(item => item.OrderId == order.Id && item.PayId == payId).FirstOrDefault();
                    DbFactory.Default
                         .InTransaction(() =>
                         {
                             order.PayDate = payTime;
                             if (order.OrderTotalAmount == 0 && order.CapitalAmount == 0)
                             {
                                 order.PaymentTypeName = PAY_BY_INTEGRAL_PAYMENT_ID;
                             }
                             else
                             {
                                 order.PaymentTypeName = PaymentTypeName;
                             }
                             order.PaymentType = paymentType;
                             if (isPlugPay)
                             {
                                 order.PaymentTypeGateway = paymentId;
                             }

                             if (order.DeliveryType == CommonModel.DeliveryType.SelfTake)
                             {
                                 OperaOrderPickupCode(order);
                             }
                             else
                             {
                                 order.OrderStatus = OrderInfo.OrderOperateStatus.WaitDelivery;
                             }

                             if (orderPayInfo != null)
                             {
                                 orderPayInfo.PayState = true;
                                 orderPayInfo.PayTime = payTime;
                             }

                             //设置实收金额=实付金额
                             order.ActualPayAmount = order.TotalAmount;
                             order.GatewayOrderId = payNo;
                             order.PayRemark = payRemark;
                             order.LastModifyTime = DateTime.Now;

                             //  SetActualItemPrice(order);         //平摊订单的优惠券金额
                             UpdateShopVisti(order);               // 修改店铺销量
                             UpdateProductVisti(orderItems.Where(p => p.OrderId == order.Id));           // 修改商品销量
                             UpdateLimitTimeBuyLog(orderItems.Where(p => p.OrderId == order.Id));   // 修改限时购销售数量
                             if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                             {
                                 order.OrderStatus = OrderInfo.OrderOperateStatus.WaitVerification;//虚拟订单付款后，则为待消费
                             }
                             DbFactory.Default.Update(order);
                             if (orderPayInfo != null)
                             {
                                 DbFactory.Default.Update(orderPayInfo);
                             }
                             if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                             {
                                 var orderItemInfo = orderItems.Where(p => p.OrderId == order.Id).FirstOrDefault();//虚拟订单项只有一个
                                 UpdateOrderItemEffectiveDateByIds(orderItems.Where(p => p.OrderId == order.Id).Select(a => a.Id).ToList(), order.PayDate.Value);
                                 if (orderItemInfo != null)
                                 {
                                     AddOrderVerificationCodeInfo(orderItemInfo.Quantity, orderItemInfo.OrderId, orderItemInfo.Id);
                                     SendMessageOnVirtualOrderPay(order, orderItemInfo.ProductId);
                                 }
                             }
                         });
                    var firstOrderItem = orderItems.First(p => p.OrderId == order.Id);
                    PaySuccessed_SingleOrderOp(order, firstOrderItem.ProductName, PaymentTypeName, "已付款", payTime);
                    isSendMsg = true;
                }
            }
            if (isSendMsg)
            {
                var firstOrder = orders.FirstOrDefault();
                if (firstOrder != null)
                {
                    var userId = firstOrder.UserId;
                    var orderItem = orderItems.FirstOrDefault(e => e.OrderId == firstOrder.Id); ;
                    
                    //发送通知消息
                    var orderMessage = new MessageOrderInfo();
                    orderMessage.OrderId = string.Join(",", orderIds);
                    orderMessage.ShopId = 0;
                    orderMessage.ShopIds = orders.Select(o => o.ShopId).ToList<long>();
                    orderMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;
                    orderMessage.TotalMoney = orders.Sum(a => a.OrderTotalAmount);
                    orderMessage.PaymentType = PaymentTypeName;

                    orderMessage.OrderTime = firstOrder.OrderDate;
                    orderMessage.PayTime = payTime;
                    orderMessage.PaymentType = "已付款";
                    orderMessage.ProductName = orderItem != null ? orderItem.ProductName : "";
                    orderMessage.UserName = firstOrder.UserName;


                    if (firstOrder != null && firstOrder.Platform == PlatformType.WeiXinSmallProg)
                    {
                        orderMessage.MsgOrderType = MessageOrderType.Applet;
                    }
                    if (firstOrder.DeliveryType == DeliveryType.SelfTake && firstOrder.ShopBranchId > 0)
                    {
                        orderMessage.PickupCode = firstOrder.PickupCode;
                        var shopbranch = DbFactory.Default.Get<ShopBranchInfo>().Where(s => s.Id == firstOrder.ShopBranchId).FirstOrDefault();
                        var address = firstOrder.Address;
                        if (shopbranch != null)
                            address = RegionApplication.GetFullName(shopbranch.AddressId) + " " + shopbranch.AddressDetail;
                        orderMessage.ShopBranchAddress = address;
                        if (firstOrder.OrderType != OrderInfo.OrderTypes.FightGroup)
                            Task.Factory.StartNew(() => Instance<IMessageService>.Create.SendMessageOnSelfTakeOrderPay(userId, orderMessage));
                    }
                    else
                    {
                        Task.Factory.StartNew(() => Instance<IMessageService>.Create.SendMessageOnOrderPay(userId, orderMessage));
                        if (firstOrder.OrderType != OrderInfo.OrderTypes.Virtual)
                        {
                            //发送给商家
                            Task.Factory.StartNew(() => Instance<IMessageService>.Create.SendMessageOnShopOrderShipping(orderMessage));
                        }
                    }
                }
            }
        }

        //TODO:【2015-09-01】预存款支付订单
        public void PayCapital(IEnumerable<long> orderIds, string payNo = null, long payId = 0)
        {
            Log.Info("PayCapital597进入");
            var orders = DbFactory.Default.Get<OrderInfo>().Where(item => item.Id.ExIn(orderIds)).ToList();
            var totalAmount = orders.Sum(e => e.OrderTotalAmount - e.CapitalAmount);
            var userid = orders.FirstOrDefault().UserId;
            var capital = DbFactory.Default.Get<CapitalInfo>().Where(e => e.MemId == userid).FirstOrDefault();
            if (capital == null)
            {
                throw new MallException("预存款金额少于订单金额");
            }
            if (capital.Balance < totalAmount)
            {
                throw new MallException("预存款金额少于订单金额");
            }
            var orderItems = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId.ExIn(orderIds)).ToList();
            foreach (var order in orders)
            {
                var needPay = order.TotalAmount - order.CapitalAmount;
                if (order != null && (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay))
                {
                    var orderPayInfo = DbFactory.Default.Get<OrderPayInfo>().Where(item => item.OrderId == order.Id && item.PayId == payId).FirstOrDefault();
                    if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
                    {
                        if (!FightGroupOrderCanPay(order.Id))
                        {
                            throw new MallException("拼团订单的状态为不可付款状态");
                        }
                    }
                    CapitalDetailInfo detail = new CapitalDetailInfo()
                    {
                        Amount = -needPay,
                        CapitalID = capital.Id,
                        CreateTime = DateTime.Now,
                        SourceType = CapitalDetailInfo.CapitalDetailType.Consume,
                        SourceData = order.Id.ToString(),
                        Id = this.GenerateOrderNumber()
                    };
                    DbFactory.Default
                        .InTransaction(() =>
                        {
                            order.PayDate = DateTime.Now;
                            order.PaymentTypeGateway = string.Empty;
                            if (order.OrderTotalAmount == 0 && order.CapitalAmount == 0)
                            {
                                order.PaymentTypeName = PAY_BY_INTEGRAL_PAYMENT_ID;
                            }
                            else
                            {
                                order.PaymentTypeName = PAY_BY_CAPITAL_PAYMENT_ID;
                            }
                            order.PaymentType = OrderInfo.PaymentTypes.Online;
                            if (order.DeliveryType == CommonModel.DeliveryType.SelfTake)
                            {
                                OperaOrderPickupCode(order);
                            }
                            else
                                order.OrderStatus = OrderInfo.OrderOperateStatus.WaitDelivery;
                            //设置实收金额=实付金额
                            order.ActualPayAmount += needPay;
                            order.LastModifyTime = DateTime.Now;
                            if (orderPayInfo != null)
                            {
                                orderPayInfo.PayState = true;
                                orderPayInfo.PayTime = DateTime.Now;
                            }
                            capital.Balance -= needPay;
                            DbFactory.Default.Add(detail);
                            //    SetActualItemPrice(order);         //平摊订单的优惠券金额
                            UpdateShopVisti(order);               // 修改店铺销量
                            UpdateProductVisti(orderItems.Where(p => p.OrderId == order.Id));           // 修改商品销量
                            UpdateLimitTimeBuyLog(orderItems.Where(p => p.OrderId == order.Id));   // 修改限时购销售数量
                            if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                            {
                                order.OrderStatus = OrderInfo.OrderOperateStatus.WaitVerification;//虚拟订单付款后，则为待消费
                            }
                            DbFactory.Default.Update(order);
                            DbFactory.Default.Update(orderPayInfo);
                            if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                            {
                                var orderItemInfo = orderItems.Where(p => p.OrderId == order.Id).FirstOrDefault();//虚拟订单项只有一个
                                UpdateOrderItemEffectiveDateByIds(orderItems.Where(p => p.OrderId == order.Id).Select(a => a.Id).ToList(), order.PayDate.Value);
                                if (orderItemInfo != null)
                                {
                                    AddOrderVerificationCodeInfo(orderItemInfo.Quantity, orderItemInfo.OrderId, orderItemInfo.Id);
                                    SendMessageOnVirtualOrderPay(order, orderItemInfo.ProductId);
                                }
                            }

                            var firstOrderItem = orderItems.First(p => p.OrderId == order.Id);
                            PaySuccessed_SingleOrderOp(order, firstOrderItem.ProductName, PAY_BY_CAPITAL_PAYMENT_ID, "已付款", DateTime.Now);
                        });
                }
            }
            var orderFirst = orders.FirstOrDefault();
            if (orderFirst != null)
            {
                //发送通知消息
                var orderMessage = new MessageOrderInfo();
                orderMessage.OrderId = string.Join(",", orderIds);
                orderMessage.OrderTime = orders.FirstOrDefault().OrderDate;
                orderMessage.ShopId = 0;
                orderMessage.ShopIds = orders.Select(o => o.ShopId).ToList<long>();
                orderMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;
                orderMessage.TotalMoney = orders.Sum(a => a.OrderTotalAmount);
                orderMessage.UserName = orders.FirstOrDefault().UserName;
                orderMessage.PaymentType = PAY_BY_CAPITAL_PAYMENT_ID;
                orderMessage.PayTime = DateTime.Now;
                orderMessage.PaymentType = "已付款";
                orderMessage.ProductName = orderItems.First().ProductName;
                var userId = orders.FirstOrDefault().UserId;
                if (orders.FirstOrDefault().Platform == PlatformType.WeiXinSmallProg)
                {
                    orderMessage.MsgOrderType = MessageOrderType.Applet;
                }
                var firstOrder = orders.FirstOrDefault();
                if (firstOrder.DeliveryType == DeliveryType.SelfTake && firstOrder.ShopBranchId > 0)
                {
                    orderMessage.PickupCode = firstOrder.PickupCode;
                    var shopbranch = DbFactory.Default.Get<ShopBranchInfo>().Where(s => s.Id == firstOrder.ShopBranchId).FirstOrDefault();
                    var address = firstOrder.Address;
                    if (shopbranch != null)
                        address = RegionApplication.GetFullName(shopbranch.AddressId) + " " + shopbranch.AddressDetail;
                    orderMessage.ShopBranchAddress = address;
                    if (firstOrder.OrderType != OrderInfo.OrderTypes.FightGroup)
                        Task.Factory.StartNew(() => Instance<IMessageService>.Create.SendMessageOnSelfTakeOrderPay(userId, orderMessage));
                }
                else
                {
                    Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnOrderPay(userId, orderMessage));
                    if (orderFirst.OrderType != OrderInfo.OrderTypes.Virtual)
                    {
                        //发送给商家
                        Task.Factory.StartNew(() => Instance<IMessageService>.Create.SendMessageOnShopOrderShipping(orderMessage));
                    }
                }
            }
        }

        private void PaySuccessed_SingleOrderOp(OrderInfo order, string productName, string paymentType, string paymentStatus, DateTime payTime)
        {
            //拼团成功
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                SetFightGroupOrderStatus(order.Id, FightGroupOrderJoinStatus.JoinSuccess);
            }
            //处理分销分佣
            ServiceProvider.Instance<IDistributionService>.Create.TreatedOrderDistributionBrokerage(order.Id, true);

            if (order.PaymentType != OrderInfo.PaymentTypes.CashOnDelivery)
            {//写入待结算
                WritePendingSettlnment(order);
            }

            //发布付款成功消息
            if (OnOrderPaySuccessed != null)
            {
                OnOrderPaySuccessed(order.Id);
            }
            else
            {//没有事件，则直接执行
                MemberApplication.OrderService_ProcessingSuccessInformation(order.Id, order);
            }
            //发送店铺通知消息
            var sordmsg = new MessageOrderInfo();
            sordmsg.OrderId = order.Id.ToString();
            sordmsg.ShopId = order.ShopId;
            sordmsg.ShopName = order.ShopName;
            sordmsg.SiteName = SiteSettingApplication.SiteSettings.SiteName;
            sordmsg.TotalMoney = order.OrderTotalAmount;
            sordmsg.PaymentType = paymentType;
            sordmsg.PayTime = payTime;
            sordmsg.OrderTime = order.OrderDate;
            sordmsg.PaymentStatus = paymentStatus;
            sordmsg.ProductName = productName;
            sordmsg.UserName = order.UserName;
            Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnShopHasNewOrder(order.ShopId, sordmsg));

            SendAppMessage(order);//支付成功后推送APP消息
        }

        public bool PayByCapitalIsOk(long userid, IEnumerable<long> orderIds)
        {
            var orders = DbFactory.Default.Get<OrderInfo>().Where(item => item.Id.ExIn(orderIds)).ToList();
            var totalAmount = orders.Sum(e => e.OrderTotalAmount);
            var capital = DbFactory.Default.Get<CapitalInfo>().Where(e => e.MemId == userid).FirstOrDefault();
            if (capital != null && capital.Balance >= totalAmount)
            {
                return true;
            }
            return false;
        }
        // 计算订单条目可退款金额
        // 看不懂具体逻辑，暂时不改
        public void CalculateOrderItemRefund(long orderId, bool isCompel = false)
        {
            var order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == orderId).FirstOrDefault();
            var orderitems = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId == orderId).ToList();
            if (order != null)
            {
                if (!isCompel)
                {
                    var ord1stitem = orderitems.FirstOrDefault();
                    if (ord1stitem == null || ord1stitem.EnabledRefundAmount == null || ord1stitem.EnabledRefundAmount <= 0
                        || ord1stitem.EnabledRefundIntegral == null)
                    {
                        isCompel = true;
                    }
                }
            }
            if (isCompel)
            {
                Log.Info("进入计算订单条目可退款金额：CalculateOrderItemRefund");
                int orditemcnt = orderitems.Count();
                int curnum = 0;
                decimal ordprosumnum = order.ProductTotalAmount - order.DiscountAmount - order.FullDiscount;
                decimal ordrefnum = order.ProductTotal;
                decimal ordindisnum = order.IntegralDiscount;
                decimal refcount = 0;
                decimal refincount = 0;   //只做整数处理
                long firstid = 0;
                var couponUseArea = 0;

                if (order.CouponId > 0)
                {
                    var selcoupon = DbFactory.Default.Get<CouponInfo>().Where(p => p.Id == order.CouponId).FirstOrDefault();
                    if (selcoupon != null && selcoupon.UseArea == 1)
                    {
                        couponUseArea = 1;
                    }
                }
                DbFactory.Default
                    .InTransaction(() =>
                    {
                        foreach (var item in orderitems)
                        {
                            decimal itemprosumnum = item.RealTotalPrice;
                            decimal curref = itemprosumnum - item.FullDiscount - item.CouponDiscount;
                            decimal curinref = 0;
                            if (curnum == 0)
                            {
                                curref = 0;    //首件退款为结果计算
                                firstid = item.Id;
                            }
                            else
                            {
                                //计算积分
                                if (ordprosumnum > 0)
                                {
                                    curinref = (decimal)Math.Round(((ordindisnum / ordprosumnum) * curref), 2);
                                    if (curinref < 0)
                                        curinref = 0;
                                }
                            }
                            item.EnabledRefundAmount = curref;
                            item.EnabledRefundIntegral = curinref;
                            refcount += curref;
                            refincount += curinref;
                            curnum++;
                            DbFactory.Default.Update(item);
                        }
                        //处理首件
                        var firstitem = orderitems.FirstOrDefault(d => d.Id == firstid);
                        if (firstitem != null)
                        {
                            firstitem.EnabledRefundAmount = ordrefnum - refcount;
                            firstitem.EnabledRefundIntegral = ordindisnum - refincount;
                        }
                        DbFactory.Default.Update(firstitem);
                    });
            }
        }

        // 商家同意退款，关闭订单
        public void AgreeToRefundBySeller(long orderId)
        {
            DbFactory.Default
                .InTransaction(() =>
                {
                    OrderInfo order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == orderId).FirstOrDefault();
                    if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
                    {
                        throw new MallException("只可以关闭待发货订单！");
                    }

                    order.OrderStatus = OrderInfo.OrderOperateStatus.Close;
                    order.CloseReason = "商家同意退款，取消订单";
                    order.LastModifyTime = DateTime.Now;
                    ReturnStock(order);
                    DbFactory.Default.Update(order);
                    //分销处理
                    //ServiceProvider.Instance<IDistributionService>.Create.TreatedOrderDistributionBrokerage(order.Id, false, DistributionBrokerageStatus.Settled);
                });
        }

        /// <summary>
        /// 获取销量
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="shopId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public long GetSaleCount(DateTime? startDate = null, DateTime? endDate = null, long? shopBranchId = null, long? shopId = null, long? productId = null, bool hasReturnCount = false, bool hasWaitPay = false)
        {
            long result = 0;
            var ordersql = DbFactory.Default.Get<OrderInfo>().Where(d => d.OrderStatus != OrderInfo.OrderOperateStatus.Close);
            var ordersqlAb = DbFactory.Default.Get<OrderInfo>();
            if (!hasWaitPay)
            {
                ordersql.Where(d => d.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay);
                ordersqlAb.Where(d => d.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay);
            }
            if (startDate.HasValue)
            {
                ordersql.Where(d => d.OrderDate >= startDate);
                ordersqlAb.Where(d => d.OrderDate >= startDate);
            }
            if (endDate.HasValue)
            {
                ordersql.Where(d => d.OrderDate <= endDate);
                ordersqlAb.Where(d => d.OrderDate <= endDate);
            }
            if (shopId.HasValue && shopId > 0)
            {
                ordersql.Where(d => d.ShopId == shopId.Value);
                ordersqlAb.Where(d => d.ShopId == shopId.Value);
            }
            if (shopBranchId.HasValue)
            {
                if (shopBranchId == 0)
                {  //查询总店
                    ordersql.Where(e => e.ShopBranchId == shopBranchId.Value || e.ShopBranchId.ExIsNull());
                    ordersqlAb.Where(e => e.ShopBranchId == shopBranchId.Value || e.ShopBranchId.ExIsNull());
                }
                else
                {
                    ordersql.Where(e => e.ShopBranchId == shopBranchId.Value);
                    ordersqlAb.Where(e => e.ShopBranchId == shopBranchId.Value);
                }
            }
            var orderids = ordersql.Select(d => d.Id).ToList<long>();            
            var orderitemsql = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.OrderId.ExIn(orderids));

            var orderabIds = ordersqlAb.Where(o=>o.DeliveryType != DeliveryType.SelfTake).Select(d => d.Id).ToList<long>();
            var orderitemsqlAb = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.OrderId.ExIn(orderabIds));
            if (productId.HasValue && productId > 0)
            {
                orderitemsql.Where(d => d.ProductId == productId.Value);
                orderitemsqlAb.Where(d => d.ProductId == productId.Value);
            }
            try
            {
                if (hasReturnCount)
                {
                    result = orderitemsql.Sum<long>(d => d.Quantity - d.ReturnQuantity);

                }
                else
                {
                    result = orderitemsql.Sum<long>(d => d.Quantity);
                }
                var branchId = shopBranchId.HasValue ? shopBranchId.Value : 0;
                //统计商家同意弃货的数量
                var refundsql = DbFactory.Default.Get<OrderRefundInfo>()
                    .Where(r => r.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed)
                    .Where(r => r.RefundMode == OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund)
                    .Where(r => r.IsReturn && r.ExpressCompanyName.ExIsNull() && r.ShipOrderNumber.ExIsNull());
                var orderitemids = orderitemsqlAb.Select(o => o.Id).ToList<long>();
                var refundCount = refundsql.Where(r => r.OrderItemId.ExIn(orderitemids)).Sum<long>(r => r.ReturnQuantity);
                result = result + refundCount;
            }
            catch
            {
                result = 0;
            }
            return result;
        }
        #endregion 订单操作 done

        #region 发票相关 done

        //发票内容
        public QueryPageModel<InvoiceContextInfo> GetInvoiceContexts(int PageNo, int PageSize = 20)
        {
            var data = DbFactory.Default.Get<InvoiceContextInfo>().OrderByDescending(o => o.Id).ToPagedList(PageNo, PageSize);
            QueryPageModel<InvoiceContextInfo> result = new QueryPageModel<InvoiceContextInfo>();
            result.Models = data;
            result.Total = data.TotalRecordCount;
            return result;
        }
        //发票内容
        public List<InvoiceContextInfo> GetInvoiceContexts()
        {
            return DbFactory.Default.Get<InvoiceContextInfo>().ToList();
        }

        public void SaveInvoiceContext(InvoiceContextInfo info)
        {
            if (info.Id >= 0)  //update
            {
                var model = DbFactory.Default.Get<InvoiceContextInfo>().Where(p => p.Id == info.Id).FirstOrDefault();
                model.Name = info.Name;
                DbFactory.Default.Update(model);
            }
            else //create
            {
                DbFactory.Default.Add(info);
            }
        }

        public void DeleteInvoiceContext(long id)
        {
            DbFactory.Default.Del<InvoiceContextInfo>().Where(n => n.Id == id).Succeed();
        }

        //发票抬头
        public List<InvoiceTitleInfo> GetInvoiceTitles(long userid, InvoiceType type)
        {
            var models = DbFactory.Default.Get<InvoiceTitleInfo>().Where(p => p.UserId == userid && p.InvoiceType == type).ToList();
            if (type == InvoiceType.OrdinaryInvoices)
            {
                models = models.Where(p => p.Name != "个人").ToList();
            }
            return models;
        }

        public long SaveInvoiceTitle(InvoiceTitleInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.Name))
            {
                return -1;
            }
            //if (info.Name == "个人")
            //{
            //    return 0;
            //}
            var models = DbFactory.Default.Get<InvoiceTitleInfo>()
                .Where(i => i.InvoiceType == InvoiceType.OrdinaryInvoices).ToList();
            var flag = true;
            if (models.Count > 0)
            {
                flag = DbFactory.Default.Set<InvoiceTitleInfo>()
                    .Where(i => i.InvoiceType == InvoiceType.OrdinaryInvoices)
                    .Set(p => p.IsDefault, 0).Succeed();
            }
            if (flag)
            {
                //已存在则不添加
                var model = DbFactory.Default.Get<InvoiceTitleInfo>().Where(p => p.UserId == info.UserId && p.Name == info.Name && p.InvoiceType == InvoiceType.OrdinaryInvoices).FirstOrDefault();
                if (model != null)
                {
                    model.Name = info.Name;
                    model.Code = info.Code;
                    model.InvoiceContext = info.InvoiceContext;
                    model.IsDefault = 1;
                    DbFactory.Default.Update<InvoiceTitleInfo>(model);
                    return 0;
                }
                var result = DbFactory.Default.Add(info);
            }
            return info.Id;
        }

        /// <summary>
        /// 保存发票信息
        /// </summary>
        /// <param name="info"></param>
        public void SaveInvoiceTitleNew(InvoiceTitleInfo info)
        {
            if (info.InvoiceType == InvoiceType.OrdinaryInvoices)
            {
                SaveInvoiceTitle(info);
            }
            else
            {
                var model = DbFactory.Default.Get<InvoiceTitleInfo>().Where(p => p.UserId == info.UserId && p.InvoiceType == info.InvoiceType && p.IsDefault == 1).FirstOrDefault();
                if (model == null)
                    DbFactory.Default.Add<InvoiceTitleInfo>(info);
                else
                {
                    model.Name = info.Name;
                    model.Code = info.Code;
                    model.RegisterAddress = info.RegisterAddress;
                    model.RegisterPhone = info.RegisterPhone;
                    model.BankName = info.BankName;
                    model.BankNo = info.BankNo;
                    model.RealName = info.RealName;
                    model.CellPhone = info.CellPhone;
                    model.Email = info.Email;
                    model.RegionID = info.RegionID;
                    model.Address = info.Address;
                    DbFactory.Default.Update<InvoiceTitleInfo>(model);
                }
                if(info.InvoiceType == InvoiceType.ElectronicInvoice)
                {
                    info.InvoiceType = InvoiceType.OrdinaryInvoices;
                    SaveInvoiceTitle(info);
                }
            }
        }

        public long EditInvoiceTitle(InvoiceTitleInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.Name) || string.IsNullOrWhiteSpace(info.Code))
            {
                return -1;
            }
            if (info.Name == "个人")
            {
                return 0;
            }
            //已存在则不添加
            var entity = DbFactory.Default.Get<InvoiceTitleInfo>().Where(p => p.UserId == info.UserId && p.Id == info.Id).FirstOrDefault();
            if (null != entity)
            {
                var result = DbFactory.Default.Set<InvoiceTitleInfo>()
                .Where(i => i.InvoiceType == InvoiceType.OrdinaryInvoices)
                .Set(p => p.IsDefault, 0).Succeed();
                if (result)
                {
                    entity.Name = info.Name;
                    entity.Code = info.Code;
                    entity.IsDefault = 1;
                    DbFactory.Default.Update(entity);
                    return entity.Id;
                }
            }

            return 0;
        }
        public void DeleteInvoiceTitle(long id, long userId = 0)
        {
            var sql = DbFactory.Default.Get<InvoiceTitleInfo>().Where(d => d.Id == id);
            if (userId > 0)
            {
                sql.Where(d => d.UserId == userId);
            }
            var obj = sql.FirstOrDefault();
            if (obj != null)
            {
                DbFactory.Default.Del(obj);
            }
        }

        #endregion 发票相关 done

        #region 私有函数

        /// <summary>
        /// 更改库存
        /// </summary>
        private void ReturnStock(OrderInfo order)
        {
            DbFactory.Default
                .InTransaction(() =>
                {
                    var orderItems =  EngineContext.Current.Resolve<IOrderService>().GetOrderItemsByOrderId(order.Id);

                    foreach (var orderItem in orderItems)
                    {
                        SKUInfo sku = DbFactory.Default.Get<SKUInfo>().Where(p => p.Id == orderItem.SkuId).FirstOrDefault();
                        if (sku != null)
                        {
                            //if (order.DeliveryType == CommonModel.DeliveryType.SelfTake)
                            if (order.DeliveryType == DeliveryType.SelfTake || order.ShopBranchId > 0)//此处如果是系统自动将订单匹配到门店或者由商家手动分配订单到门店，其配送方式仍为快递。所以改为也能根据门店ID去判断
                            {
                                var sbSku = DbFactory.Default.Get<ShopBranchSkuInfo>().Where(p => p.SkuId == sku.Id && p.ShopBranchId == order.ShopBranchId).FirstOrDefault();
                                if (sbSku != null)
                                {
                                    sbSku.Stock += (int)orderItem.Quantity;
                                    DbFactory.Default.Update(sbSku);
                                }
                            }
                            else
                            {
                                sku.Stock += orderItem.Quantity;
                                DbFactory.Default.Update(sku);
                            }

                            // 限购还原活动库存
                            if (order.OrderType == OrderInfo.OrderTypes.LimitBuy)
                            {
                                var flashSaleDetailInfo = DbFactory.Default.Get<FlashSaleDetailInfo>().Where(a => a.SkuId == orderItem.SkuId && a.FlashSaleId == orderItem.FlashSaleId).FirstOrDefault();
                                if (flashSaleDetailInfo != null)
                                {                                    
                                    flashSaleDetailInfo.TotalCount += (int)orderItem.Quantity;
                                    DbFactory.Default.Update(flashSaleDetailInfo);
                                }
                            }
                        }
                    }
                });
        }

        private void CancelIntegral(MemberInfo member, long orderId, decimal integralDiscount)
        {
            if (integralDiscount == 0)
            {
                return; //没使用积分直接返回
            }
            var IntegralExchange = ServiceProvider.Instance<IMemberIntegralService>.Create.GetIntegralChangeRule();
            if (IntegralExchange == null)
            {
                return; //没设置兑换规则直接返回
            }
            var IntegralPerMoney = IntegralExchange.IntegralPerMoney;
            var integral = Convert.ToInt32(Math.Floor(integralDiscount * IntegralPerMoney));
            var record = new Mall.Entities.MemberIntegralRecordInfo();
            record.UserName = member.UserName;
            record.MemberId = member.Id;
            record.RecordDate = DateTime.Now;
            record.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.Cancel;
            record.ReMark = "订单被取消，返还积分，订单号:" + orderId.ToString();
            var action = new Mall.Entities.MemberIntegralRecordActionInfo();
            action.VirtualItemTypeId = Mall.Entities.MemberIntegralInfo.VirtualItemType.Cancel;
            action.VirtualItemId = orderId;
            record.MemberIntegralRecordActionInfo.Add(action);
            var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(Mall.Entities.MemberIntegralInfo.IntegralType.Cancel, integral);
            ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegralNotAddHistoryIntegrals(record, memberIntegral);
        }

        /// <summary>
        /// 退回预存款
        /// </summary>
        /// <param name="member"></param>
        /// <param name="order"></param>
        /// <param name="capitalAmount"></param>
        private void CancelCapital(MemberInfo member, OrderInfo order, decimal capitalAmount)
        {
            DbFactory.Default
                .InTransaction(() =>
                {
                    if (capitalAmount <= 0) return;
                    var entity = DbFactory.Default.Get<CapitalInfo>().Where(e => e.MemId == member.Id).FirstOrDefault();
                    if (entity == null)
                    {
                        throw new MallException("未存在预存款记录");
                    }

                    CapitalDetailInfo detail = new CapitalDetailInfo()
                    {
                        Amount = capitalAmount,
                        CapitalID = entity.Id,
                        CreateTime = DateTime.Now,
                        SourceType = CapitalDetailInfo.CapitalDetailType.Refund,
                        SourceData = order.Id.ToString(),
                        Id = this.GenerateOrderNumber()
                    };
                    order.ActualPayAmount -= capitalAmount;//会员净消费会实时统计此字段 
                    member.NetAmount -= capitalAmount;//同步修改会员净消费金额
                    entity.Balance += capitalAmount;
                    DbFactory.Default.Add(detail);
                    DbFactory.Default.Update(entity);
                    DbFactory.Default.Update(order);
                    DbFactory.Default.Update(member);
                });
        }

        /// <summary>
        /// 订单取消返回优惠卷 TODO:ZYF
        /// </summary>
        /// <param name="member"></param>
        /// <param name="order"></param>
        private void ReturnCoupon(MemberInfo member, OrderInfo order)
        {
            var couponId = order.CouponId;
            if (couponId <= 0) return;
            var coupon = DbFactory.Default.Get<CouponInfo>().Where(c => c.Id == couponId).FirstOrDefault();
            if (coupon == null)
                throw new MallException("优惠卷不存在");
            var couponRecord = DbFactory.Default.Get<CouponRecordInfo>().Where(r => r.CouponId == couponId && r.UserId == member.Id && r.OrderId == order.Id).FirstOrDefault();
            if (couponRecord == null)
                throw new MallException("用户领取优惠卷记录不存在");

            couponRecord.UsedTime = null;
            couponRecord.OrderId = null;
            couponRecord.CounponStatus = CouponRecordInfo.CounponStatuses.Unuse;
            DbFactory.Default.Update(couponRecord);
        }

        /// <summary>
        /// 订单取消退回代金红包 TODO:ZYF
        /// </summary>
        /// <param name="member"></param>
        /// <param name="order"></param>
        private void ReturnShopBonus(OrderInfo order)
        {
            var shopBonusReceive = DbFactory.Default.Get<ShopBonusReceiveInfo>().Where(b => b.UserId == order.UserId && b.UsedOrderId == order.Id).FirstOrDefault();
            if (shopBonusReceive == null)
                throw new MallException("代金红包不存在");
            shopBonusReceive.State = ShopBonusReceiveInfo.ReceiveState.NotUse;
            shopBonusReceive.UsedTime = null;
            shopBonusReceive.UsedOrderId = null;
            DbFactory.Default.Update(shopBonusReceive);
        }

        //消费获得积分
        private void AddIntegral(MemberInfo member, long orderId, decimal orderTotal)
        {
            if (orderTotal <= 0)
            {
                return;
            }
            var IntegralExchange = ServiceProvider.Instance<IMemberIntegralService>.Create.GetIntegralChangeRule();
            if (IntegralExchange == null)
            {
                return; //没设置兑换规则直接返回
            }
            var MoneyPerIntegral = IntegralExchange.MoneyPerIntegral;
            if (MoneyPerIntegral == 0)
            {
                return;
            }
            var integral = Convert.ToInt32(Math.Floor(orderTotal / MoneyPerIntegral));
            Mall.Entities.MemberIntegralRecordInfo record = new Mall.Entities.MemberIntegralRecordInfo();
            record.UserName = member.UserName;
            record.MemberId = member.Id;
            record.RecordDate = DateTime.Now;
            record.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.Consumption;
            Mall.Entities.MemberIntegralRecordActionInfo action = new Mall.Entities.MemberIntegralRecordActionInfo();
            action.VirtualItemTypeId = Mall.Entities.MemberIntegralInfo.VirtualItemType.Consumption;
            action.VirtualItemId = orderId;
            record.MemberIntegralRecordActionInfo.Add(action);
            var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(Mall.Entities.MemberIntegralInfo.IntegralType.Consumption, integral);
            ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(record, memberIntegral);
        }

        // 添加订单操作日志
        private void AddOrderOperationLog(long orderId, string userName, string operateContent)
        {
            OrderOperationLogInfo orderOperationLog = new OrderOperationLogInfo();
            orderOperationLog.Operator = userName;
            orderOperationLog.OrderId = orderId;
            orderOperationLog.OperateDate = DateTime.Now;
            orderOperationLog.OperateContent = operateContent;

            DbFactory.Default.Add(orderOperationLog);
        }

        public void DeductionIntegral(MemberInfo member, IEnumerable<long> Ids, int integral)
        {
            if (integral == 0)
            {
                return;
            }
            var record = new MemberIntegralRecordInfo();
            record.UserName = member.UserName;
            record.MemberId = member.Id;
            record.RecordDate = DateTime.Now;
            var remark = "订单号:";
            record.TypeId = MemberIntegralInfo.IntegralType.Exchange;
            foreach (var t in Ids)
            {
                remark += t + ",";
                var action = new MemberIntegralRecordActionInfo();
                action.VirtualItemTypeId = MemberIntegralInfo.VirtualItemType.Exchange;
                action.VirtualItemId = t;
                record.MemberIntegralRecordActionInfo.Add(action);
            }
            record.ReMark = remark.TrimEnd(',');
            var memberIntegral = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create.Create(Mall.Entities.MemberIntegralInfo.IntegralType.Exchange, integral);
            ServiceProvider.Instance<IMemberIntegralService>.Create.AddMemberIntegral(record, memberIntegral);
        }
        /// <summary>
        /// 减少预存款
        /// </summary>
        /// <param name="member"></param>
        /// <param name="Ids"></param>
        /// <param name="integral"></param>
        public void DeductionCapital(Entities.MemberInfo member, List<OrderInfo> orders, decimal capital)
        {
            if (capital == 0)
            {
                return;
            }
            var entity = DbFactory.Default.Get<CapitalInfo>().Where(e => e.MemId == member.Id).FirstOrDefault();
            if (entity == null)
            {
                throw new MallException("预存款金额少于订单金额");
            }
            DbFactory.Default
                .InTransaction(() =>
                {
                    foreach (var order in orders)
                    {
                        CapitalDetailInfo detail = new CapitalDetailInfo()
                        {
                            Amount = -order.CapitalAmount,
                            CapitalID = entity.Id,
                            CreateTime = DateTime.Now,
                            SourceType = CapitalDetailInfo.CapitalDetailType.Consume,
                            SourceData = order.Id.ToString(),
                            Id = this.GenerateOrderNumber()
                        };

                        entity.Balance -= order.CapitalAmount;
                        DbFactory.Default.Add(detail);
                        DbFactory.Default.Update(entity);
                    }
                });
        }

        // 获取积分所能兑换的总金额
        public decimal GetIntegralDiscountAmount(int integral, long userId)
        {
            if (integral == 0)
            {
                return 0;
            }
            var integralService = Instance<IMemberIntegralService>.Create;
            var userIntegral = MemberIntegralApplication.GetAvailableIntegral(userId);
            if (userIntegral < integral)
                throw new MallException("用户积分不足不能抵扣订单");

            var exchangeModel = integralService.GetIntegralChangeRule();
            var integralPerMoney = exchangeModel == null ? 0 : exchangeModel.IntegralPerMoney;
            decimal money = 0;
            if (integralPerMoney > 0)
            {
                money = integral / (decimal)integralPerMoney;
                money = Math.Floor(money * (decimal)Math.Pow(10, 2)) / (decimal)Math.Pow(10, 2);
            }
            //return integralPerMoney == 0 ? 0 : Math.Round(integral / (decimal)integralPerMoney, 2, MidpointRounding.AwayFromZero);
            return money;
        }

        /// <summary>
        /// 获取预存款总金额
        /// </summary>
        /// <param name="capital"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public decimal GetCapitalAmount(decimal capital, long userId)
        {
            if (capital == 0) return 0;
            var userCapital = MemberCapitalApplication.GetBalanceByUserId(userId);
            if (userCapital < capital)
                throw new MallException("用户预付款不足不能提交订单");
            return capital;
        }

        // 获取单个商品的销售价格
        public decimal GetSalePrice(string skuId, decimal salePrice, long? collid, int SkuCount, bool IslimitBuy = false)
        {
            var price = salePrice;
            if (collid.HasValue && collid.Value != 0 && SkuCount > 1)//组合购且大于一个商品
            {
                var collsku = ServiceProvider.Instance<ICollocationService>.Create.GetColloSku(collid.Value, skuId);
                if (collsku != null)
                {
                    price = collsku.Price;
                }
                //获取组合购的价格
            }
            else if (SkuCount == 1 && IslimitBuy) //订单是限时购
            {
                var limit = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetDetail(skuId);
                if (limit != null)
                {
                    price = (decimal)limit.Price;
                }
            }
            return price;
        }






        /// <summary>
        /// 獲取當個訂單项所使用的优惠券的金额大小
        /// </summary>
        /// <param name="realTotalPrice">當前商品的減價後的價格</param>
        /// <param name="ProductTotal">訂單所有商品的總價格</param>
        /// <param name="couponDisCount">總的優惠券金額</param>
        /// <returns></returns>
        private decimal GetItemCouponDisCount(decimal realTotalPrice, decimal ProductTotal, decimal couponDiscount)
        {
            var ItemCouponDiscount = Math.Round(couponDiscount * realTotalPrice / ProductTotal, 2);
            return ItemCouponDiscount;
        }


        //发送短信通知
        public void SendMessage(IEnumerable<OrderInfo> infos)
        {
            Log.Info("进入SendMessage发送短信通知方法里面");
            if (infos == null || infos.Count() == 0)
            {
                return;
            }
            var orderMessage = new MessageOrderInfo();
            var orderIds = infos.Select(item => item.Id).ToArray();
            orderMessage.OrderId = string.Join(",", orderIds);
            var model = infos.FirstOrDefault();
            if (orderIds.Length > 1)
            {
                orderMessage.ShopId = 0;
            }
            else
            {
                orderMessage.ShopId = model.ShopId;
                orderMessage.ShopName = model.ShopName;
            }
            var userId = model.UserId;
            orderMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;
            orderMessage.TotalMoney = infos.Sum(a => a.OrderTotalAmount);
            orderMessage.UserName = model.UserName;
            orderMessage.Quantity = GetOrderTotalProductCount(model.Id);
            orderMessage.OrderTime = model.OrderDate;
            orderMessage.ProductName = model.OrderItemInfo.FirstOrDefault().ProductName;

            if (model.Platform == PlatformType.WeiXinSmallProg)
            {
                orderMessage.MsgOrderType = MessageOrderType.Applet;
            }
            Task.Factory.StartNew(() => Instance<IMessageService>.Create.SendMessageOnOrderCreate(userId, orderMessage));
        }

        // 更新限时购活动购买记录
        private void UpdateLimitTimeBuyLog(IEnumerable<OrderItemInfo> orderItems)
        {
            ServiceProvider.Instance<ILimitTimeBuyService>.Create.IncreaseSaleCount(orderItems.Select(a => a.OrderId).ToList());
        }

        private void UpdateProductVisti(IEnumerable<OrderItemInfo> orderItems)
        {
            var date1 = DateTime.Now.Date;
            var date2 = DateTime.Now.Date.AddDays(1);
            DbFactory.Default
                .InTransaction(() =>
                {
                    foreach (OrderItemInfo orderItem in orderItems)
                    {
                        var productVisti = DbFactory.Default
                            .Get<ProductVistiInfo>()
                            .Where(item => item.ProductId == orderItem.ProductId && item.Date >= date1 && item.Date <= date2)
                            .FirstOrDefault();
                        if (productVisti == null)
                        {
                            productVisti = new ProductVistiInfo();
                            productVisti.ProductId = orderItem.ProductId;
                            productVisti.Date = DateTime.Now.Date;
                            productVisti.OrderCounts = 0;
                            DbFactory.Default.Add(productVisti);
                        }

                        var productInfo = DbFactory.Default.Get<ProductInfo>().Where(n => n.Id == orderItem.ProductId).FirstOrDefault();
                        var searchProduct = DbFactory.Default.Get<SearchProductInfo>().Where(r => r.ProductId == orderItem.ProductId).FirstOrDefault();
                        if (productInfo != null)
                        {
                            productInfo.SaleCounts += orderItem.Quantity;
                            DbFactory.Default.Update(productInfo);
                            if (searchProduct != null)
                            {
                                searchProduct.SaleCount += (int)orderItem.Quantity;
                                DbFactory.Default.Update(searchProduct);
                            }
                        }
                        productVisti.SaleCounts += orderItem.Quantity;
                        productVisti.SaleAmounts += orderItem.RealTotalPrice;
                        DbFactory.Default.Update(productVisti);
                    }
                });
        }

        // 更新商品购买的订单总数
        public void UpdateProductVistiOrderCount(long orderId)
        {
            DbFactory.Default
                .InTransaction(() =>
                {
                    //获取订单明细
                    var items = DbFactory.Default.Get<OrderItemInfo>().Where(o => o.OrderId == orderId).ToList();
                    //更新商品购买的订单总数
                    foreach (OrderItemInfo model in items)
                    {
                        ProductVistiInfo productVisti = DbFactory.Default.Get<ProductVistiInfo>().Where(p => p.ProductId == model.ProductId).FirstOrDefault();
                        if (productVisti != null)
                        {
                            productVisti.OrderCounts = (productVisti.OrderCounts == null ? 0 : productVisti.OrderCounts) + 1;
                            DbFactory.Default.Update(productVisti);
                        }
                    }
                });
        }

        // 更新店铺访问量
        private void UpdateShopVisti(OrderInfo order)
        {//TODO:店铺访问量统计，暂时取消实时统计
            /* 
            var date = DateTime.Now.Date;
            ShopVistiInfo shopVisti = Context.ShopVistiInfo.FindBy(item =>
                item.ShopId == order.ShopId && item.Date.Year == date.Year && item.Date.Month == date.Month && item.Date.Day == date.Day).FirstOrDefault();
            if (shopVisti == null)
            {
                shopVisti = new ShopVistiInfo();
                shopVisti.ShopId = order.ShopId;
                shopVisti.Date = DateTime.Now.Date;
                Context.ShopVistiInfo.Add(shopVisti);
            }
            shopVisti.SaleCounts += order.OrderProductQuantity;
            shopVisti.SaleAmounts += order.ProductTotalAmount;
            Context.SaveChanges();
             */
        }

        /// <summary>
        /// 是否可以发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private bool CanSendGood(long orderId)
        {
            bool result = false;
            var ordobj = DbFactory.Default.Get<OrderInfo>().Where(d => d.Id == orderId).FirstOrDefault();
            if (ordobj == null)
            {
                throw new MallException("错误的订单编号");
            }
            if (ordobj.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                var fgord = DbFactory.Default.Get<FightGroupOrderInfo>().Where(d => d.OrderId == orderId).FirstOrDefault();
                if (fgord.CanSendGood)
                {
                    result = true;
                }
            }
            else
            {
                result = true;
            }
            return result;
        }

        #region 拼团订单
        /// <summary>
        /// 设定拼团订单状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private FightGroupOrderJoinStatus SetFightGroupOrderStatus(long orderId, FightGroupOrderJoinStatus status)
        {
            FightGroupOrderJoinStatus result = _iFightGroupService.SetOrderStatus(orderId, status);
            return result;
        }
        /// <summary>
        /// 拼团订单是否可以付款
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private bool FightGroupOrderCanPay(long orderId)
        {
            bool result = false;
            result = _iFightGroupService.OrderCanPay(orderId);
            return result;
        }
        #endregion

        #endregion 私有函数

        public decimal GetRecentMonthAveragePrice(long shopId, long productId)
        {
            //var list = (from o in Context.OrderInfo
            //            join oi in Context.OrderItemInfo on o.Id equals oi.OrderId
            //            where o.ShopId == shopId && o.OrderStatus == OrderInfo.OrderOperateStatus.Finish
            //            && o.PayDate >= start && o.PayDate <= DateTime.Now && oi.ProductId == productId
            //            select oi
            //         ).Take(30);
            decimal average = 0;
            var start = DateTime.Now.AddMonths(-1);
            average = DbFactory.Default
                .Get<Mall.Entities.OrderInfo>()
                .InnerJoin<Mall.Entities.OrderItemInfo>((order, orderitem) => order.Id == orderitem.OrderId)
                .Where(n => n.ShopId == shopId && n.OrderStatus == Entities.OrderInfo.OrderOperateStatus.Finish &&
                    n.PayDate >= start && n.PayDate <= DateTime.Now)
                .Where<Mall.Entities.OrderItemInfo>(n => n.ProductId == productId)
                .Avg<Mall.Entities.OrderItemInfo, decimal>(n => n.RealTotalPrice - n.DiscountAmount);

            if (average <= 0)
            {
                var product = DbFactory.Default
                    .Get<Mall.Entities.ProductInfo>()
                    .Where(p => p.Id == productId)
                    .FirstOrDefault();
                if (null != product)
                    average = product.MinSalePrice;
            }

            return average;
        }

        #region 旧方法
        //public void AutoConfirmOrder()
        //{
        //    try
        //    {
        //        DbFactory.Default
        //            .InTransaction(() =>
        //            {
        //                //  var siteSetting = SiteSettingApplication.SiteSettings;
        //                var siteSetting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettingsByObjectCache();
        //                //退换货间隔天数
        //                int intIntervalDay = siteSetting == null ? 7 : (siteSetting.NoReceivingTimeout == 0 ? 7 : siteSetting.NoReceivingTimeout);
        //                DateTime waitReceivingDate = DateTime.Now.AddDays(-intIntervalDay);
        //                var orders = DbFactory.Default.Get<OrderInfo>().Where(a => a.ShippingDate < waitReceivingDate && a.OrderStatus == OrderInfo.OrderOperateStatus.WaitReceiving).ToList();
        //                foreach (var o in orders)
        //                {
        //                    //Log.Debug("orderid=" + o.Id.ToString());
        //                    o.OrderStatus = OrderInfo.OrderOperateStatus.Finish;
        //                    o.CloseReason = "完成过期未确认收货的订单";
        //                    o.FinishDate = DateTime.Now;
        //                    //过了售后维权期再给积分
        //                    //var member = DbFactory.Default.Get<MemberInfo>().Where(a => a.Id == o.UserId).FirstOrDefault();
        //                    //AddIntegral(member, o.Id, o.ProductTotalAmount - o.DiscountAmount - o.IntegralDiscount - o.RefundTotalAmount);//增加积分
        //                    if (o.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery)
        //                    {
        //                        o.PayDate = DateTime.Now;
        //                        var orderItems = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId == o.Id).ToList();
        //                        UpdateProductVisti(orderItems);
        //                    }

        //                    DbFactory.Default.Update(o);

        //                    #region 到期自动订单确认,写入待结算
        //                    WritePendingSettlnment(o);
        //                    #endregion
        //                }
        //            });
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("AutoConfirmOrder:" + ex.Message + "/r/n" + ex.StackTrace);
        //    }
        //}
        #endregion 

        /// <summary>
        /// 写入待结算
        /// </summary>
        /// <param name="o"></param>
        public void WritePendingSettlnment(OrderInfo o)
        {
            try
            {
                DbFactory.Default
                    .InTransaction(() =>
                    {
                        var orderDetail = DbFactory.Default.Get<OrderItemInfo>().Where(a => a.OrderId == o.Id).ToList();

                        /*
                         订单金额=商品总价 - 满额优惠 - 优惠券 + 运费 + 税费
                         */
                        var item = new PendingSettlementOrderInfo();
                        item.ShopId = o.ShopId;
                        item.ShopName = o.ShopName;
                        item.OrderId = o.Id;
                        item.FreightAmount = o.Freight;
                        item.TaxAmount = o.Tax;
                        item.IntegralDiscount = o.IntegralDiscount;
                        item.OrderType = o.OrderType;
                        //平台佣金 = 商品金额 * 对应分类设置的平台抽佣比例（多种商品分开计算）
                        //单项四舍五入后，再累加
                        foreach(var c in orderDetail)
                        {
                            item.PlatCommission += Math.Round((c.RealTotalPrice - c.CouponDiscount - c.FullDiscount) * c.CommisRate, 2, MidpointRounding.AwayFromZero);
                        }
                        //item.PlatCommission = orderDetail.Sum(c => (c.RealTotalPrice - c.CouponDiscount - c.FullDiscount) * c.CommisRate);
                        //item.PlatCommission = Math.Round(item.PlatCommission, 2, MidpointRounding.AwayFromZero);//((int)(item.PlatCommission * 100)) / (decimal)100;//与退款计算佣金方式一致 //Math.Round(item.PlatCommission, 2, MidpointRounding.AwayFromZero);//四舍五入算法

                        decimal refundAmount = 0M;//hasRefundOrdersDetails.Sum(c => c.Amount);//退款金额
                        item.RefundAmount = refundAmount;
                        //平台佣金退还
                        item.PlatCommissionReturn = 0M; //hasRefundOrdersDetails.Sum(a => a.ReturnPlatCommission);

                        //统计订单分销佣金
                        item.DistributorCommission = ServiceProvider.Instance<IDistributionService>.Create.GetDistributionBrokerageAmount(o.Id);

                        //结算金额=商品总价 - 满额优惠 - 优惠券 + 运费 + 税费 - 平台佣金 - 分销佣金 - 退款金额
                        item.ProductsAmount = o.ProductTotalAmount - o.DiscountAmount - o.FullDiscount;
                        item.SettlementAmount = item.ProductsAmount + item.FreightAmount + item.TaxAmount - item.PlatCommission - item.DistributorCommission - refundAmount + item.PlatCommissionReturn + item.DistributorCommissionReturn;
                        item.CreateDate = DateTime.Now;
                        if (o.FinishDate.HasValue)
                        {
                            item.OrderFinshTime = (DateTime)o.FinishDate;
                        }

                        item.PaymentTypeName = o.PaymentTypeDesc;
                        item.OrderAmount = o.OrderTotalAmount;
                        DbFactory.Default.Add(item);
                        //更新店铺资金账户
                        var m = DbFactory.Default.Get<ShopAccountInfo>().Where(a => a.ShopId == o.ShopId).FirstOrDefault();
                        if (m != null)
                        {
                            m.PendingSettlement += item.SettlementAmount;
                            DbFactory.Default.Update(m);
                        }
                        //更新平台资金账户
                        var plat = DbFactory.Default.Get<PlatAccountInfo>().FirstOrDefault();
                        if (plat != null)
                        {
                            //  var mid = item.PlatCommission - item.PlatCommissionReturn;
                            plat.PendingSettlement += item.SettlementAmount;
                            DbFactory.Default.Update(plat);
                        }
                    });
            }
            catch (Exception ex)
            {
                Log.Error("WritePendingSettlnment:" + ex.Message + "/r/n" + ex.StackTrace);
            }
        }
        /// <summary>
        /// 更新待结算订单完成时间
        /// </summary>
        /// <param name="order"></param>
        private void UpdatePendingSettlnmentFinishDate(long orderid, DateTime dt)
        {
            DbFactory.Default.Set<PendingSettlementOrderInfo>().Set(e => e.OrderFinshTime, dt).Where(e => e.OrderId == orderid).Succeed();
        }
        //public void AutoCloseOrder()
        //{
        //    try
        //    {
        //        DbFactory.Default
        //            .InTransaction(() =>
        //            {
        //                var date = DateTime.Now;
        //                //  var siteSetting = SiteSettingApplication.SiteSettings;
        //                //采用asp.net cache
        //                var siteSetting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettingsByObjectCache();
        //                var orders = DbFactory.Default.Get<OrderInfo>().Where(a => a.OrderDate < date && a.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay).ToList();
        //                var productService = ServiceProvider.Instance<Mall.IServices.IProductService>.Create;
        //                foreach (var o in orders)
        //                {
        //                    int hours = siteSetting == null ? 2 : (siteSetting.UnpaidTimeout == 0 ? 2 : siteSetting.UnpaidTimeout);
        //                    if (DateTime.Now.Subtract(o.OrderDate).TotalHours >= hours)
        //                    {
        //                        //Log.Debug("OrderJob:orderid=" + o.Id.ToString());
        //                        o.OrderStatus = OrderInfo.OrderOperateStatus.Close;
        //                        o.CloseReason = "过期没付款，自动关闭";
        //                        //加回库存
        //                        ReturnStock(o);
        //                        var member = DbFactory.Default.Get<MemberInfo>().Where(a => a.Id == o.UserId).FirstOrDefault();
        //                        CancelIntegral(member, o.Id, o.IntegralDiscount);//取消订单增加积分
        //                        if (o.CapitalAmount > 0)
        //                            CancelCapital(member, o, o.CapitalAmount);//退回预付款
        //                                                                      //拼团失败
        //                        if (o.OrderType == OrderInfo.OrderTypes.FightGroup)
        //                        {
        //                            SetFightGroupOrderStatus(o.Id, FightGroupOrderJoinStatus.JoinFailed);
        //                        }
        //                        DbFactory.Default.Update(o);
        //                    }
        //                }
        //            });
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("AutoCloseOrder:" + ex.Message + "/r/n" + ex.StackTrace);
        //    }
        //}

        public void ConfirmZeroOrder(IEnumerable<long> Ids, long userId)
        {
            var orders = DbFactory.Default
                .Get<OrderInfo>()
                .Where(item => item.Id.ExIn(Ids) && item.UserId == userId && item.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay
                    || item.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery && item.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery &&
                    item.Id.ExIn(Ids) && item.UserId == userId)
                .ToList();
            DbFactory.Default
                .InTransaction(() =>
                {
                    foreach (var order in orders)
                    {
                        if (order.OrderWaitPayAmountIsZero)
                        {
                            if (order.DeliveryType == CommonModel.DeliveryType.SelfTake)
                            {
                                OperaOrderPickupCode(order);
                            }
                            else
                                order.OrderStatus = OrderInfo.OrderOperateStatus.WaitDelivery;

                            order.PaymentType = OrderInfo.PaymentTypes.Online;
                            order.PaymentTypeName = PAY_BY_INTEGRAL_PAYMENT_ID;
                            order.PayDate = DateTime.Now;
                            if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                            {
                                order.OrderStatus = OrderInfo.OrderOperateStatus.WaitVerification;//虚拟订单付款后，则为待消费
                            }
                            DbFactory.Default.Update(order);

                            //发布付款成功消息
                            //MessageQueue.PublishTopic(CommonConst.MESSAGEQUEUE_PAYSUCCESSED, order.Id);
                            if (OnOrderPaySuccessed != null)
                                OnOrderPaySuccessed(order.Id);

                            SendAppMessage(order);//支付成功后推送APP消息
                        }
                    }

                    var orderItems = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId.ExIn(Ids)).ToList();
                    foreach (var order in orders)
                    {
                        UpdateProductVisti(orderItems.Where(p => p.OrderId == order.Id));
                        if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                        {
                            var orderItemInfo = orderItems.Where(p => p.OrderId == order.Id).FirstOrDefault();//虚拟订单项只有一个
                            UpdateOrderItemEffectiveDateByIds(orderItems.Where(p => p.OrderId == order.Id).Select(a => a.Id).ToList(), order.PayDate.Value);
                            if (orderItemInfo != null)
                            {
                                AddOrderVerificationCodeInfo(orderItemInfo.Quantity, orderItemInfo.OrderId, orderItemInfo.Id);
                                SendMessageOnVirtualOrderPay(order, orderItemInfo.ProductId);
                            }
                        }
                    }
                    if (orders != null && orders.Count > 0)
                        ServiceProvider.Instance<ILimitTimeBuyService>.Create.IncreaseSaleCount(orders.Select(a => a.Id).ToList());//这里应该传入重新过滤后的订单
                });
        }

        public void CancelOrders(IEnumerable<long> Ids, long userId)
        {
            if (Ids.Count() > 0)
            {
                DbFactory.Default.Del<OrderItemInfo>().Where(p => p.OrderId.ExIn(Ids)).Succeed();
                DbFactory.Default.Del<OrderInfo>().Where(p => p.Id.ExIn(Ids)).Succeed();
            }
        }

        //TODO LRL 2015/08/06 获取子订单对象
        public OrderItemInfo GetOrderItem(long orderItemId)
        {
            var orderitem = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.Id == orderItemId).FirstOrDefault();
            if (null == orderitem)
            {
                var flag = SiteSettingApplication.SiteSettings.IsOpenHistoryOrder;
                if (flag)
                {
                    orderitem = DbFactory.MongoDB.AsQueryable<OrderItemInfo>().FirstOrDefault(p => p.Id == orderItemId);
                }
            }
            return orderitem;
        }


        public OrderDayStatistics GetOrderDayStatistics(long shop, DateTime begin, DateTime end)
        {
            var result = new OrderDayStatistics();
            var payOrders = DbFactory.Default.Get<OrderInfo>().Where(a => a.PayDate >= begin && a.PayDate < end);
            var orders = DbFactory.Default.Get<OrderInfo>().Where(a => a.OrderDate >= begin && a.OrderDate < end);
            if (shop > 0)
            {
                payOrders.Where(p => p.ShopId == shop);
                orders.Where(p => p.ShopId == shop);
            }
            result.OrdersNum = orders.Count();
            result.PayOrdersNum = payOrders.Count();
            result.SaleAmount = payOrders.Sum<decimal>(p => p.ProductTotalAmount + p.Freight + p.Tax - p.DiscountAmount);
            return result;
        }

        /// <summary>
        /// 商家给订单备注
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="remark"></param>
        /// <param name="shopId">店铺ID</param>
        public void UpdateSellerRemark(long orderId, string remark, int flag)
        {
            DbFactory.Default.Set<OrderInfo>()
                .Where(p => p.Id == orderId)
               .Set(p => p.SellerRemark, remark)
               .Set(p => p.SellerRemarkFlag, flag)
               .Succeed();
        }

        /// <summary>
        /// 根据提货码取订单
        /// </summary>
        /// <param name="pickCode"></param>
        /// <returns></returns>
        public OrderInfo GetOrderByPickCode(string pickCode)
        {
            return DbFactory.Default.Get<OrderInfo>().Where(e => e.PickupCode == pickCode).FirstOrDefault();
        }

        #region 创建sku列表
        /// <summary>
        /// 判断数据是否能获取sku列表，否则抛出异常
        /// </summary>
        private void CheckWhenCreateOrder(OrderCreateModel model)
        {
            if (model.CurrentUser.Id <= 0)
                throw new InvalidPropertyException("会员Id无效");
            if (model.SkuIds == null || model.SkuIds.Count() == 0)
                throw new InvalidPropertyException("待提交订单的商品不能为空");
            if (model.Counts == null || model.Counts.Count() == 0)
                throw new InvalidPropertyException("待提交订单的商品数量不能为空");
            if (model.Counts.Count(item => item <= 0) > 0)
                throw new InvalidPropertyException("待提交订单的商品数量必须都大于0");
            if (model.SkuIds.Count() != model.Counts.Count())
                throw new InvalidPropertyException("商品数量不一致");
            bool checkaddress = false;
            foreach (var item in model.OrderShops)
            {
                if (item.DeliveryType != Mall.CommonModel.DeliveryType.SelfTake)
                {
                    checkaddress = true;
                }
            }

            if (!model.IsVirtual && checkaddress && model.ReceiveAddressId <= 0)//虚拟订单无需收货地址
                throw new InvalidPropertyException("收货地址无效");

            var productService = ServiceProvider.Instance<IProductService>.Create;
            var limitTimeService = ServiceProvider.Instance<ILimitTimeBuyService>.Create;

            #region 1、检查拼团活动状态 及 拼团商品限购数检查
            var fgser = ServiceProvider.Instance<IFightGroupService>.Create;
            var isFightOrder = false;
            FightGroupActiveInfo actobj = null;
            if (!model.IsShopbranchOrder && model.ActiveId > 0)
            {//1、检查拼团活动状态 及 是否超过拼团限购数
                actobj = fgser.GetActive(model.ActiveId);
                if (model.GroupId > 0)
                {
                    var canjoin = fgser.CanJoinGroup(model.ActiveId, model.GroupId, model.CurrentUser.Id);
                    if (!canjoin)
                    {
                        throw new InvalidPropertyException("不可参团，可能重复参团或火拼团不在进行中");
                    }
                }

                var hasBuynum = fgser.GetMarketSaleCountForUserId(model.ActiveId, model.CurrentUser.Id);

                if (model.Counts.Any(p => hasBuynum + p > actobj.LimitQuantity))
                {
                    throw new InvalidPropertyException("购买数量错误，每人限购" + actobj.LimitQuantity + "件，您还可以购买：" + (actobj.LimitQuantity - hasBuynum) + "件");
                }
                //拼团订单标记
                isFightOrder = true;
            }
            #endregion

            #region 2、检查商品状态（下架、删除）
            //检查商品状态（下架、删除）
            var skus = productService.GetSKUs(model.SkuIds);
            var skuAndCounts = new Dictionary<SKUInfo, int>();

            for (int i = 0; i < model.SkuIds.Count(); i++)
            {
                var skuId = model.SkuIds.ElementAt(i);
                var sku = skus.FirstOrDefault(p => p.Id == skuId);
                if (sku == null)
                    throw new InvalidPropertyException("未找到" + skuId + "对应的商品");
                if (!skuAndCounts.ContainsKey(sku))
                    skuAndCounts.Add(sku, model.Counts.ElementAt(i));
            }

            var products = productService.GetAllProductByIds(skus.Select(p => p.ProductId));

            if (!model.IsShopbranchOrder)
            {
                //门店不参加限购活动，如果不是门店页面提交的订单，需要判断限购预热阶段是否允许购买
            var iLimitService = ServiceApplication.Create<ILimitTimeBuyService>();
            var flashSaleConfig = iLimitService.GetConfig();
            foreach (var p in products)
            {
                var flashSale = iLimitService.IsFlashSaleDoesNotStarted(p.Id);
                if (flashSale != null)
                {
                    TimeSpan flashSaleTime = DateTime.Parse(flashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
                    if (flashSaleConfig != null)
                    {
                        TimeSpan preheatTime = new TimeSpan(flashSaleConfig.Preheat, 0, 0);  //预热时间是多久
                        if (preheatTime >= flashSaleTime && !flashSaleConfig.IsNormalPurchase)  //预热大于开始
                        {
                            throw new InvalidPropertyException("有商品正在参加限时购活动，预热阶段不允许购买");
                        }
                    }
                }
            }           
            }      

            if (products.Any(p => p.SaleStatus != ProductInfo.ProductSaleStatus.OnSale && p.AuditStatus != ProductInfo.ProductAuditStatus.Audited))
                throw new InvalidPropertyException(products.Count > 1 ? "商品中有下架商品" : "商品已下架");
            if (products.Any(p => p.IsDeleted))
                throw new InvalidPropertyException(products.Count > 1 ? "商品中有删除商品" : "商品已删除");

            List<Entities.ShopBranchSkuInfo> shopBranchSkus = null;
            if (null != model.CartItemIds && model.CartItemIds.Any())
            {
                //model.CartItemIds = products.Select(n => n.Id).ToArray();

                //判断门店库存,从购物车中信息判断是否为门店提交订单
                var cartList = DbFactory.Default.Get<ShoppingCartInfo>().Where(x => x.Id.ExIn(model.CartItemIds)).ToList();

                if (cartList.Any(x => x.ShopBranchId > 0))
                {
                    shopBranchSkus = _iShopBranchService.GetSkusByIds(cartList[0].ShopBranchId, skus.Select(p => p.Id).ToList());

                    if (shopBranchSkus.Any(p => p.Status == ShopBranchSkuStatus.InStock))
                        throw new InvalidPropertyException(shopBranchSkus.Count > 1 ? "门店商品中有下架商品" : "门店商品已下架");
                    if (shopBranchSkus.Count < skus.Count)
                        throw new InvalidPropertyException(shopBranchSkus.Count > 1 ? "商品中有门店删除商品" : "门店商品已删除");

                }
            }
            if (model.IsShopbranchOrder && model.IsVirtual && model.OrderShops != null)
            {
                var product = products.FirstOrDefault(p => p.Id == skus.FirstOrDefault().ProductId);
                var orderShop = model.OrderShops.FirstOrDefault(p => p.ShopId == product.ShopId);
                if (orderShop != null)
                {
                    shopBranchSkus = _iShopBranchService.GetSkusByIds(orderShop.ShopBrandId, skus.Select(p => p.Id).ToList());

                    if (shopBranchSkus.Any(p => p.Status == ShopBranchSkuStatus.InStock))
                        throw new InvalidPropertyException(shopBranchSkus.Count > 1 ? "门店商品中有下架商品" : "门店商品已下架");
                    if (shopBranchSkus.Count < skus.Count)
                        throw new InvalidPropertyException(shopBranchSkus.Count > 1 ? "商品中有门店删除商品" : "门店商品已删除");
                }
                
            }
            #endregion 检查商品状态（下架、删除）

            #region 3、商品库存，限时购限购数检查
            foreach (var sku in skus)
            {
                int buynum = skuAndCounts[sku];
                var product = products.FirstOrDefault(p => p.Id == sku.ProductId);

                if (actobj != null)
                {
                    var activeItem = actobj.ActiveItems.FirstOrDefault(d => d.SkuId == sku.Id);
                    if (activeItem == null)
                    {
                        throw new InvalidPropertyException("未找到" + sku.Id + "对应的商品");
                    }
                    if (activeItem.ActiveStock < buynum)
                    {
                        throw new InvalidPropertyException("商品“" + actobj.ProductName + "”库存不够，仅剩" + activeItem.ActiveStock + "件");
                    }

                    sku.SalePrice = activeItem.ActivePrice;
                    sku.Stock = activeItem.ActiveStock;
                }
                else
                {
                    if (!model.IsShopbranchOrder && (model.CollPids == null || (model.CollPids != null && model.CollPids.Count() <= 0)))//非组合购，判断限时购
                    {
                        var limitProduct = limitTimeService.GetLimitTimeMarketItemByProductId(sku.ProductId);
                        if (limitProduct != null)
                        {
                            model.IslimitBuy = true;
                            model.FlashSaleId = limitProduct.Id;
                            var userByProductCount = limitTimeService.GetMarketSaleCountForUserId(sku.ProductId, model.CurrentUser.Id);
                            if (limitProduct != null && limitProduct.LimitCountOfThePeople < (userByProductCount + buynum))
                            {
                                throw new MallException("您购买数量超过限时购限定的最大数！");
                            }
                        }
                    }

                    //判断门店库存
                    if (shopBranchSkus != null)
                    {
                        if (shopBranchSkus != null && shopBranchSkus.Any(x => x.SkuId == sku.Id && x.Stock < buynum))
                        {
                            throw new MallException("商品“" + product.ProductName + "”库存不够，仅剩" + shopBranchSkus.FirstOrDefault(x => x.SkuId == sku.Id).Stock + "件");
                        }
                    }
                    else
                    {
                        if (sku.Stock < buynum)
                        {
                            throw new MallException("商品“" + product.ProductName + "”库存不够，仅剩" + sku.Stock + "件");
                        }
                    }
                }

                model.ProductList.Add(product);
                model.SKUList.Add(sku);
            }
            #endregion

            #region 4、普通商品限购数检查，阶梯价商品不做检查
            if (!model.IslimitBuy && !isFightOrder)
            {
                //普通商品限购检查，阶梯价商品不做检查
                if (products.Any(p => p.MaxBuyCount > 0))
                {
                    var productIds = products.Where(p => p.MaxBuyCount > 0).Select(p => p.Id);
                    var buyedCountsNoLimitBuy = GetProductBuyCountNotLimitBuy(model.CurrentUser.Id, productIds);//包括拼团活动购买数量，已过滤限时购
                    var buyedCountsfightGroup = fgser.GetMarketSaleCountForProductIdAndUserId(productIds, model.CurrentUser.Id);//拼团活动购买数量 
                                                                                                                                //var buyedCounts = GetProductBuyCount(model.CurrentUser.Id, products.Where(p => p.MaxBuyCount > 0).Select(p => p.Id));
                    var outMaxBuyCountProduct = products.FirstOrDefault(p => !p.IsOpenLadder && p.MaxBuyCount > 0 && p.MaxBuyCount < skus.Where(sku => sku.ProductId == p.Id).Sum(sku => skuAndCounts[sku]) + (buyedCountsNoLimitBuy.ContainsKey(p.Id) ? buyedCountsNoLimitBuy[p.Id] : 0) - (buyedCountsfightGroup.ContainsKey(p.Id) ? buyedCountsfightGroup[p.Id] : 0));
                    if (outMaxBuyCountProduct != null)
                        throw new InvalidPropertyException(string.Format("已超出商品[{0}]的最大购买数", outMaxBuyCountProduct.ProductName));
                }
            }
            #endregion
        }

        /// <summary>
        /// 创建新sku列表对象
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<OrderSkuInfo> GetOrderSkuInfo(OrderCreateModel model)
        {
            int i = -1;
            var products = model.SkuIds.Select(skuId =>
            {
                var skuInfo = model.SKUList.FirstOrDefault(s => s.Id.Equals(skuId));
                var product = model.ProductList.FirstOrDefault(p => p.Id.Equals(skuInfo.ProductId));
                i += 1;
                return new OrderSkuInfo
                {
                    SKU = skuInfo,
                    Product = product,
                    Quantity = model.Counts.ElementAt(i),
                    ColloPid = model.CollPids != null && model.CollPids.Any() ? model.CollPids.ElementAt(i) : 0
                };
            }).ToList();

            return products;
        }
        #endregion

        #region OrderBO 的方法

        /// <summary>
        /// 设置运费
        /// </summary>
        /// <param name="order">订单</param>
        /// <param name="freight">运费</param>
        public void SetFreight(OrderInfo order, decimal freight)
        {
            if (freight < 0)
            {
                throw new MallException("运费不能为负值！");
            }
            order.Freight = freight;
        }

        /// <summary>
        /// 设置订单状态为完成
        /// </summary>
        public void SetStateToConfirm(OrderInfo order)
        {
            if (order == null)
            {
                throw new MallException("处理订单错误，请确认该订单状态正确");
            }
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitReceiving)
            {
                throw new MallException("只有等待收货状态的订单才能进行确认操作");
            }
            if (order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery)
            {
                order.PayDate = DateTime.Now;
            }
            order.OrderStatus = OrderInfo.OrderOperateStatus.Finish;
            order.FinishDate = DateTime.Now;
        }

        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="order">订单ID</param>
        /// <param name="closeReason">理由</param>
        public void CloseOrder(OrderInfo order)
        {
            CheckCloseOrder(order);

            order.OrderStatus = OrderInfo.OrderOperateStatus.Close;
        }
        /// <summary>
        /// 检测订单是否可以被关闭
        /// </summary>
        /// <param name="order"></param>
        public void CheckCloseOrder(OrderInfo order)
        {
            if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay ||
                   order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery && order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery)
            {
                if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
                {
                    var fgser = ServiceProvider.Instance<IFightGroupService>.Create;
                    var fgord = fgser.GetFightGroupOrderStatusByOrderId(order.Id);
                    if (
                        fgord.JoinStatus == FightGroupOrderJoinStatus.Ongoing.GetHashCode() ||
                        fgord.JoinStatus == FightGroupOrderJoinStatus.JoinSuccess.GetHashCode() ||
                        fgord.JoinStatus == FightGroupOrderJoinStatus.BuildSuccess.GetHashCode()
                        )
                    {
                        throw new MallException("拼团订单不可关闭");
                    }
                }
            }
            else
            {
                throw new MallException("只有待付款状态或货到付款待发货状态的订单才能进行取消操作");
            }
        }

        /// <summary>
        /// 检查是否满额免运费
        /// </summary>
        public bool IsFullFreeFreight(ShopInfo shop, decimal OrderPaidAmount)
        {
            bool result = false;
            if (shop != null)
            {
                if (shop.FreeFreight > 0)
                {
                    if (OrderPaidAmount >= shop.FreeFreight)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取真实费用
        /// </summary>
        public decimal GetRealTotalPrice(OrderInfo order, OrderItemInfo item, decimal discountAmount)
        {
            if (item.RealTotalPrice - item.FullDiscount - item.CouponDiscount - discountAmount < 0)
            {
                throw new MallException("优惠金额不能大于商品总金额！");
            }
            if (order.OrderTotalAmount - discountAmount < 0)
            {
                throw new MallException("减价不能导致订单总金额为负值！");
            }

            return item.RealTotalPrice - discountAmount;
        }


        private static object obj = new object();
        /// <summary>
        ///  生成订单号
        /// </summary>
        public long GenerateOrderNumber()
        {
            lock (obj)
            {
                int rand;
                char code;
                string orderId = string.Empty;
                Random random = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
                for (int i = 0; i < 5; i++)
                {
                    rand = random.Next();
                    code = (char)('0' + (char)(rand % 10));
                    orderId += code.ToString();
                }
                return long.Parse(DateTime.Now.ToString("yyyyMMddfff") + orderId);
            }
        }

        private static object objpay = new object();
        /// <summary>
        /// 生成支付订单号
        /// </summary>
        /// <returns></returns>
        public long GetOrderPayId()
        {
            lock (objpay)
            {
                int rand;
                char code;
                string orderId = string.Empty;
                Random random = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
                for (int i = 0; i < 6; i++)
                {
                    rand = random.Next();
                    code = (char)('0' + (char)(rand % 10));
                    orderId += code.ToString();
                }
                return long.Parse(DateTime.Now.ToString("yyMMddmmHHss") + orderId);
            }
        }


        /// <summary>
        /// 保存发票内容
        /// </summary>
        public void AddInvoiceTitle(string titleName, string InvoiceCode, long userid)
        {
            if (!DbFactory.Default.Get<InvoiceTitleInfo>().Where(p => p.Name == titleName).Exist())
            {
                DbFactory.Default.Add(new InvoiceTitleInfo() { Name = titleName, Code = InvoiceCode, UserId = userid });
            }
        }

        #region 创建额外订单信息
        /// <summary>
        /// 从订单对象中拆分出额外的订单信息
        /// </summary>
        public OrderCreateAdditional CreateAdditional(IEnumerable<OrderSkuInfo> orderSkuInfos, OrderCreateModel model)
        {
            var baseOrderCoupons = GetOrdersCoupons(model.CurrentUser.Id, model.CouponIdsStr);//获取所有订单使用的优惠券列表
            var orderIntegral = GetIntegralDiscountAmount(model.Integral, model.CurrentUser.Id);//获取所有订单能使用的积分金额
            var orderCapital = GetCapitalAmount(model.Capital, model.CurrentUser.Id);//获取所有订单能使用的预付款金额

            bool checkaddress = false;
            foreach (var item in model.OrderShops)
            {
                if (item.DeliveryType != Mall.CommonModel.DeliveryType.SelfTake)
                {
                    checkaddress = true;
                }
            }
            Entities.ShippingAddressInfo address = null;
            if (!model.IsVirtual && checkaddress)
            {
                address = ServiceProvider.Instance<IShippingAddressService>.Create.GetUserShippingAddress(model.ReceiveAddressId);//获取用户的收货地址
                if (address == null)
                {
                    throw new MallException("错误的收货地址！");
                }
            }
            else
            {
                address = new Entities.ShippingAddressInfo();
            }
            OrderCreateAdditional additional = new OrderCreateAdditional();
            additional.BaseCoupons = baseOrderCoupons;
            additional.Address = address;
            additional.IntegralTotal = orderIntegral;
            additional.CapitalTotal = orderCapital;
            additional.CreateDate = DateTime.Now;
            return additional;
        }

        /// <summary>
        /// 获取所有订单使用的优惠券列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="couponIdsStr"></param>
        /// <returns></returns>
        public IEnumerable<BaseAdditionalCoupon> GetOrdersCoupons(long userId, IEnumerable<string[]> couponIdsStr)
        {
            var couponService = ServiceProvider.Instance<ICouponService>.Create;
            var shopBonusService = ServiceProvider.Instance<IShopBonusService>.Create;
            if (couponIdsStr == null || couponIdsStr.Count() <= 0)
            {
                return null;
            }
            List<BaseAdditionalCoupon> list = new List<BaseAdditionalCoupon>();
            foreach (string[] str in couponIdsStr)
            {
                BaseAdditionalCoupon item;
                if (int.Parse(str[1]) == 0)
                {
                    var obj = couponService.GetOrderCoupons(userId, new long[] { long.Parse(str[0]) }).FirstOrDefault();
                    if (obj == null)
                        throw new MallException("优惠券不存在或优惠券已使用!");
                    item = new BaseAdditionalCoupon();
                    item.Type = 0;
                    item.Coupon = obj;
                    item.ShopId = obj.ShopId;
                }
                else if (int.Parse(str[1]) == 1)
                {
                    var obj = shopBonusService.GetDetailById(userId, long.Parse(str[0]));
                    var grant = shopBonusService.GetGrant(obj.BonusGrantId);
                    var bonus = shopBonusService.GetShopBonus(grant.ShopBonusId);
                    item = new BaseAdditionalCoupon();
                    item.Type = 1;
                    item.Coupon = obj;
                    item.ShopId = bonus.ShopId;
                }
                else
                {
                    item = new BaseAdditionalCoupon();
                    item.Type = 99;
                }

                list.Add(item);
            }

            return list;
        }
        #endregion

        #region  创建订单列表对象
        /// <summary>
        /// 创建订单列表对象
        /// </summary>
        public List<OrderInfo> GetOrderInfos(IEnumerable<OrderSkuInfo> orderSkuInfos, OrderCreateModel model, OrderCreateAdditional additional)
        {
            //把购买的商品信息按店铺分组
            var cartGroupInfo = orderSkuInfos.GroupBy(item => item.Product.ShopId).ToList();

            List<OrderInfo> infos = new List<OrderInfo>();
            int i = 0;
            foreach (var item in cartGroupInfo)
            {
                string remark = string.Empty;
                if (model.OrderRemarks.LongCount() >= i + 1)
                {//如果有输入备注，则补充。正常情况下，备注条数与订单数一致
                    remark = model.OrderRemarks.ElementAt(i);
                }
                var order = CreateOrderInfo(item, model, additional, remark);
                order.DeliveryType = CommonModel.DeliveryType.Express;
                if (model.OrderShops != null)
                {//过滤null,商城APP订单提交时，暂未处理门店逻辑
                    var orderShop = model.OrderShops.FirstOrDefault(p => p.ShopId == order.ShopId);
                    if (orderShop != null)
                    {
                        order.ShopBranchId = orderShop.ShopBrandId;
                        order.DeliveryType = orderShop.DeliveryType;
                    }
                }
                if (order.DeliveryType == DeliveryType.SelfTake)
                {
                    order.Freight = 0;
                    if (order.RegionId <= 0 && order.TopRegionId <= 0 && order.ShopBranchId > 0)
                    {
                        var shopBranchInfo = _iShopBranchService.GetShopBranchById(order.ShopBranchId);
                        if (shopBranchInfo != null)
                        {
                            var cityRegion = _iRegionService.GetRegion(shopBranchInfo.AddressId, Region.RegionLevel.City);
                            order.RegionId = cityRegion == null ? shopBranchInfo.AddressId : cityRegion.Id;
                            order.TopRegionId = string.IsNullOrEmpty(shopBranchInfo.AddressPath) ? 0 : int.Parse(shopBranchInfo.AddressPath.Split(',')[0]);
                        }
                    }
                }

                else if (order.OrderType != OrderInfo.OrderTypes.Virtual && order.DeliveryType == DeliveryType.ShopStore && order.ShopBranchId > 0)
                {
                    var shopBranchInfo = _iShopBranchService.GetShopBranchById(order.ShopBranchId);
                    if (shopBranchInfo != null)
                    {
                        //if (shopBranchInfo.FreeMailFee > 0 && (order.ProductTotalAmount - order.DiscountAmount) >= shopBranchInfo.FreeMailFee)
                        if ((order.ProductTotalAmount - order.DiscountAmount - order.FullDiscount) >= shopBranchInfo.FreeMailFee && shopBranchInfo.IsFreeMail)
                            order.Freight = 0;//门店免配送费
                        else
                            order.Freight = shopBranchInfo.DeliveFee;
                    }
                }

                if(order.OrderType == OrderInfo.OrderTypes.Virtual)
                {
                    if (order.RegionId <= 0 && order.TopRegionId <= 0 && order.ShopBranchId > 0)
                    {
                        var shopBranchInfo = _iShopBranchService.GetShopBranchById(order.ShopBranchId);
                        if (shopBranchInfo != null)
                        {
                            var cityRegion = _iRegionService.GetRegion(shopBranchInfo.AddressId, Region.RegionLevel.City);
                            order.RegionId = cityRegion == null ? shopBranchInfo.AddressId : cityRegion.Id;
                            order.TopRegionId = string.IsNullOrEmpty(shopBranchInfo.AddressPath) ? 0 : int.Parse(shopBranchInfo.AddressPath.Split(',')[0]);
                        }
                    }

                    if (order.RegionId <= 0 && order.TopRegionId <= 0 && order.ShopId > 0 && order.ShopBranchId <= 0)
                    {
                        var verificationShipper = ShopShippersApplication.GetDefaultVerificationShipper(order.ShopId);
                        if (verificationShipper != null)
                        {
                            var cityRegion = _iRegionService.GetRegion(verificationShipper.RegionId, Region.RegionLevel.City);
                            order.RegionId = cityRegion == null ? verificationShipper.RegionId : cityRegion.Id;

                            var provinceRegion = _iRegionService.GetRegion(verificationShipper.RegionId, Region.RegionLevel.Province);
                            order.TopRegionId = provinceRegion == null ? verificationShipper.RegionId : provinceRegion.Id;
                        }
                    }
                    order.Freight = 0;
                }

                i++;
                infos.Add(order);
            }

            //计算商品总金额，并且减去优惠券的金额，减满额减价格
            decimal productsTotals = infos.Sum(a => a.ProductTotal); //商品价-优惠券-满额减
            decimal IntegralTotal = additional.IntegralTotal;
            //重算一下积分可抵
            var IntegralExchange = ServiceProvider.Instance<IMemberIntegralService>.Create.GetIntegralChangeRule();
            if (IntegralExchange != null && model.Integral > 0)
            {
                var maxint = Math.Ceiling(productsTotals * IntegralExchange.IntegralPerMoney);
                var config = SiteSettingApplication.SiteSettings;
                var userid = model.CurrentUser.Id;
                var usermaxint = MemberIntegralApplication.GetAvailableIntegral(userid);
                if (maxint > usermaxint)
                {
                    maxint = usermaxint;
                }
                var maxintdis = decimal.Round(maxint / (decimal)IntegralExchange.IntegralPerMoney, 2, MidpointRounding.AwayFromZero);
                var canmaxintdis = decimal.Round(productsTotals * (config.IntegralDeductibleRate / (decimal)100), 2, MidpointRounding.AwayFromZero);
                if (maxintdis > canmaxintdis)
                {
                    IntegralTotal = canmaxintdis;
                }
            }
            if (IntegralTotal > productsTotals)
            {
                throw new MallException("积分抵扣金额不能超过商品总金额！");
            }
            if (additional.IntegralTotal > IntegralTotal)
            {
                additional.IntegralTotal = IntegralTotal;
            }           

            //处理积分
            ProcessIntegralDiscount(infos, additional, productsTotals);

            foreach (var item in infos)
            {
                #region TDO:ZYF 处理发票信息
                var orderShop = model.OrderShops.FirstOrDefault(p => p.ShopId == item.ShopId);
                if (orderShop != null)
                {
                    var orderInvoice = orderShop.PostOrderInvoice;
                    if (orderInvoice != null)
                    {
                        if (orderInvoice.InvoiceType == InvoiceType.None)
                            item.Tax = 0;
                        else
                        {
                            decimal rate = 0;
                            var invoiceConfig = ShopApplication.GetShopInvoiceConfig(item.ShopId);
                            if (invoiceConfig != null && invoiceConfig.IsInvoice)
                            {
                                switch (orderInvoice.InvoiceType)
                                {
                                    case InvoiceType.OrdinaryInvoices:
                                        if (invoiceConfig.IsPlainInvoice)
                                            rate = invoiceConfig.PlainInvoiceRate;
                                        break;
                                    case InvoiceType.ElectronicInvoice:
                                        if (invoiceConfig.IsElectronicInvoice)
                                            rate = invoiceConfig.PlainInvoiceRate;
                                        break;
                                    case InvoiceType.VATInvoice:
                                        if (invoiceConfig.IsVatInvoice)
                                            rate = invoiceConfig.VatInvoiceRate;
                                        break;
                                }
                            }
                            if (rate > 0)
                            {
                                item.Tax = decimal.Round(item.OrderTotalAmountByTax * (rate / 100), 2, MidpointRounding.AwayFromZero);
                            }
                            OrderInvoiceInfo invoice = new OrderInvoiceInfo()
                            {
                                OrderId = item.Id,
                                InvoiceType = orderInvoice.InvoiceType,
                                InvoiceTitle = orderInvoice.InvoiceTitle,
                                InvoiceCode = orderInvoice.InvoiceCode,
                                InvoiceContext = orderInvoice.InvoiceContext,
                                RegisterAddress = orderInvoice.RegisterAddress,
                                RegisterPhone = orderInvoice.RegisterPhone,
                                BankName = orderInvoice.BankName,
                                BankNo = orderInvoice.BankNo,
                                RealName = orderInvoice.RealName,
                                CellPhone = orderInvoice.CellPhone,
                                Email = orderInvoice.Email,
                                RegionID = orderInvoice.RegionID,
                                Address = orderInvoice.Address
                            };
                            item.OrderInvoice = invoice;
                        }
                    }
                }
                #endregion
            }
            //处理预付款
            ProcessCapitalAmount(infos, additional, productsTotals);

            var payid = infos.Count == 1 ? infos.First().Id : GetOrderPayId();
            foreach (var item in infos)
            {
                item.TotalAmount = item.OrderTotalAmount;
                item.ActualPayAmount = item.CapitalAmount;
                if (item.TotalAmount - item.CapitalAmount <= 0)
                {
                    item.PaymentTypeGateway = string.Empty;
                    item.PaymentType = OrderInfo.PaymentTypes.Online;
                    item.PaymentTypeName = PAY_BY_CAPITAL_PAYMENT_ID;
                    item.GatewayOrderId = item.Id.ToString();
                    item.PayDate = DateTime.Now;


                    item.OrderPay = new OrderPayInfo
                    {
                        OrderId = item.Id,
                        PayId = payid,
                        PayState = true,
                        PayTime = DateTime.Now
                    };
                }
            }

            return infos;
        }
        /// <summary>
        /// 创建单个订单对象
        /// </summary>
        /// <returns></returns>
        private OrderInfo CreateOrderInfo(IGrouping<long, OrderSkuInfo> groupItem, OrderCreateModel model, OrderCreateAdditional additional, string remark)
        {
            int cityId = 0;
            if (additional.Address != null)
            {
                //cityId = ServiceProvider.Instance<IRegionService>.Create.GetRegion(additional.Address.RegionId, Region.RegionLevel.City).Id;
                cityId = additional.Address.RegionId;
            }
            var user = model.CurrentUser;
            var shop = ServiceProvider.Instance<IShopService>.Create.GetShop(groupItem.Key);
            if (shop.ShopStatus == ShopInfo.ShopAuditStatus.Freeze || shop.ShopStatus == ShopInfo.ShopAuditStatus.HasExpired)
            {
                throw new MallException(shop.ShopName + "已冻结或者过期，请移除此店铺的商品！");
            }

            IEnumerable<long> productIds = groupItem.Select(item => item.Product.Id);
            IEnumerable<int> productCounts = groupItem.Select(item => (int)item.Quantity);

            //总的商品价格计算每一个订单项的价格
            //  decimal productTotalAmount = groupItem.Sum(item => GetSalePrice(item.Product.Id, item.SKU, item.ColloPid, productIds.Count()) * item.Quantity);

            //货到付款相关
            string paymentTypeName = "";
            OrderInfo.OrderOperateStatus orderstatus;
            OrderInfo.PaymentTypes paymentType;
            if (model.IsCashOnDelivery && groupItem.First().Product.ShopId == 1)
            {
                paymentTypeName = "货到付款";
                orderstatus = OrderInfo.OrderOperateStatus.WaitDelivery;

                paymentType = OrderInfo.PaymentTypes.CashOnDelivery;
            }
            else
            {
                orderstatus = OrderInfo.OrderOperateStatus.WaitPay;
                paymentType = OrderInfo.PaymentTypes.None;
            }

            //TDO:ZYF3.2 取消原先订单发票信息
            //var tempInvoiceType = model.Invoice;
            //var tempInvoiceTitle = model.InvoiceTitle;
            //var tempInvoiceCode = model.InvoiceCode;
            //var tempInvoiceContext = model.InvoiceContext;
            //var shopinvoice = ServiceProvider.Instance<IShopService>.Create.GetShopInvoiceConfig(groupItem.Key);
            //if (!shopinvoice.IsInvoice)//商家是否提供发票
            //{
            //    tempInvoiceType = InvoiceType.None;
            //    tempInvoiceTitle = "";
            //    tempInvoiceCode = "";
            //    tempInvoiceContext = "";
            //}

            var order = new OrderInfo()
            {
                Id = GenerateOrderNumber(),
                ShopId = groupItem.Key,
                ShopName = shop.ShopName,
                UserId = user.Id,
                UserName = user.UserName,
                OrderDate = additional.CreateDate,
                RegionId = additional.Address.RegionId,
                ShipTo = additional.Address.ShipTo == null ? "" : additional.Address.ShipTo,
                Address = GetAddressDetail(additional.Address.Address, additional.Address.AddressDetail),//additional.Address.Address == null ? "" : additional.Address.Address,
                RegionFullName = additional.Address.RegionFullName == null ? "" : additional.Address.RegionFullName,
                CellPhone = additional.Address.Phone == null ? "" : additional.Address.Phone,
                TopRegionId = string.IsNullOrEmpty(additional.Address.RegionIdPath) ? 0 : int.Parse(additional.Address.RegionIdPath.Split(',')[0]),
                ReceiveLatitude = model.ReceiveLatitude,
                ReceiveLongitude = model.ReceiveLongitude,
                ReceiveAddressId = additional.Address.Id,
                OrderStatus = orderstatus,
                //Freight = additional.Address.Id == 0 ? 0 : ServiceProvider.Instance<IProductService>.Create.GetFreight(productIds, productCounts, cityId),
                IsPrinted = false,
                OrderRemarks = remark,
                // ProductTotalAmount = productTotalAmount,商品总价格
                RefundTotalAmount = 0,
                CommisTotalAmount = 0,
                RefundCommisAmount = 0,
                Platform = model.PlatformType,
                //InvoiceType = tempInvoiceType,
                //InvoiceTitle = tempInvoiceTitle,
                //InvoiceCode = tempInvoiceCode,
                //InvoiceContext = tempInvoiceContext,
                //修改优惠券优惠金额不在此计算
                //DiscountAmount = GetShopCouponDiscount(additional.BaseCoupons, groupItem.Key),
                PaymentTypeName = paymentTypeName,
                PaymentType = paymentType,
                LastModifyTime = DateTime.Now
            };

            if (model.CollPids != null && model.CollPids.Count() > 1)
            {
                order.OrderType = OrderInfo.OrderTypes.Collocation;
            }
            if (model.ActiveId > 0)
            {
                order.OrderType = OrderInfo.OrderTypes.FightGroup;  //拼团订单
            }
            if (model.IslimitBuy)
            {
                order.OrderType = OrderInfo.OrderTypes.LimitBuy;
            }
            if (model.IsVirtual)
            {
                order.OrderType = OrderInfo.OrderTypes.Virtual;
            }
            if (order.OrderItemInfo == null)
            {
                order.OrderItemInfo = new List<OrderItemInfo>();
            }
            var productCount = productIds.Count();
            var groupCartByProduct = groupItem.GroupBy(i => i.Product.Id).ToList();//按照商品分组
            foreach (var item in groupItem)
            {
                if (item.Product.SaleStatus != ProductInfo.ProductSaleStatus.OnSale || item.Product.AuditStatus != ProductInfo.ProductAuditStatus.Audited)
                {
                    throw new MallException("订单中有失效的商品，请返回重新提交！");
                }
                var price = item.SKU.SalePrice;

                #region 阶梯价商品--张宇枫
                //限时抢购和拼团不参与阶梯价
                if (order.OrderType != OrderInfo.OrderTypes.FightGroup &&
                    order.OrderType != OrderInfo.OrderTypes.LimitBuy)
                {
                    if (item.Product.IsOpenLadder)
                    {
                        //获取商品总数量，不分规格
                        var quantity =
                            groupCartByProduct.Where(i => i.Key == item.Product.Id)
                                .ToList()
                                .Sum(cartitem => cartitem.Sum(i => i.Quantity));

                        var ladderPrces = _iProductLadderPriceService.GetLadderPricesByProductIds(item.Product.Id);
                        var minTradeCount = 0;
                        if (ladderPrces.Any())
                        {
                            price =
                                ladderPrces.Find(
                                    i => (quantity <= i.MinBath) || (quantity >= i.MinBath && quantity <= i.MaxBath))
                                    .Price;
                            minTradeCount = ladderPrces.Min(t => t.MinBath);
                        }
                        if (quantity < minTradeCount)
                        {
                            throw new MallException("【" + item.Product.ProductName + "】订购数量未达最小批量值！");
                        }
                    }
                }

                #endregion
                var orderItem = new OrderItemInfo();
                orderItem.OrderId = order.Id;
                orderItem.ShopId = order.ShopId;
                orderItem.ProductId = item.Product.Id;
                orderItem.SkuId = item.SKU.Id;
                orderItem.Quantity = item.Quantity;
                orderItem.SKU = item.SKU.Sku;
                orderItem.ReturnQuantity = 0;
                orderItem.CostPrice = item.SKU.CostPrice;
                orderItem.SalePrice = price;

                if (shop.IsSelf)//官方自营店无平台佣金
                {
                    orderItem.CommisRate = 0;
                }
                else
                {

                    decimal CommisRate = DbFactory.Default.Get<BusinessCategoryInfo>().Where(b => b.CategoryId == item.Product.CategoryId && b.ShopId == item.Product.ShopId).Select(a => a.CommisRate).FirstOrDefault<decimal>();
                    orderItem.CommisRate = CommisRate / 100;
                }
                //不是限时购和拼团计算会员价

                if (order.OrderType != OrderInfo.OrderTypes.FightGroup && order.OrderType != OrderInfo.OrderTypes.LimitBuy)
                {
                    if (shop.IsSelf) //如果是自营店计算会员折扣
                    {
                        orderItem.SalePrice = user.MemberDiscount * orderItem.SalePrice;
                    }
                }
                if (!model.IsShopbranchOrder)
                {
                    //组合购或者限时购的价格
                    orderItem.SalePrice = GetSalePrice(item.SKU.Id, orderItem.SalePrice, item.ColloPid, productCount, order.OrderType == OrderInfo.OrderTypes.LimitBuy);
                }
                orderItem.SalePrice = decimal.Round(orderItem.SalePrice, 2, MidpointRounding.AwayFromZero);
                orderItem.IsLimitBuy = model.IslimitBuy;
                orderItem.DiscountAmount = 0;
                var itemTotal = (orderItem.SalePrice * item.Quantity);
                orderItem.RealTotalPrice = itemTotal;
                orderItem.RefundPrice = 0;

                orderItem.Color = item.SKU.Color;
                orderItem.Size = item.SKU.Size;
                orderItem.Version = item.SKU.Version;
                orderItem.ProductName = item.Product.ProductName;
                orderItem.ThumbnailsUrl = item.Product.RelativePath;
                if(orderItem.IsLimitBuy)
                {
                    orderItem.FlashSaleId = model.FlashSaleId;
                }
                order.OrderItemInfo.Add(orderItem);
            }

            //订单的商品价格只包含会员价，限时购价，拼团价等不包含其他优惠
            order.ProductTotalAmount = order.OrderItemInfo.Sum(a => a.RealTotalPrice);

            //处理平摊满额减
            SetOrderFullDiscount(order);

            if (additional.BaseCoupons != null)
            {
                var coupon = additional.BaseCoupons.Where(a => a.ShopId == order.ShopId).FirstOrDefault();

                if (coupon != null)//存在使用优惠券的情况
                {
                    decimal couponUseAmount = 0;
                    decimal couponDiscount = 0;
                    if (coupon.Type == 0)//优惠券
                    {
                        var couponObj = (coupon.Coupon as CouponRecordInfo);
                        var cou = DbFactory.Default.Get<CouponInfo>(p => p.Id == couponObj.CouponId).FirstOrDefault();
                        order.CouponId = couponObj.CouponId;
                        couponUseAmount = cou.OrderAmount;
                        couponDiscount = cou.Price;
                    }
                    else if (coupon.Type == 1)//代金红包
                    {
                        var couponObj = (coupon.Coupon as ShopBonusReceiveInfo);
                        var service = Instance<IShopBonusService>.Create;
                        var grant = service.GetGrant(couponObj.BonusGrantId);
                        var bonus = service.GetShopBonus(grant.ShopBonusId);

                        couponUseAmount = bonus.UsrStatePrice;
                        couponDiscount = couponObj.Price;
                    }
                    var selcoupon = DbFactory.Default.Get<CouponInfo>().Where(p => p.Id == order.CouponId).FirstOrDefault();
                    if (selcoupon != null && selcoupon.UseArea == 1)
                    {
                        var couponProducts = DbFactory.Default.Get<CouponProductInfo>().Where(p => p.CouponId == selcoupon.Id).Select(p => p.ProductId).ToList<long>();
                        decimal coupontotal = 0;
                        foreach (var p in order.OrderItemInfo)
                        {
                            if (couponProducts.Contains(p.ProductId))
                                coupontotal += p.RealTotalPrice - p.FullDiscount;
                        }
                        if (couponDiscount > coupontotal)
                        {
                            order.DiscountAmount = coupontotal;
                        }
                        else
                        {
                            order.DiscountAmount = couponDiscount;
                        }
                    }
                    else
                    {
                        if (order.ProductTotalAmount < couponUseAmount)
                        {
                            throw new MallException("优惠券不满足使用条件");
                        }
                        if (couponDiscount >= order.ProductTotalAmount - order.FullDiscount) //优惠券面值大于商品总金额,优惠金额为订单总金额
                        {
                            order.DiscountAmount = order.ProductTotalAmount - order.FullDiscount;
                        }
                        else
                        {
                            order.DiscountAmount = couponDiscount;
                        }
                    }
                    //订单处理平摊优惠券
                    SetActualItemPrice(order);
                }
            }
            if (order.OrderType != OrderInfo.OrderTypes.Virtual)
            {
                #region 指定地区包邮
                List<long> excludeIds = new List<long>();//排除掉包邮的商品
                var templateIds = groupItem.Select(item => item.Product.FreightTemplateId).Distinct().ToList();//商品模板ID集合
                templateIds.ForEach(p =>
                {
                    var ids = groupItem.Where(a => a.Product.FreightTemplateId == p).Select(a => a.Product.Id);//属于当前模板的商品ID集合
                    var orderItems = order.OrderItemInfo.Where(b => ids.Contains(b.ProductId));//该商品集合的订单项数据
                    bool isFree = false;

                    var freeRegions = ServiceProvider.Instance<IFreightTemplateService>.Create.GetShippingFreeRegions(p);
                    freeRegions.ForEach(c =>
                    {
                        c.RegionSubList = ServiceProvider.Instance<IRegionService>.Create.GetSubsNew(c.RegionId, true).Select(a => a.Id).ToList();
                    });
                    var regions = freeRegions.Where(d => d.RegionSubList.Contains(cityId));//根据模板设置的包邮地区过滤出当前配送地址所在地址
                    if (regions != null && regions.Count() > 0)
                    {
                        var groupIds = regions.Select(e => e.GroupId).ToList();
                        var freeGroups = ServiceProvider.Instance<IFreightTemplateService>.Create.GetShippingFreeGroupInfos(p, groupIds);

                        //只要有一个符合包邮条件，则退出
                        long count = orderItems.Sum(item => item.Quantity);//总数量
                        decimal amount = orderItems.Sum(item => item.RealTotalPrice);//总金额
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
                IEnumerable<long> pIds = groupItem.Where(p => !excludeIds.Contains(p.Product.Id)).Select(item => item.Product.Id);
                IEnumerable<int> pCounts = groupItem.Where(p => !excludeIds.Contains(p.Product.Id)).Select(item => (int)item.Quantity);
                //计算运费
                if (pIds != null && pIds.Count() > 0 && pCounts != null && pCounts.Count() > 0)
                {
                    order.Freight = additional.Address.Id == 0 ? 0 : ServiceProvider.Instance<IProductService>.Create.GetFreight(pIds, pCounts, cityId);
                }
                #endregion

                //满额免运费判断，不为货到付款时才执行
                if (IsFullFreeFreight(shop, order.ProductTotalAmount - order.FullDiscount - order.DiscountAmount))
                {
                    order.Freight = 0;
                }
            }
            return order;
        }

        /// <summary>
        /// 获取详细地址（大厦+门牌号）
        /// </summary>
        /// <param name="address"></param>
        /// <param name="detail"></param>
        /// <returns></returns>
        private static string GetAddressDetail(string address, string detail)
        {
            string addressDetail = "";
            if (!string.IsNullOrEmpty(address))
                addressDetail = address;

            if (!string.IsNullOrEmpty(detail))
                addressDetail += " " + detail;

            return addressDetail;
        }
        #endregion


        /// <summary>
        /// 订单提交处理满额减
        /// </summary>
        /// <param name="order"></param>
        private static void SetOrderFullDiscount(OrderInfo order)
        {
            List<OrderItemInfo> fulldiscountP = new List<OrderItemInfo>();
            foreach (var p in order.OrderItemInfo)
            {
                var canJoin = true;
                //限时购不参与满额减（bug需求34735）
                if (order.OrderType == OrderInfo.OrderTypes.LimitBuy)
                {
                    var ltmbuy = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetLimitTimeMarketItemByProductId(p.ProductId);
                    if (ltmbuy != null)
                    {
                        canJoin = false;
                    }
                }
                //拼团不参与满额减（bug需求34735）
                if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
                {
                    var activeInfo = ServiceProvider.Instance<IFightGroupService>.Create.GetActiveByProId(p.ProductId);
                    if (activeInfo != null && activeInfo.ActiveStatus > FightGroupActiveStatus.Ending)
                    {
                        canJoin = false;
                    }
                }
                if (canJoin)
                    fulldiscountP.Add(p);
            }
            IEnumerable<long> productIds = null;
            fulldiscountP = fulldiscountP.OrderBy(d => d.SkuId).ToList();
            if (fulldiscountP.Count() > 0)
                productIds = fulldiscountP.Select(A => A.ProductId).Distinct();
            List<ActiveInfo> actives = null;
            if (productIds != null)
                actives = Instance<IFullDiscountService>.Create.GetOngoingActiveByProductIds(productIds, order.ShopId);
            decimal orderFullDiscount = 0;
            if (actives != null)
            {
                foreach (var active in actives)
                {
                    var pids = FullDiscountApplication.GetActiveProductIds(active.Id).Select(p => p.ProductId).ToList();
                    var items = fulldiscountP;
                    if (!active.IsAllProduct)
                    {
                        items = items.Where(a => pids.Contains(a.ProductId)).ToList();
                    }
                    var realTotal = items.Sum(a => a.RealTotalPrice);  //满足满额减的总商品金额

                    var rule = FullDiscountApplication.GetActiveRules(active.Id).Where(a => a.Quota <= realTotal).OrderByDescending(a => a.Quota).FirstOrDefault();
                    decimal fullDiscount = 0;
                    if (rule != null)//找不到就是不满足金额
                    {
                        fullDiscount = rule.Discount;
                        var infos = items.ToArray();
                        var count = items.Count();
                        decimal itemFullDiscount = 0;
                        //平分优惠金额到各个订单项
                        for (var i = 0; i < count; i++)
                        {
                            var _order = infos[i];
                            if (i < count - 1)
                            {
                                infos[i].FullDiscount = decimal.Parse((fullDiscount * (infos[i].RealTotalPrice) / realTotal).ToString("F2"));
                                itemFullDiscount += infos[i].FullDiscount;
                            }
                            else
                            {
                                infos[i].FullDiscount = fullDiscount - itemFullDiscount;
                            }
                        }
                        orderFullDiscount += fullDiscount; //订单总优惠金额 
                    }
                }
            }
            order.FullDiscount = orderFullDiscount;
        }





        /// <summary>
        /// 处理积分
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="additional"></param>
        /// <param name="productsTotals"></param>
        /// <returns></returns>
        public void ProcessIntegralDiscount(List<OrderInfo> infos, OrderCreateAdditional additional, decimal productsTotals)
        {
            var t = infos.Count;
            decimal integralDiscount = 0;
            for (var i = 0; i < infos.Count; i++)
            {
                var _order = infos[i];
                if (i < t - 1)
                {
                    infos[i].IntegralDiscount = Math.Round(additional.IntegralTotal * (infos[i].ProductTotalAmount - infos[i].DiscountAmount - infos[i].FullDiscount) / productsTotals, 2, MidpointRounding.AwayFromZero);//积分抵扣应该也要减去满额减
                    integralDiscount += infos[i].IntegralDiscount;
                }
                else
                {
                    infos[i].IntegralDiscount = additional.IntegralTotal - integralDiscount;
                }
            }
        }
        /// <summary>
        /// 处理预付款
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="additional"></param>
        /// <param name="productsTotals"></param>
        public void ProcessCapitalAmount(List<OrderInfo> infos, OrderCreateAdditional additional, decimal productsTotals)
        {
            if (additional.CapitalTotal <= 0) return;
            var t = infos.Count;
            decimal capitalAmount = 0;
            var totalFreight = infos.Sum(n => n.Freight);
            var totalTax = infos.Sum(n => n.Tax);
            var totalCapital = additional.CapitalTotal - totalFreight - totalTax;
            //if (totalCapital <= 0) return;
            for (var i = 0; i < t; i++)
            {
                var _order = infos[i];
                if (i < t - 1)
                {
                    //商品总金额不为零时，才分摊计算
                    if (productsTotals > 0)
                    {
                        _order.CapitalAmount = Math.Round(totalCapital * (_order.ProductTotalAmount - _order.DiscountAmount - _order.FullDiscount) / productsTotals, 2, MidpointRounding.AwayFromZero) + _order.Freight + _order.Tax;//积分抵扣应该也要减去满额减
                    }
                    else
                    {//否则只计算运费
                        _order.CapitalAmount = _order.Freight;
                    }
                    capitalAmount += _order.CapitalAmount - _order.Freight - _order.Tax;
                }
                else
                {
                    _order.CapitalAmount = totalCapital - capitalAmount + _order.Freight + _order.Tax;
                }
            }
        }

        /// <summary>
        /// 处理商品库存，这是订单服务内部的方法，整个网站就一个地方使用
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="stockChange"></param>
        private void UpdateSKUStockInOrder(string skuId, long stockChange)
        {
            var sku = DbFactory.Default.Get<SKUInfo>().Where(item => item.Id == skuId).FirstOrDefault();
            if (sku != null)
            {
                sku.Stock += stockChange;
                if (sku.Stock < 0)
                    throw new MallException("商品库存不足");
                DbFactory.Default.Update(sku);
            }
        }

        private void UpdateShopBranchSku(long shopBranchId, string skuId, int stockChange)
        {
            var sku = DbFactory.Default.Get<SKUInfo>().Where(item => item.Id == skuId).FirstOrDefault();
            if (sku != null)
            {
                var sbSku = DbFactory.Default.Get<ShopBranchSkuInfo>().Where(p => p.ShopBranchId == shopBranchId && p.SkuId == sku.Id).FirstOrDefault();
                if (sbSku != null)
                {
                    sbSku.Stock += stockChange;
                    if (sbSku.Stock < 0)
                        throw new MallException("门店库存不足");
                    DbFactory.Default.Update(sbSku);
                }
            }
        }

        /// <summary>
        /// 优惠券状态改变
        /// </summary>
        public void UseCoupon(List<OrderInfo> orders, List<BaseAdditionalCoupon> coupons, long userid)
        {
            var couponService = ServiceProvider.Instance<ICouponService>.Create;
            var shopBonusService = ServiceProvider.Instance<IShopBonusService>.Create;
            foreach (var coupon in coupons)
            {
                if (coupon.Type == 0)
                {
                    long id = (coupon.Coupon as CouponRecordInfo).Id;
                    couponService.UseCoupon(userid, new long[] { id }, orders);
                }
                else if (coupon.Type == 1)
                {
                    long id = (coupon.Coupon as ShopBonusReceiveInfo).Id;
                    shopBonusService.SetBonusToUsed(userid, orders, id);
                }
            }
        }
        #endregion
        #endregion

        #region 私有方法
        /// <summary>
        /// 处理自提订单提货码
        /// <para>拼团订单的提货码需要成团成功后生成</para>
        /// </summary>
        /// <param name="order"></param>
        /// <param name="isMust">必须生成</param>
        private void OperaOrderPickupCode(OrderInfo order)
        {
            if (order.DeliveryType == CommonModel.DeliveryType.SelfTake)
            {
                order.OrderStatus = OrderInfo.OrderOperateStatus.WaitSelfPickUp;
                if (order.OrderType != OrderInfo.OrderTypes.FightGroup)
                {
                    order.PickupCode = GeneratePickupCode(order.Id);
                }
            }
        }
        public static string GeneratePickupCode(long orderId)
        {
            var digits = "0123456789";
            var random = new byte[3];
            _randomPickupCode.GetBytes(random);

            string newOrderId = orderId.ToString().Substring(2);
            var pickupCode = string.Format("{0}{1}", newOrderId, string.Join("", random.Select(p => digits[p % digits.Length])));
            return pickupCode;
        }

        private long[] GetOrderIdRange(string orderIdStr)
        {
            long orderId;
            if (!string.IsNullOrEmpty(orderIdStr) && long.TryParse(orderIdStr, out orderId))
            {
                var temp = this.GenerateOrderNumber().ToString();
                if (orderIdStr.Length < temp.Length)
                {
                    var len = temp.Length - orderIdStr.Length;
                    orderId = orderId * (long)Math.Pow(10, len);
                    var max = orderId + long.Parse(string.Join("", new int[len].Select(p => 9)));
                    return new[] { orderId, max };
                }
                else if (orderIdStr.Length == temp.Length)
                    return new[] { orderId };
            }

            return null;
        }
        private IMongoQueryable<OrderInfo> ToPaymentWhere(GetBuilder<OrderInfo> orders, IMongoQueryable<OrderInfo> history, OrderQuery query)
        {
            if (!string.IsNullOrWhiteSpace(query.PaymentTypeName))
            {
                if (null != orders)
                    orders.Where(p => p.PaymentTypeName.Contains(query.PaymentTypeName));

                history = history.Where(p => p.PaymentTypeName.Contains(query.PaymentTypeName));
            }
            if (query.PaymentTypeGateways != null && query.PaymentTypeGateways.Count > 0)
            {
                if (null != orders)
                    orders.Where(p => p.PaymentTypeGateway.ExIn(query.PaymentTypeGateways) && p.PaymentTypeGateway != "");

                history = history.Where(p => p.PaymentTypeGateway.ExIn(query.PaymentTypeGateways) && p.PaymentTypeGateway != "");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(query.PaymentTypeGateway))
                {
                    switch (query.PaymentTypeGateway)
                    {
                        case "组合支付":
                            orders.Where(p => p.CapitalAmount > 0 && p.PaymentTypeGateway!="");
                            break;
                        case "预存款支付":
                            orders.Where(p => p.CapitalAmount > 0);
                            history = history.Where(p => p.CapitalAmount > 0);
                            break;
                        case "货到付款":
                            orders.Where(p => p.PaymentTypeName == query.PaymentTypeGateway);
                            history = history.Where(p => p.PaymentTypeName == query.PaymentTypeGateway);
                            break;
                        case "线下收款":
                            orders.Where(p => p.PaymentType == OrderInfo.PaymentTypes.Offline);
                            history = history.Where(p => p.PaymentType == OrderInfo.PaymentTypes.Offline);
                            break;
                        case "其他":
                            orders.Where(p => p.TotalAmount == 0);
                            history = history.Where(p => p.TotalAmount == 0);
                            break;
                        default:
                            //此处会造成 查询微信支付 Mall.Plugin.Payment.WeiXinPay 时会把微信APP支付Mall.Plugin.Payment.WeiXinPay_App 和微信扫码支付 Mall.Plugin.Payment.WeiXinPay_Native查询出来
                            //  orders.Where(p => p.PaymentTypeGateway.Contains(query.PaymentTypeGateway)); 
                            if (null != orders)
                                orders.Where(p => p.PaymentTypeGateway == query.PaymentTypeGateway);

                            history = history.Where(p => p.PaymentTypeGateway == query.PaymentTypeGateway);
                            break;

                    }
                }
            }
            return history;
        }
        private IMongoQueryable<OrderInfo> ToWhere(GetBuilder<OrderInfo> orders, IMongoQueryable<OrderInfo> history, OrderQuery query)
        {
            var orderIdRange = GetOrderIdRange(query.OrderId);

            if (orderIdRange == null)
            {
                if (!string.IsNullOrWhiteSpace(query.OrderId))//如果是按订单ID查询，但又无法正确识别,那么直接按ID精准查询则无法查到数据
                {
                    long tempOrderId = 0;
                    long.TryParse(query.OrderId,out tempOrderId);
                    if (null != orders)
                        orders.Where(item => item.Id == tempOrderId);

                    history = history.Where(item => item.Id== tempOrderId);
                }
                orderIdRange = GetOrderIdRange(query.SearchKeyWords);

                if (orderIdRange == null && !string.IsNullOrWhiteSpace(query.SearchKeyWords))
                {
                    if (null != orders)
                    {
                        var sub = DbFactory.Default
                            .Get<OrderItemInfo>()
                            .Where<OrderInfo>((oii, oi) => oii.OrderId == oi.Id && oii.ProductName.Contains(query.SearchKeyWords));

                        orders.Where(p => p.ExExists(sub));
                    }
                    var flag = SiteSettingApplication.SiteSettings.IsOpenHistoryOrder;
                    if (flag)
                    {
                        var ids = DbFactory.MongoDB
                            .AsQueryable<OrderItemInfo>()
                            .Where(n => n.ProductName.Contains(query.SearchKeyWords))
                            .Select(n => n.OrderId)
                            .ToList();
                        history = history.Where(p => ids.Contains(p.Id));
                    }
                }
            }

            if (orderIdRange != null)
            {
                var min = orderIdRange[0];
                if (orderIdRange.Length == 2)
                {
                    var max = orderIdRange[1];
                    if (null != orders)
                        orders.Where(item => item.Id >= min && item.Id <= max);

                    history = history.Where(item => item.Id >= min && item.Id <= max);
                }
                else
                {
                    if (null != orders)
                        orders.Where(item => item.Id == min);

                    history = history.Where(item => item.Id == min);
                }
            }
            if (query.InvoiceType.HasValue)
            {
                var orderids = DbFactory.Default.Get<OrderInvoiceInfo>().Where(item => item.InvoiceType == (InvoiceType)query.InvoiceType).Select(item => item.OrderId).ToList<long>();
                if (orders != null)
                    orders.Where(o => o.Id.ExIn(orderids));
            }

            if (query.IsVirtual.HasValue)
            {
                if (query.IsVirtual.Value)
                {
                    if (null != orders)
                        orders.Where(p => p.OrderType == OrderInfo.OrderTypes.Virtual);

                    history = history.Where(p => p.OrderType == OrderInfo.OrderTypes.Virtual);
                }
                else
                {
                    if (null != orders)
                        orders.Where(p => p.OrderType != OrderInfo.OrderTypes.Virtual);

                    history = history.Where(p => p.OrderType != OrderInfo.OrderTypes.Virtual);
                }
            }
            if (query.IsSelfTake.HasValue && query.IsSelfTake.Value == 1)
            {
                if (null != orders)
                    orders.Where(p => p.DeliveryType == Mall.CommonModel.DeliveryType.SelfTake);

                history = history.Where(p => p.DeliveryType == Mall.CommonModel.DeliveryType.SelfTake);
            }
            if (query.ShopId.HasValue)
            {
                var shopId = query.ShopId.Value;
                if (null != orders)
                    orders.Where(p => p.ShopId == shopId);

                history = history.Where(p => p.ShopId == shopId);
            }
            if (query.ShopBranchId.HasValue && query.ShopBranchId.Value != -1)
            {
                if (query.ShopBranchId.Value == 0)
                { //查询总店
                    if (null != orders)
                        orders.Where(e => e.ShopBranchId == query.ShopBranchId.Value || e.ShopBranchId.ExIsNull());

                    history = history.Where(e => e.ShopBranchId == query.ShopBranchId.Value);
                }
                else
                {
                    if (null != orders)
                        orders.Where(e => e.ShopBranchId == query.ShopBranchId.Value);

                    history = history.Where(e => e.ShopBranchId == query.ShopBranchId.Value);
                }
            }
            if (query.AllotStore.HasValue && query.AllotStore.Value != 0)
            {
                if (query.AllotStore.Value == 1)
                {
                    if (null != orders)
                        orders.Where(e => e.ShopBranchId > 0);

                    history = history.Where(e => e.ShopBranchId > 0);
                }
                else
                {
                    if (null != orders)
                        orders.Where(e => e.ShopBranchId <= 0);

                    history = history.Where(e => e.ShopBranchId <= 0);
                }
            }
            if (!string.IsNullOrWhiteSpace(query.ShopName))
            {
                if (null != orders)
                    orders.Where(p => p.ShopName.Contains(query.ShopName));

                history = history.Where(p => p.ShopName.Contains(query.ShopName));
            }
            if (!string.IsNullOrWhiteSpace(query.UserName))
            {
                if (null != orders)
                    orders.Where(p => p.UserName.Contains(query.UserName));

                history = history.Where(p => p.UserName.Contains(query.UserName));
            }
            if (query.UserId.HasValue)
            {
                var userId = query.UserId.Value;
                if (null != orders)
                    orders.Where(p => p.UserId == userId);

                history = history.Where(p => p.UserId == userId);
            }

            history = ToPaymentWhere(orders, history, query);

            if (query.IgnoreSelfPickUp.HasValue)
            {
                if (query.IgnoreSelfPickUp.Value)
                {
                    orders.Where(p => p.DeliveryType != DeliveryType.SelfTake);
                }
            }
            if (query.Commented.HasValue)
            {
                var commented = query.Commented.Value;
                if (commented)
                {
                    if (null != orders)
                    {
                        var sub = DbFactory.Default
                            .Get<OrderCommentInfo>()
                            .Where<OrderInfo>((oci, oi) => oci.OrderId == oi.Id);
                        orders.Where(p => p.ExExists(sub));
                    }
                    var commentids = DbFactory.Default
                            .Get<OrderCommentInfo>()
                            .LeftJoin<OrderInfo>((oci, oi) => oci.OrderId == oi.Id)
                            .Where<OrderInfo>(n => n.Id.ExIsNull())
                            .Select(n => n.OrderId)
                            .Distinct()
                            .ToList<long>();
                    history = history.Where(p => p.Id.ExIn(commentids));
                }
                else
                {
                    if (null != orders)
                    {
                        var sub = DbFactory.Default
                            .Get<OrderCommentInfo>()
                            .Where<OrderInfo>((oci, oi) => oci.OrderId == oi.Id);
                        orders.Where(p => p.ExNotExists(sub));
                    }
                    var commentids = DbFactory.Default
                            .Get<OrderCommentInfo>()
                            .Select(n => n.OrderId)
                            .Distinct()
                            .ToList<long>();
                    history = history.Where(p => !commentids.Contains(p.Id));
                }
            }

            if (query.OrderType.HasValue)
            {
                var orderType = (PlatformType)query.OrderType.Value;
                if (null != orders)
                    orders.Where(item => item.Platform == orderType);

                history = history.Where(item => item.Platform == orderType);
            }

            if (query.Status.HasValue)
            {
                var _where = PredicateExtensions.False<OrderInfo>();
                var _wherehistory = PredicateExtensions.False<OrderInfo>();
                switch (query.Status)
                {
                    case OrderInfo.OrderOperateStatus.UnComment:
                        if (null != orders)
                        {
                            var comments = DbFactory.Default.Get<OrderCommentInfo>().Where<OrderInfo>((oci, oi) => oci.OrderId == oi.Id).Select(n => n.Id);
                            _where = _where.Or(d => d.ExNotExists(comments) && d.OrderStatus == OrderInfo.OrderOperateStatus.Finish);
                        }
                        var commentids = DbFactory.Default.Get<OrderCommentInfo>().Select(n => n.OrderId).Distinct();
                        if (query.UserId.HasValue && query.UserId.Value > 0) commentids.Where(n => n.UserId == query.UserId.Value);
                        var cids = commentids.ToList<long>();
                        _wherehistory = _wherehistory.Or(d => !cids.Contains(d.Id) && d.OrderStatus == OrderInfo.OrderOperateStatus.Finish);
                        break;
                    case OrderInfo.OrderOperateStatus.WaitDelivery:
                        var fgordids = DbFactory.Default
                               .Get<FightGroupOrderInfo>()
                               .Where(d => d.JoinStatus != 4)
                               .Select(d => d.OrderId.ExIfNull(0))
                               .ToList<long>();
                        if (null != orders)
                        {
                            //处理拼团的情况
                            _where = _where.Or(d => d.OrderStatus == query.Status && d.Id.ExNotIn(fgordids));
                        }
                        _wherehistory = _wherehistory.Or(d => d.OrderStatus == query.Status && fgordids.Contains(d.Id));
                        break;
                    case OrderInfo.OrderOperateStatus.History:
                        break;
                    default:
                        if (null != orders)
                            _where = _where.Or(d => d.OrderStatus == query.Status);
                        _wherehistory = _wherehistory.Or(d => d.OrderStatus == query.Status);

                        //如果是前端待收货状态时也查询出待消费的订单
                        if (query.IsFront && query.Status == OrderInfo.OrderOperateStatus.WaitReceiving)
                        {
                            if (null != orders)
                            {
                                _where = _where.Or(d => d.OrderStatus == OrderInfo.OrderOperateStatus.WaitVerification);
                            }
                            _wherehistory = _wherehistory.Or(d => d.OrderStatus == OrderInfo.OrderOperateStatus.WaitVerification);
                        }

                        break;
                }


                if (query.MoreStatus != null)
                {
                    foreach (var stitem in query.MoreStatus)
                    {
                        if (null != orders)
                            _where = _where.Or(d => d.OrderStatus == stitem);
                        _wherehistory = _wherehistory.Or(d => d.OrderStatus == stitem);
                    }
                }

                if (null != orders) orders.Where(_where);
                history = history.Where(_wherehistory);
            }

            if (query.PaymentType != OrderInfo.PaymentTypes.None)
            {
                if (null != orders)
                    orders.Where(item => item.PaymentType == query.PaymentType);

                history = history.Where(item => item.PaymentType == query.PaymentType);
            }

            if (query.IsBuyRecord)//购买记录只查询付了款的
            {
                if (null != orders)
                    orders.Where(a => a.PayDate.HasValue);

                history = history.Where(a => a.PayDate.HasValue);
            }

            //开始结束时间
            if (query.StartDate.HasValue)
            {
                DateTime sdt = query.StartDate.Value;
                if (null != orders)
                    orders.Where(d => d.OrderDate >= sdt);

                history = history.Where(d => d.OrderDate >= sdt);
            }
            if (query.EndDate.HasValue)
            {
                DateTime edt = query.EndDate.Value.AddDays(1);
                if (null != orders)
                    orders.Where(d => d.OrderDate < edt);

                history = history.Where(d => d.OrderDate < edt);
            }

            if ((query.ShopBranchId.HasValue && query.ShopBranchId.Value > 0) || !string.IsNullOrWhiteSpace(query.ShopBranchName))
            {
                if (null != orders)
                    // orders.Where(p => p.DeliveryType == CommonModel.DeliveryType.SelfTake);
                    orders.Where(p => p.DeliveryType == DeliveryType.SelfTake || p.ShopBranchId.ExIfNull(0) > 0);//3.0版本新增订单自动分配到门店，其配送方式不是到店自提但仍属于门店订单

                history = history.Where(p => p.DeliveryType == DeliveryType.SelfTake || p.ShopBranchId > 0);//3.0版本新增订单自动分配到门店，其配送方式不是到店自提但仍属于门店订单

                if (query.ShopBranchId.HasValue)
                {
                    var shopBranchId = query.ShopBranchId.Value;
                    if (null != orders)
                        orders.Where(p => p.ShopBranchId == shopBranchId);

                    history = history.Where(p => p.ShopBranchId == shopBranchId);
                }
                else
                {
                    var sbIds = DbFactory.Default
                        .Get<ShopBranchInfo>()
                        .Where(p => p.ShopBranchName.Contains(query.ShopBranchName))
                        .Select(p => p.Id)
                        .ToList<long>();
                    if (null != orders)
                        orders.Where(p => p.ShopBranchId.ExIn(sbIds));

                    history = history.Where(p => sbIds.Contains(p.ShopBranchId));
                }
            }

            if (!string.IsNullOrEmpty(query.UserContact))
            {
                if (null != orders)
                    orders.Where(p => p.CellPhone.StartsWith(query.UserContact));

                history = history.Where(p => p.CellPhone.StartsWith(query.UserContact));
            }
            return history;
        }
        #endregion

        #region 内部类
        public class OrderSkuInfo
        {

            public SKUInfo SKU { get; set; }

            public ProductInfo Product { get; set; }

            public int Quantity { get; set; }

            public long ColloPid { get; set; }
        }
        #endregion
        #region 分配门店
        /// <summary>
        /// 商家订单分配门店时更新商家、门店库存(单个订单)
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="quantity"></param>
        public void AllotStoreUpdateStock(List<string> skuIds, List<int> counts, long shopBranchId)
        {
            if (skuIds.Count > 0)
            {
                DbFactory.Default
                     .InTransaction(() =>
                     {
                         int quantity = 0; string skuId = string.Empty;
                         for (int i = 0; i < skuIds.Count(); i++)
                         {
                             skuId = skuIds[i];
                             quantity = counts.ElementAt(i);
                             SKUInfo sku = DbFactory.Default.Get<SKUInfo>().Where(p => p.Id == skuId).FirstOrDefault();
                             if (sku != null)
                             {
                                 sku.Stock += quantity;
                                 DbFactory.Default.Update(sku);
                             }
                             ShopBranchSkuInfo sbSku = DbFactory.Default.Get<ShopBranchSkuInfo>().Where(e => e.ShopBranchId == shopBranchId && e.SkuId == sku.Id).FirstOrDefault();
                             if (sbSku != null)
                             {
                                 sbSku.Stock -= quantity;
                                 DbFactory.Default.Update(sbSku);
                             }
                             if (sbSku.Stock < 0)
                                 throw new MallException("门店商品库存不足");
                         }
                     });
            }
        }

        /// <summary>
        /// 更改旧门店订单到新门店(单个订单)
        /// </summary>
        /// <param name="stuIds"></param>
        /// <param name="newShopBranchId"></param>
        /// <param name="oldShopBranchId"></param>
        public void AllotStoreUpdateStockToNewShopBranch(List<string> skuIds, List<int> counts, long newShopBranchId, long oldShopBranchId)
        {
            DbFactory.Default
                 .InTransaction(() =>
                 {
                     int quantity = 0; string skuId = string.Empty;
                     for (int i = 0; i < skuIds.Count(); i++)
                     {
                         skuId = skuIds[i];
                         quantity = counts.ElementAt(i);
                         ShopBranchSkuInfo sbSkuNew = DbFactory.Default.Get<ShopBranchSkuInfo>().Where(e => e.ShopBranchId == newShopBranchId && e.SkuId == skuId).FirstOrDefault();
                         if (sbSkuNew != null)
                         {
                             sbSkuNew.Stock -= quantity;
                             DbFactory.Default.Update(sbSkuNew);
                         }
                         ShopBranchSkuInfo sbSkuOld = DbFactory.Default.Get<ShopBranchSkuInfo>().Where(e => e.ShopBranchId == oldShopBranchId && e.SkuId == skuId).FirstOrDefault();
                         if (sbSkuOld != null)
                         {
                             sbSkuOld.Stock += quantity;
                             DbFactory.Default.Update(sbSkuOld);
                         }
                         if (sbSkuNew.Stock < 0)
                             throw new MallException("门店商品库存不足");
                     }
                 });
        }

        /// <summary>
        /// 更改门店订单回到商家(单个订单)
        /// </summary>
        public void AllotStoreUpdateStockToShop(List<string> skuIds, List<int> counts, long shopBranchId)
        {
            DbFactory.Default
                .InTransaction(() =>
                {
                    int quantity = 0; string skuId = string.Empty;
                    for (int i = 0; i < skuIds.Count(); i++)
                    {
                        skuId = skuIds[i];
                        quantity = counts.ElementAt(i);
                        SKUInfo sku = DbFactory.Default.Get<SKUInfo>().Where(p => p.Id == skuId).FirstOrDefault();
                        if (sku != null)
                        {
                            sku.Stock -= quantity;
                            DbFactory.Default.Update(sku);
                        }
                        ShopBranchSkuInfo sbSku = DbFactory.Default.Get<ShopBranchSkuInfo>().Where(e => e.ShopBranchId == shopBranchId && e.SkuId == skuId).FirstOrDefault();
                        if (sbSku != null)
                        {
                            sbSku.Stock += quantity;
                            DbFactory.Default.Update(sbSku);
                        }
                        if (sku.Stock < 0)
                            throw new MallException("商品库存不足");
                    }
                });
        }
        /// <summary>
        /// 更新订单所属门店
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopBranchId"></param>
        public void UpdateOrderShopBranch(long orderId, long shopBranchId)
        {
            var orderdata = DbFactory.Default.Get<OrderInfo>().Where(d => d.Id == orderId).FirstOrDefault();
            if (orderdata == null)
            {
                throw new MallException("错误的订单编号");
            }
            orderdata.ShopBranchId = shopBranchId;
            DbFactory.Default.Update(orderdata);
        }
        #endregion
        #region 门店/商家APP发货消息推送
        public void SendAppMessage(OrderInfo orderInfo)
        {
            var app = new AppMessageInfo()
            {
                Content = string.Format("{0} 等待您发货", orderInfo.Id),
                IsRead = false,
                sendtime = DateTime.Now,
                SourceId = orderInfo.Id,
                Title = "您有新的订单",
                TypeId = (int)AppMessagesType.Order,
                OrderPayDate = Core.Helper.TypeHelper.ObjectToDateTime(orderInfo.PayDate),
                ShopId = 0,
                ShopBranchId = 0
            };
            if (orderInfo.ShopBranchId > 0)
            {
                app.ShopBranchId = orderInfo.ShopBranchId;
            }
            else app.ShopId = orderInfo.ShopId;

            if (orderInfo.DeliveryType == DeliveryType.SelfTake)
            {
                app.Title = "您有新自提订单";
                app.TypeId = (int)AppMessagesType.Order;
                app.Content = string.Format("{0} 等待您备货", orderInfo.Id);
            }
            _iAppMessageService.AddAppMessages(app);
        }
        #endregion

        /// <summary>
        /// 获取待评价订单数量
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public int GetWaitingForComments(OrderQuery query)
        {
            GetBuilder<OrderInfo> orders = null;
            var flag = SiteSettingApplication.SiteSettings.IsOpenHistoryOrder;
            var history = DbFactory.MongoDB.AsQueryable<OrderInfo>();
            if (query.Status != OrderInfo.OrderOperateStatus.History)
            {
                orders = DbFactory.Default.Get<OrderInfo>();
            }
            history = ToWhere(orders, history, query);
            if (null == orders && flag) return history.Count();
            return orders.Count();
        }
        public List<long> GetOrderIdsByLatestTime(int time, long shopBranchId, long shopId)
        {
            var timeformat = string.Format("DATE_SUB(NOW(),INTERVAL {0} MINUTE)", time);
            var orders = DbFactory.Default
                .Get<OrderInfo>()
                .Select(n => n.Id)
                //.Where(n => n.ShopBranchId.ExIfNull(0) == shopBranchId && n.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay &&
                //    n.OrderStatus != OrderInfo.OrderOperateStatus.Close && n.OrderStatus != OrderInfo.OrderOperateStatus.Finish &&
                //    n.OrderStatus != OrderInfo.OrderOperateStatus.UnComment && n.OrderType!= OrderInfo.OrderTypes.Virtual && n.OrderDate >= timeformat.ExFormat<DateTime>());
                .Where(n => n.PayDate >= timeformat.ExFormat<DateTime>() && n.ShopBranchId.ExIfNull(0) == shopBranchId && n.OrderType != OrderInfo.OrderTypes.Virtual
                && (n.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || n.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp));
            if (shopId > 0)
            {
                orders.Where(n => n.ShopId == shopId);
            }
            return orders.ToList<long>();
        }

        /// <summary>
        /// 获取订单统计数据
        /// </summary>
        public OrderStatisticsItem GetOrderCountStatistics(OrderCountStatisticsQuery query)
        {
            var db = DbFactory.Default.Get<OrderInfo>();

            #region Where
            if (query.UserId > 0)
                db.Where(p => p.UserId == query.UserId);
            if (query.ShopId > 0)
                db.Where(p => p.ShopId == query.ShopId);
            if (query.ShopBranchId > 0)
                db.Where(p => p.ShopBranchId == query.ShopBranchId);

            if (query.OrderDateBegin.HasValue)
                db.Where(p => p.OrderDate > query.OrderDateBegin.Value);
            if (query.OrderDateEnd.HasValue)
                db.Where(p => p.OrderDate < query.OrderDateEnd.Value);

            //已支付订单
            if (query.IsPayed.HasValue && query.IsPayed.Value)
                db.Where(p => p.PayDate.ExIsNotNull());
            if (query.OrderOperateStatusList != null)
            {
                if (query.OrderOperateStatus.HasValue)
                    query.OrderOperateStatusList.Add(query.OrderOperateStatus.Value);
                db.Where(p => p.OrderStatus.ExIn(query.OrderOperateStatusList));
            }
            else if (query.OrderOperateStatus.HasValue)
            {
                //db.Where(p => p.OrderStatus == query.OrderOperateStatus.Value);
                switch (query.OrderOperateStatus)
                {
                    case OrderInfo.OrderOperateStatus.WaitDelivery:
                        var fgordids = DbFactory.Default
                               .Get<FightGroupOrderInfo>()
                               .Where(d => d.JoinStatus != 4)
                               .Select(d => d.OrderId.ExIfNull(0))
                               .ToList<long>();
                        //处理拼团的情况
                        db.Where(d => d.OrderStatus == query.OrderOperateStatus && d.Id.ExNotIn(fgordids));
                        break;
                    default:
                        db.Where(p => p.OrderStatus == query.OrderOperateStatus.Value);
                        break;
                }
            }

            if (query.IsCommented.HasValue)
            {
                if (query.IsCommented.Value)
                {
                    var sub = DbFactory.Default
                        .Get<OrderCommentInfo>()
                        .Where<OrderInfo>((oci, oi) => oci.OrderId == oi.Id);
                    db.Where(p => p.ExExists(sub));
                }
                else
                {
                    var sub = DbFactory.Default
                        .Get<OrderCommentInfo>()
                        .Where<OrderInfo>((oci, oi) => oci.OrderId == oi.Id);
                    db.Where(p => p.ExNotExists(sub));
                }
            }

            if (query.IsVirtual.HasValue)
            {
                if (query.IsVirtual.Value)
                    db.Where(p => p.OrderType == OrderInfo.OrderTypes.Virtual);//是虚拟订单
                else
                    db.Where(p => p.OrderType != OrderInfo.OrderTypes.Virtual);//非虚拟订单
            }
            #endregion

            var result = new OrderStatisticsItem();

            #region Fields
            if (query.Fields.Contains(OrderCountStatisticsFields.ActualPayAmount))
                result.TotalActualPayAmount = db.Sum(p => p.ActualPayAmount);
            if (query.Fields.Contains(OrderCountStatisticsFields.OrderCount))
                result.OrderCount = db.Count();
            #endregion
            return result;
        }
        public List<VirtualOrderItemInfo> GetVirtualOrderItemInfosByOrderId(long orderId)
        {
            return DbFactory.Default.Get<VirtualOrderItemInfo>().Where(a => a.OrderId == orderId).ToList();
        }
        public List<OrderVerificationCodeInfo> GetOrderVerificationCodeInfosByOrderIds(List<long> orderIds)
        {
            return DbFactory.Default.Get<OrderVerificationCodeInfo>().Where(a => a.OrderId.ExIn(orderIds)).ToList();
        }
        public OrderVerificationCodeInfo GetOrderVerificationCodeInfoByCode(string verificationCode)
        {
            return DbFactory.Default.Get<OrderVerificationCodeInfo>().Where(a => a.VerificationCode == verificationCode).FirstOrDefault();
        }
        public List<OrderVerificationCodeInfo> GetOrderVerificationCodeInfoByCodes(List<string> verificationCodes)
        {
            return DbFactory.Default.Get<OrderVerificationCodeInfo>().Where(a => a.VerificationCode.ExIn(verificationCodes)).ToList();
        }
        public bool UpdateOrderVerificationCodeStatusByCodes(List<string> verficationCodes, long orderId, OrderInfo.VerificationCodeStatus status, DateTime? verificationTime, string verificationUser = "")
        {
            bool result = false;
            if (verificationTime.HasValue)
            {
                result = DbFactory.Default.Set<OrderVerificationCodeInfo>().Set(p => p.Status, status).Set(a => a.VerificationTime, verificationTime.Value).Set(a => a.VerificationUser, verificationUser).Where(p => p.VerificationCode.ExIn(verficationCodes)).Succeed();
            }
            else
            {
                result = DbFactory.Default.Set<OrderVerificationCodeInfo>().Set(p => p.Status, status).Where(p => p.VerificationCode.ExIn(verficationCodes)).Succeed();
            }
            OrderInfo.OrderOperateStatus orderStatus = 0;
            var orderVerificationCodes = GetOrderVerificationCodeInfosByOrderIds(new List<long>() { orderId });
            int count1 = orderVerificationCodes.Where(a => a.Status == OrderInfo.VerificationCodeStatus.WaitVerification || a.Status == OrderInfo.VerificationCodeStatus.Refund).Count();
            int count2 = 0, count3 = 0;
            if (count1 > 0)
            {
                orderStatus = OrderInfo.OrderOperateStatus.WaitVerification;
            }
            else
            {
                count3 = orderVerificationCodes.Where(a => a.Status == OrderInfo.VerificationCodeStatus.Expired || a.Status == OrderInfo.VerificationCodeStatus.RefundComplete).Count();
                if (count3 == orderVerificationCodes.Count())
                {
                    orderStatus = OrderInfo.OrderOperateStatus.Close;
                }
                else
                {
                    var alreadyVerificationInfo = orderVerificationCodes.FirstOrDefault(a => a.Status == OrderInfo.VerificationCodeStatus.AlreadyVerification);
                    if (alreadyVerificationInfo != null)
                    {
                        var other = orderVerificationCodes.Where(a => a.Id != alreadyVerificationInfo.Id);//排除已核销
                        count2 = other.Where(a => a.Status != OrderInfo.VerificationCodeStatus.WaitVerification && a.Status != OrderInfo.VerificationCodeStatus.Refund).Count();
                        if (count2 == other.Count())
                        {
                            orderStatus = OrderInfo.OrderOperateStatus.Finish;
                        }
                    }
                }
            }
            if (orderStatus != 0)
            {
                if (orderStatus == OrderInfo.OrderOperateStatus.Finish || orderStatus == OrderInfo.OrderOperateStatus.Close)
                {
                    var closeReason = "";
                    if (orderStatus == OrderInfo.OrderOperateStatus.Close)
                    {
                        closeReason = "核销码已过期，自动关闭";
                        if (orderVerificationCodes.Where(a => a.Status == OrderInfo.VerificationCodeStatus.RefundComplete).Count() == orderVerificationCodes.Count)
                        {
                            closeReason = "核销码已退款，自动关闭";
                        }
                    }
                    DbFactory.Default.Set<OrderInfo>().Set(p => p.OrderStatus, orderStatus).Set(p => p.FinishDate, DateTime.Now).Set(p => p.CloseReason, closeReason).Where(a => a.Id == orderId).Succeed();
                    
                    var order = GetOrder(orderId);
                    //会员确认收货后，不会马上给积分，得需要过了售后维权期才给积分,（虚拟商品除外）
                    var member = DbFactory.Default.Get<MemberInfo>().Where(a => a.Id == order.UserId).FirstOrDefault();
                    AddIntegral(member, order.Id, order.TotalAmount - order.RefundTotalAmount);//增加积分
                    //更新待结算订单完成时间
                    UpdatePendingSettlnmentFinishDate(orderId, DateTime.Now);
                }
                else
                {
                    DbFactory.Default.Set<OrderInfo>().Set(p => p.OrderStatus, orderStatus).Where(a => a.Id == orderId).Succeed();
                }
                //订单付款时，就已经写入待结算
                //虚拟订单，订单状态为已完成或已关闭时，就给商家结算
                //if (orderStatus == OrderInfo.OrderOperateStatus.Finish || orderStatus == OrderInfo.OrderOperateStatus.Close)
                //{
                //    Log.Info(string.Format("订单状态：{0},写入待结算", orderStatus.ToDescription()));
                //    var order = DbFactory.Default.Get<OrderInfo>(a => a.Id == orderId).FirstOrDefault();
                //    var model = DbFactory.Default.Get<PendingSettlementOrderInfo>().Where(a => a.OrderId == orderId).FirstOrDefault();
                //    if (model != null)
                //    {
                //        DbFactory.Default.Delete<PendingSettlementOrderInfo>(model);
                //    }
                //    //每次状态变化时重新写入结算
                //    WritePendingSettlnment(order);
                //}
            }
            if (status == OrderInfo.VerificationCodeStatus.AlreadyVerification && verificationTime.HasValue)
            {
                SendMessageOnVirtualOrderVerificationSuccess(orderId, verficationCodes, verificationTime.Value);
            }
            return result;
        }
        public VerificationRecordInfo GetVerificationRecordInfoById(long id)
        {
            return DbFactory.Default.Get<VerificationRecordInfo>().Where(p => p.Id == id).FirstOrDefault();
        }
        public bool AddVerificationRecord(VerificationRecordInfo info)
        {
            return DbFactory.Default.Add(info);
        }
        public bool AddVirtualOrderItemInfo(List<VirtualOrderItemInfo> infos)
        {
            return DbFactory.Default.Add<VirtualOrderItemInfo>(infos);
        }
        public bool AddOrderVerificationCodeInfo(List<OrderVerificationCodeInfo> infos)
        {
            return DbFactory.Default.Add<OrderVerificationCodeInfo>(infos);
        }
        public int GetWaitConsumptionOrderNumByUserId(long userId = 0, long shopId = 0, long shopBranchId = 0)
        {
            if (shopId > 0)
                return DbFactory.Default.Get<OrderInfo>().Where(a => a.ShopId == shopId && a.OrderStatus == OrderInfo.OrderOperateStatus.WaitVerification).Count();
            else if (shopBranchId > 0)
                return DbFactory.Default.Get<OrderInfo>().Where(a => a.ShopBranchId == shopBranchId && a.OrderStatus == OrderInfo.OrderOperateStatus.WaitVerification).Count();

            return DbFactory.Default.Get<OrderInfo>().Where(a => a.UserId == userId && a.OrderStatus == OrderInfo.OrderOperateStatus.WaitVerification).Count();
        }

        /// <summary>
        /// 虚拟订单信息项实体
        /// </summary>
        /// <param name="orderIds">订单号集合</param>
        /// <returns></returns>
        public List<VirtualOrderItemInfo> GeVirtualOrderItemsByOrderId(IEnumerable<long> orderIds)
        {
            return DbFactory.Default.Get<VirtualOrderItemInfo>().Where(p => p.OrderId.ExIn(orderIds)).ToList();
        }

        public QueryPageModel<OrderVerificationCodeInfo> GetOrderVerificationCodeInfos(VerificationRecordQuery query)
        {
            var db = WhereVerificationCodeBuilder(query);
            db = db.OrderByDescending(p => "PayDate");

            var data = db.ToPagedList(query.PageNo, query.PageSize);

            return new QueryPageModel<OrderVerificationCodeInfo>()
            {
                Models = data,
                Total = data.TotalRecordCount
            };
        }

        private GetBuilder<OrderVerificationCodeInfo> WhereVerificationCodeBuilder(VerificationRecordQuery query)
        {
            var db = DbFactory.Default.Get<OrderVerificationCodeInfo>()
                .LeftJoin<OrderInfo>((fi, pi) => fi.OrderId == pi.Id)
                .Select()
                .Select<OrderInfo>(p => new { p.PayDate, p.ShopBranchId, p.ShopId });
            if (query.ShopBranchId.HasValue && query.ShopBranchId.Value > 0)
            {
                var ordersql = DbFactory.Default
                    .Get<OrderInfo>()
                    .Where<OrderInfo, OrderVerificationCodeInfo>((si, pi) => si.Id == pi.OrderId && si.ShopBranchId == query.ShopBranchId.Value);
                db.Where(p => p.ExExists(ordersql));
            }
            else if (query.IsAll)
            {
                var ordersql = DbFactory.Default
                    .Get<OrderInfo>()
                    .Where<OrderInfo, OrderVerificationCodeInfo>((si, pi) => si.Id == pi.OrderId && si.ShopId == query.ShopId.Value);
                db.Where(p => p.ExExists(ordersql));
            }
            else if (query.IsShop)
            {
                var ordersql = DbFactory.Default
                   .Get<OrderInfo>()
                   .Where<OrderInfo, OrderVerificationCodeInfo>((si, pi) => si.Id == pi.OrderId && si.ShopId == query.ShopId.Value && si.ShopBranchId == 0);
                db.Where(p => p.ExExists(ordersql));
            }

            //该字段有值肯定是选了商家或门店后传过来的
            if (query.Type.HasValue)
            {
                if (query.Type.Value == 1)
                {
                    var ordersql = DbFactory.Default.Get<OrderInfo>()
                                    .Where<OrderInfo, OrderVerificationCodeInfo>((si, pi) => si.Id == pi.OrderId && si.ShopId == query.SearchId);
                    db.Where(p => p.ExExists(ordersql));
                }
                else if (query.Type.Value == 2)
                {
                    var ordersql = DbFactory.Default.Get<OrderInfo>()
                                    .Where<OrderInfo, OrderVerificationCodeInfo>((si, pi) => si.Id == pi.OrderId && si.ShopBranchId == query.SearchId);
                    db.Where(p => p.ExExists(ordersql));
                }
            }
            else if (!string.IsNullOrWhiteSpace(query.ShopBranchName))
            {
                var _where = PredicateExtensions.False<OrderVerificationCodeInfo>();
                var shops = DbFactory.Default.Get<ShopInfo>().Where(a => a.ShopName.Contains(query.ShopBranchName)).ToList();
                var shopIds = shops.Select(a => a.Id);
                var count = shops.Count;
                var shopName = "";
                if (count == 1)
                    shopName = shops.FirstOrDefault().ShopName;

                if (count == 1 && shopName == query.ShopBranchName)//如果模糊查询只有一个，而且搜索词与商家名称相同，则为检索商家全称，则会把此商家及门店的所有订单都检索出来
                {
                    var _ordersql = DbFactory.Default.Get<OrderInfo>()
                                             .Where<OrderInfo, OrderVerificationCodeInfo>((si, pi) => si.Id == pi.OrderId && si.ShopId.ExIn(shopIds));
                    _where = _where.Or(p => p.ExExists(_ordersql));
                }
                else
                {
                    var _ordersql = DbFactory.Default.Get<OrderInfo>()
                                                .Where<OrderInfo, OrderVerificationCodeInfo>((si, pi) => si.Id == pi.OrderId && si.ShopId.ExIn(shopIds) && si.ShopBranchId == 0);
                    _where = _where.Or(p => p.ExExists(_ordersql));

                    var shopBranchIds = DbFactory.Default.Get<ShopBranchInfo>().Where(a => a.ShopBranchName.Contains(query.ShopBranchName)).Select(a => a.Id);
                    var ordersql = DbFactory.Default.Get<OrderInfo>()
                                                 .Where<OrderInfo, OrderVerificationCodeInfo>((si, pi) => si.Id == pi.OrderId && si.ShopBranchId.ExIn(shopBranchIds));
                    _where = _where.Or(p => p.ExExists(ordersql));
                }

                db.Where(_where);
            }

            var orderIdRange = GetOrderIdRange(query.OrderId);
            if (orderIdRange != null)
            {
                var min = orderIdRange[0];
                if (orderIdRange.Length == 2)
                {
                    var max = orderIdRange[1];
                    db.Where(item => item.OrderId >= min && item.OrderId <= max);
                }
                else
                    db.Where(item => item.OrderId == min);
            }
            if (query.Status.HasValue)
            {
                db.Where(item => item.Status == query.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.VerificationCode))
            {
                db.Where(item => item.VerificationCode == query.VerificationCode && item.Status != OrderInfo.VerificationCodeStatus.WaitVerification && item.Status != OrderInfo.VerificationCodeStatus.Refund);
            }

            if (query.PayDateStart.HasValue)
            {
                DateTime sdt = query.PayDateStart.Value;
                var ordersql = DbFactory.Default
                    .Get<OrderInfo>()
                    .Where<OrderInfo, OrderVerificationCodeInfo>((si, pi) => si.Id == pi.OrderId && si.PayDate >= sdt);
                db.Where(p => p.ExExists(ordersql));
            }
            if (query.PayDateEnd.HasValue)
            {
                DateTime sdt = query.PayDateEnd.Value.AddDays(1).AddSeconds(-1);
                var ordersql = DbFactory.Default
                    .Get<OrderInfo>()
                    .Where<OrderInfo, OrderVerificationCodeInfo>((si, pi) => si.Id == pi.OrderId && si.PayDate <= sdt);
                db.Where(p => p.ExExists(ordersql));
            }

            if (query.VerificationTimeStart.HasValue)
            {
                db.Where(item => item.VerificationTime >= query.VerificationTimeStart.Value);
            }

            if (query.VerificationTimeEnd.HasValue)
            {
                DateTime sdt = query.VerificationTimeEnd.Value.AddDays(1).AddSeconds(-1);
                db.Where(item => item.VerificationTime <= sdt);
            }

            return db;
        }

        public List<SearchShopAndShopbranchModel> GetShopOrShopBranch(string keyword, sbyte? type)
        {
            List<SearchShopAndShopbranchModel> list = new List<SearchShopAndShopbranchModel>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var shops = DbFactory.Default.Get<ShopInfo>().Where(a => a.ShopName.Contains(keyword)).ToList();
                shops.ForEach(a =>
                {
                    list.Add(new SearchShopAndShopbranchModel()
                    {
                        Name = a.ShopName,
                        Type = 1,
                        SearchId = a.Id
                    });
                });
                var shopbranchs = DbFactory.Default.Get<ShopBranchInfo>().Where(a => a.ShopBranchName.Contains(keyword)).ToList();
                shopbranchs.ForEach(a =>
                {
                    list.Add(new SearchShopAndShopbranchModel()
                    {
                        Name = a.ShopBranchName,
                        Type = 2,
                        SearchId = a.Id
                    });
                });
                if (type.HasValue)
                {
                    if (type.Value == 1)
                    {
                        list.RemoveAll(a => a.Type == 2);
                    }
                    else if (type.Value == 2)
                    {
                        list.RemoveAll(a => a.Type == 1);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 待结算实体
        /// </summary>
        /// <param name="orderIds">订单号集合</param>
        /// <returns></returns>
        public List<PendingSettlementOrderInfo> GetPendingSettlementOrdersByOrderId(IEnumerable<long> orderIds)
        {
            return DbFactory.Default.Get<PendingSettlementOrderInfo>().Where(p => p.OrderId.ExIn(orderIds)).ToList();
        }

        /// <summary>
        /// 已结算订单集合
        /// </summary>
        /// <param name="orderIds">订单号集合</param>
        /// <returns></returns>
        public List<AccountDetailInfo> GetAccountDetailByOrderId(IEnumerable<long> orderIds)
        {
            return DbFactory.Default.Get<AccountDetailInfo>().Where(p => p.OrderId.ExIn(orderIds)).ToList();
        }

        /// <summary>
        /// 订单发票实体集合
        /// </summary>
        /// <param name="orderIds">订单号集合</param>
        /// <returns></returns>
        public List<OrderInvoiceInfo> GetOrderInvoicesByOrderId(IEnumerable<long> orderIds)
        {
            return DbFactory.Default.Get<OrderInvoiceInfo>().Where(p => p.OrderId.ExIn(orderIds)).ToList();
        }

        /// <summary>
        /// 获取订单发票实体
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public OrderInvoiceInfo GetOrderInvoiceInfo(long orderId)
        {
            var model = DbFactory.Default.Get<OrderInvoiceInfo>().Where(o => o.OrderId == orderId).FirstOrDefault();
            if (model != null)
                model.RegionFullName = RegionApplication.GetFullName(model.RegionID);
            return model;
        }

        #region 私有方法
        private void UpdateOrderItemEffectiveDateByIds(List<long> orderItemIds, DateTime? payDate)
        {
            if (payDate != null)
            {
                var orderItems = DbFactory.Default.Get<OrderItemInfo>(p => p.Id.ExIn(orderItemIds)).ToList();
                var virtualProducts = ServiceProvider.Instance<IProductService>.Create.GetVirtualProductInfoByProductIds(orderItems.Select(a => a.ProductId).ToList());
                foreach (var item in orderItems)
                {
                    var virtualPInfo = virtualProducts.FirstOrDefault(a => a.ProductId == item.ProductId);
                    if (virtualPInfo != null)
                    {
                        if (virtualPInfo.EffectiveType == 1)
                        {
                            item.EffectiveDate = DateTime.Now;
                        }
                        else if (virtualPInfo.EffectiveType == 2)
                        {
                            item.EffectiveDate = payDate.Value.AddHours(virtualPInfo.Hour);
                        }
                        else if (virtualPInfo.EffectiveType == 3)
                        {
                            item.EffectiveDate = DateTime.Parse(payDate.Value.AddDays(1).ToString("yyyy-MM-dd"));
                        }
                        DbFactory.Default.Update(item);
                    }
                }
            }
        }

        private void AddOrderVerificationCodeInfo(long quantitry, long orderId, long orderItemId)
        {
            var hasCode = DbFactory.Default.Get<OrderVerificationCodeInfo>().Where(p => p.OrderId == orderId).Count();
            if (hasCode >= quantitry) return;
            var codes = new List<OrderVerificationCodeInfo>();
            for (int i = 0; i < quantitry; i++)
            {
                codes.Add(new OrderVerificationCodeInfo()
                {
                    OrderId = orderId,
                    OrderItemId = orderItemId,
                    Status = OrderInfo.VerificationCodeStatus.WaitVerification,
                    VerificationCode = GenerateRandomCode(12),
                    VerificationUser = ""
                });
            }
            AddOrderVerificationCodeInfo(codes);
        }
        private void SendMessageOnVirtualOrderPay(OrderInfo orderInfo, long productId)
        {
            if (orderInfo == null)
            {
                return;
            }
            var virtualOrderMessage = new MessageVirtualOrderInfo();
            virtualOrderMessage.OrderId = orderInfo.Id.ToString();
            virtualOrderMessage.ShopId = orderInfo.ShopId;
            virtualOrderMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;

            if (orderInfo.ShopBranchId > 0)
            {
                var shopBranchInfo = DbFactory.Default.Get<ShopBranchInfo>().Where(a => a.Id == orderInfo.ShopBranchId).FirstOrDefault();
                if (shopBranchInfo != null)
                {
                    virtualOrderMessage.ShopName = shopBranchInfo.ShopBranchName;//门店名称
                    virtualOrderMessage.Phone = shopBranchInfo.ContactPhone;//门店电话
                    virtualOrderMessage.Address = RegionApplication.GetFullName(shopBranchInfo.AddressId, CommonConst.ADDRESS_PATH_SPLIT) + CommonConst.ADDRESS_PATH_SPLIT + shopBranchInfo.AddressDetail;//门店地址
                }
            }
            else
            {
                virtualOrderMessage.ShopName = orderInfo.ShopName;//商家名称
                var shopInfo = DbFactory.Default.Get<ShopShipperInfo>().Where(a => a.ShopId == orderInfo.ShopId && a.IsDefaultSendGoods).FirstOrDefault();
                if (shopInfo != null)
                {
                    virtualOrderMessage.Phone = shopInfo.TelPhone;//商家电话
                    virtualOrderMessage.Address = RegionApplication.GetFullName(shopInfo.RegionId, CommonConst.ADDRESS_PATH_SPLIT) + CommonConst.ADDRESS_PATH_SPLIT + shopInfo.Address;//门店地址
                }
            }
            var verificationCodes = DbFactory.Default.Get<OrderVerificationCodeInfo>(a => a.OrderId == orderInfo.Id).ToList().Select(a => a.VerificationCode).ToList();
            if (verificationCodes != null && verificationCodes.Count > 0)
            {
                var codeStr = "";
                int i = 1;
                foreach (var code in verificationCodes)
                {
                    if (i >= 10)
                        codeStr += "...";
                    else
                        codeStr += code + ",";
                }
                virtualOrderMessage.VerificationCodes = codeStr;//到店的核销码
            }
            var virtualProduct = DbFactory.Default.Get<VirtualProductInfo>().Where(a => a.ProductId == productId).FirstOrDefault();
            if (virtualProduct != null)
            {
                if (virtualProduct.ValidityType && virtualProduct.EndDate.HasValue)
                {
                    virtualOrderMessage.DueTime = virtualProduct.EndDate.Value.ToString("yyyy年MM月dd日");//到期时间
                }
                else if (!virtualProduct.ValidityType)
                {
                    virtualOrderMessage.DueTime = "长期有效";
                }
                virtualOrderMessage.EffectiveType = virtualProduct.EffectiveType;
                virtualOrderMessage.Hour = virtualProduct.Hour;
            }
            Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnVirtualOrderPay(orderInfo.UserId, virtualOrderMessage));
        }
        private void SendMessageOnVirtualOrderVerificationSuccess(long orderId, List<string> verificationCodes, DateTime verificationTime)
        {
            var orderInfo = DbFactory.Default.Get<OrderInfo>().Where(a => a.Id == orderId).FirstOrDefault();
            if (orderInfo == null)
            {
                return;
            }
            var virtualOrderMessage = new MessageVirtualOrderVerificationInfo();
            virtualOrderMessage.OrderId = orderInfo.Id.ToString();
            virtualOrderMessage.ShopId = orderInfo.ShopId;
            virtualOrderMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;

            var orderItemInfo = DbFactory.Default.Get<OrderItemInfo>().Where(a => a.OrderId == orderId).FirstOrDefault();
            if (orderItemInfo != null)
            {
                var productInfo = DbFactory.Default.Get<ProductInfo>().Where(a => a.Id == orderItemInfo.ProductId).FirstOrDefault();
                if (productInfo != null)
                {
                    virtualOrderMessage.ProductName = productInfo.ProductName;
                }
            }
            virtualOrderMessage.VerificationTime = verificationTime.ToString("yyyy年MM月dd日");

            if (orderInfo.ShopBranchId > 0)
            {
                var shopBranchInfo = DbFactory.Default.Get<ShopBranchInfo>().Where(a => a.Id == orderInfo.ShopBranchId).FirstOrDefault();
                if (shopBranchInfo != null)
                {
                    virtualOrderMessage.ShopBranchName = shopBranchInfo.ShopBranchName;//核销门店名称
                }
            }
            else
            {
                virtualOrderMessage.ShopBranchName = orderInfo.ShopName;//核销商家名称
            }

            if (verificationCodes != null && verificationCodes.Count > 0)
            {
                virtualOrderMessage.VerificationCodes = string.Join(",", verificationCodes);//本次核销的核销码
            }
            Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnVirtualOrderVerificationSuccess(orderInfo.UserId, virtualOrderMessage));
        }
        private string GenerateRandomCode(int length)
        {
            var result = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                var r = new Random(Guid.NewGuid().GetHashCode());
                result.Append(r.Next(0, 10));
            }
            return result.ToString();
        }
        #endregion
    }
}