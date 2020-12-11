using AutoMapper;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Application
{
    /// <summary>
    /// 拼团逻辑
    /// </summary>
    public class FightGroupApplication : BaseApplicaion<IFightGroupService>
    {
        /// <summary>
        /// 当前营销类型
        /// </summary>
        private static MarketType CurMarketType = MarketType.FightGroup;

        #region 系统
        /// <summary>
        /// 拼团营销活动费用设置
        /// </summary>
        /// <returns></returns>
        public static decimal GetMarketServicePrice()
        {
            var marketser = MarketApplication.GetServiceSetting(CurMarketType);
            if (marketser == null)
            {
                marketser = new Entities.MarketSettingInfo() { TypeId = CurMarketType, Price = 0 };
                MarketApplication.AddOrUpdateServiceSetting(marketser);
            }
            return marketser.Price;
        }
        /// <summary>
        /// 设置拼团营销活动费用设置
        /// </summary>
        /// <param name="price"></param>
        public static void SetMarketServicePrice(decimal price)
        {
            Entities.MarketSettingInfo marketser = new Entities.MarketSettingInfo() { TypeId = CurMarketType, Price = price };
            MarketApplication.AddOrUpdateServiceSetting(marketser);
        }
        /// <summary>
        /// 是否已开启拼团营销
        /// </summary>
        /// <returns></returns>
        public static bool IsOpenMarketService()
        {
            bool result = false;
            var marketser = MarketApplication.GetServiceSetting(CurMarketType);
            if (marketser != null)
            {
                if (marketser.Price >= 0)
                {
                    result = true;
                }
            }
            return result;
        }
        /// <summary>
        /// 获取拼团营销服务
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static MarketServiceModel GetMarketService(long shopId)
        {
            MarketServiceModel result = new MarketServiceModel();
            var market = MarketApplication.GetMarketService(shopId, CurMarketType);
            var marketser = MarketApplication.GetServiceSetting(CurMarketType);
            result.LastBuyPrice = -1;
            if (marketser != null)
            {
                if (marketser.Price >= 0)
                {
                    result.ShopId = shopId;
                    result.Price = marketser.Price;
                    result.MarketType = CurMarketType;
                    if (market != null && market.Id>0)
                    {
                        result.EndTime = MarketApplication.GetServiceEndTime(market.Id);
                        result.LastBuyPrice = MarketApplication.GetLastBuyPrice(market.Id);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 是否可以使用拼团服务
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static bool IsCanUseMarketService(long shopId)
        {
            bool result = false;
            if (shopId <= 0)
            {
                throw new MallException("错误的商家编号");
            }
            var market = GetMarketService(shopId);
            if (market != null)
            {
                if (market.IsBuy)
                {
                    result = !market.IsExpired;
                }
            }
            return result;
        }
        /// <summary>
        /// 购买拼团服务
        /// </summary>
        /// <param name="month">数量(月)</param>
        /// <param name="shopId">店铺编号</param>
        public static void BuyMarketService(int month, long shopId)
        {

            if (shopId <= 0)
            {
                throw new MallException("错误的商家编号");
            }
            if (month <= 0)
            {
                throw new MallException("错误的购买数量(月)");
            }
            if (month > 120)
            {
                throw new MallException("购买数量(月)过大");
            }
            MarketApplication.OrderMarketService(month, shopId, CurMarketType);
        }

        /// <summary>
        /// 获取服务购买列表
        /// </summary>
        /// <param name="shopName"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static QueryPageModel<MarketServiceBuyRecordModel> GetMarketServiceBuyList(string shopName, int page = 1, int pagesize = 10)
        {
            QueryPageModel<MarketServiceBuyRecordModel> result = new QueryPageModel<MarketServiceBuyRecordModel>();
            var queryModel = new MarketBoughtQuery()
            {
                PageSize = pagesize,
                PageNo = page,
                ShopName = shopName,
                MarketType = CurMarketType
            };

            QueryPageModel<Entities.MarketServiceRecordInfo> marketEntities = MarketApplication.GetBoughtShopList(queryModel);
            if (marketEntities.Total > 0)
            {
                result.Models = marketEntities.Models.Select(d => {
                    var market = MarketApplication.GetMarketService(d.MarketServiceId);
                    return new MarketServiceBuyRecordModel
                    {
                        Id = d.Id,
                        EndTime = d.EndTime,
                        MarketServiceId = d.MarketServiceId,
                        StartTime = d.StartTime,
                        SettlementFlag = d.SettlementFlag,
                        ShopName = market.ShopName
                    };
                }).ToList();
            }
            if (result.Models == null)
            {
                result.Models = new List<MarketServiceBuyRecordModel>();
            }
            result.Total = marketEntities.Total;

            return result;
        }
        #endregion

        #region 拼团活动
        /// <summary>
        /// 新增拼团活动
        /// </summary>
        /// <param name="data"></param>
        public static void AddActive(FightGroupActiveModel data)
        {
            //    Mapper.CreateMap<FightGroupActiveModel, FightGroupActiveInfo>();
            //  Mapper.CreateMap<FightGroupActiveItemModel, FightGroupActiveItemInfo>();
            // var model = Mapper.Map<FightGroupActiveModel, FightGroupActiveInfo>(data);
            var model = data.Map<FightGroupActiveInfo>();
            
            Service.AddActive(model);
            Cache.Remove(CacheKeyCollection.CACHE_FIGHTGROUP);
        }

        /// <summary>
        /// 商品是否可以参加拼团活动
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool ProductCanJoinActive(long productId)
        {
            bool result = false;
            result = Service.ProductCanJoinActive(productId);
            return result;
        }
        /// <summary>
        /// 修改拼团活动
        /// </summary>
        /// <param name="data"></param>
        public static void UpdateActive(FightGroupActiveModel data)
        {
            #region 更新活动
            //   Mapper.CreateMap<FightGroupActiveModel, FightGroupActiveInfo>();
            //  Mapper.CreateMap<FightGroupActiveItemModel, FightGroupActiveItemInfo>();
            // var model = Mapper.Map<FightGroupActiveModel, FightGroupActiveInfo>(data);
            var model = data.Map<FightGroupActiveInfo>();
            
            Service.UpdateActive(model);
            Cache.Remove(CacheKeyCollection.CACHE_FIGHTGROUP);
            #endregion
        }
        /// <summary>
        /// 下架拼团活动
        /// </summary>
        /// <param name="id"></param>
        /// <param name="manageRemark">下架原因</param>
        /// <param name="manageId">管理员编号</param>
        public static void CancelActive(long id, string manageRemark, long manageId)
        {
            Service.CancelActive(id, manageRemark, manageId);
        }
        /// <summary>
        /// 获取拼团活动
        /// </summary>
        /// <param name="id"></param>
        /// <param name="needGetProductCommentNumber">是否需要同步获取商品的评价数量</param>
        /// <param name="isLoadItems">是否加载节点信息</param>
        /// <returns></returns>
        public static FightGroupActiveModel GetActive(long id, bool needGetProductCommentNumber = false, bool isLoadItems = true)
        {
            var data = Service.GetActive(id, needGetProductCommentNumber, isLoadItems);
            //   Mapper.CreateMap<FightGroupActiveInfo, FightGroupActiveModel>();
            //规格映射
            //Mapper.CreateMap<FightGroupActiveItemInfo, FightGroupActiveItemModel>();
            //FightGroupActiveModel result = Mapper.Map<Entities.FightGroupActiveInfo, FightGroupActiveModel>(data);

            FightGroupActiveModel result = data.Map<FightGroupActiveModel>();


            if (result != null)
            {
                //商品图片地址修正
                result.ProductDefaultImage = MallIO.GetProductSizeImage(data.ProductImgPath, 1, (int)ImageSize.Size_350);
                if (!string.IsNullOrWhiteSpace(result.IconUrl))
                {
                    result.IconUrl = Mall.Core.MallIO.GetImagePath(result.IconUrl);
                }
                if (result.ActiveItems != null)
                {
                    foreach (var item in result.ActiveItems)
                    {
                        if (!string.IsNullOrWhiteSpace(item.ShowPic))
                        {
                            item.ShowPic = MallIO.GetImagePath(item.ShowPic);
                        }
                    }
                }
                //商品主图视频地址修正
                if (!string.IsNullOrWhiteSpace(data.VideoPath))
                {
                    result.VideoPath = Mall.Core.MallIO.GetRomoteImagePath(data.VideoPath);
                }
            }
            return result;
        }
        public static List<FightGroupActiveModel> GetActives(long[] ids)
        {
            var data = Service.GetActive(ids);
            //Mapper.CreateMap<FightGroupActiveInfo, FightGroupActiveModel>();
            //规格映射
            // Mapper.CreateMap<FightGroupActiveItemInfo, FightGroupActiveItemModel>();
            // List<FightGroupActiveModel> result = Mapper.Map<List<Entities.FightGroupActiveInfo>, List<FightGroupActiveModel>>(data);

            List<FightGroupActiveModel> result =data.Map<List<FightGroupActiveModel>>();


            return result;
        }
        /// <summary>
        /// 使用商品编号获取正在进行的拼团活动
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static FightGroupActiveModel GetActiveByProductId(long productId)
        {
            long actid = Service.GetActiveIdByProductId(productId);
            FightGroupActiveModel result = null;
            if (actid > 0)
            {
                result = GetActive(actid, false, false);
            }
            return result;
        }

        /// <summary>
        /// 使用商品编号获取正在进行的拼团活动
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static FightGroupActiveModel GetActiveAndItemsByProductId(long productId)
        {
            long actid = Service.GetActiveIdByProductId(productId);
            FightGroupActiveModel result = null;
            if (actid > 0)
            {
                result = GetActive(actid, false, true);
            }
            return result;
        }
        public static List<FightGroupActiveModel> GetActiveByProductIds(long[] productIds)
        {
            var actids = Service.GetActiveIdByProductIds(productIds);
            var result = new List<FightGroupActiveModel>();
            if (actids != null && actids.Count > 0)
            {
                var a =
                result = GetActives(actids.ToArray());
            }
            return result;
        }
        /// <summary>
        /// 获取拼团项
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<FightGroupActiveItemModel> GetActiveItems(long id)
        {
            List<FightGroupActiveItemModel> result = new List<FightGroupActiveItemModel>();
            var data = Service.GetActiveItems(id);
            //  Mapper.CreateMap<List<FightGroupActiveItemInfo>, List<FightGroupActiveItemModel>>();
            //result = Mapper.Map<List<FightGroupActiveItemModel>>(data);
            result = data.Map<List<FightGroupActiveItemModel>>();


            return result;
        }
        /// <summary>
        /// 获取拼团项用于新增活动
        /// </summary>
        /// <param name="productId">商品编号</param>
        /// <returns></returns>
        public static FightGroupGetSkuListModel GetNewActiveItems(long productId)
        {
            FightGroupGetSkuListModel result = new FightGroupGetSkuListModel();
            var pro = GetService<IProductService>().GetProduct(productId);
            result.ProductImg = MallIO.GetProductSizeImage(pro.RelativePath, 1, (int)Mall.CommonModel.ImageSize.Size_150);
            result.skulist = new List<FightGroupActiveItemModel>();
            var price = 0m;
            if (pro.IsOpenLadder)
            {
                var ladder = ProductManagerApplication.GetLadderPriceByProductIds(productId);
                if (ladder.Any())
                {
                    price = ladder.OrderBy(m => m.MinBath).FirstOrDefault().Price;
                }
            }
            var skus = ProductManagerApplication.GetSKUs(pro.Id);
            foreach (var item in skus)
            {
                FightGroupActiveItemModel data = new FightGroupActiveItemModel();
                data.ProductId = productId;
                data.SkuId = item.Id;
                data.SkuName = item.Color + " " + item.Size + " " + item.Version;
                data.ProductPrice = price > 0 ? price : item.SalePrice;
                data.ProductStock = item.Stock;
                data.ActivePrice = data.ProductPrice;
                data.ActiveStock = 0;  //活动库存置空
                result.skulist.Add(data);
            }
            return result;
        }
        /// <summary>
        /// 获取活动列表
        /// </summary>
        /// <param name="Statuses">状态集</param>
        /// <param name="ProductName">商品名</param>
        /// <param name="ShopName">店铺名</param>
        /// <param name="ShopId">店铺编号</param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static QueryPageModel<FightGroupActiveListModel> GetActives(FightGroupActiveQuery query)
        {
            QueryPageModel<FightGroupActiveListModel> result = null;
            var data = Service.GetActives(query);
            //Mapper.CreateMap<FightGroupActiveInfo, FightGroupActiveListModel>();
            // Mapper.CreateMap<QueryPageModel<FightGroupActiveInfo>, QueryPageModel<FightGroupActiveListModel>>();
            //result = Mapper.Map<QueryPageModel<FightGroupActiveListModel>>(data);

            result = data.Map<QueryPageModel<FightGroupActiveListModel>>();

            if (result.Total > 0)
            {
                //数据映射
                foreach (var item in result.Models)
                {
                    if (!string.IsNullOrWhiteSpace(item.IconUrl))
                    {
                        item.IconUrl = Mall.Core.MallIO.GetImagePath(item.IconUrl);
                    }
                }
            }
            return result;
        }
        #endregion

        #region 拼团组团详情
        /// <summary>
        /// 开团
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="userId">团长用户编号</param>
        /// <returns>组团编号</returns>
        public static long OpenGroup(long activeId, long userId)
        {
            return Service.AddGroup(activeId, userId).Id;
        }
        /// <summary>
        /// 是否可以参团
        /// </summary>
        /// <param name="activeId"></param>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool CanJoinGroup(long activeId, long groupId, long userId)
        {
            return Service.CanJoinGroup(activeId, groupId, userId);
        }
        /// <summary>
        /// 获取拼团
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="groupId">团编号</param>
        /// <returns></returns>
        public static FightGroupsModel GetGroup(long activeId, long groupId)
        {
            var data = Service.GetGroup(activeId, groupId);
            //   Mapper.CreateMap<FightGroupInfo, FightGroupsModel>();
            //子数据映射
            //Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            //FightGroupsModel result = Mapper.Map<FightGroupsModel>(data);

            FightGroupsModel result = data.Map<FightGroupsModel>();


            if (result != null)
            {
                result.HeadUserIcon = Mall.Core.MallIO.GetImagePath(result.HeadUserIcon);
                if (result.GroupOrders != null)
                {
                    foreach (var subitem in result.GroupOrders)
                    {
                        subitem.Photo = Mall.Core.MallIO.GetImagePath(subitem.Photo);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 获取拼团详情列表
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="Statuses">状态集</param>
        /// <param name="StartTime">开始时间</param>
        /// <param name="EndTime">结束时间</param>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        public static QueryPageModel<FightGroupsListModel> GetGroups(
            long activeId,
            List<FightGroupBuildStatus> Statuses,
            DateTime? StartTime,
            DateTime? EndTime,
            int PageNo,
            int PageSize
            )
        {
            //  Mapper.CreateMap<FightGroupInfo, FightGroupsListModel>();
            //  Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            // var data = Service.GetGroups(activeId, Statuses, StartTime, EndTime, PageNo, PageSize);


            var data = Service.GetGroups(activeId, Statuses, StartTime, EndTime, PageNo, PageSize);


            QueryPageModel<FightGroupsListModel> result = new QueryPageModel<FightGroupsListModel>();
            result.Total = data.Total;
            result.Models = new List<FightGroupsListModel>();
            if (data.Total > 0)
            {
                foreach (var item in data.Models)
                {
                    item.HeadUserIcon = Mall.Core.MallIO.GetImagePath(item.HeadUserIcon);
                    //     var _tmp = Mapper.Map<FightGroupsListModel>(item);
                    var _tmp = item.Map<FightGroupsListModel>();


                    if (item.GroupOrders != null)
                    {
                        _tmp.OrderIds = new List<long>();
                        foreach (var ord in item.GroupOrders)
                            _tmp.OrderIds.Add(ord.OrderId);
                    }
                    result.Models.Add(_tmp);
                }
            }
            return result;
        }
        public static List<FightGroupsListModel> GetCanJoinGroupsFirst(List<FightGroupBuildStatus> Statuses, int PageNo = 1, int PageSize = 10)
        {
           // Mapper.CreateMap<FightGroupInfo, FightGroupsListModel>();
           // Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            var data = Service.GetCanJoinGroupsFirst(Statuses, PageNo, PageSize);
            List<FightGroupsListModel> result = new List<FightGroupsListModel>();
            if (data.Count > 0)
            {
                foreach (var item in data)
                {
                    item.HeadUserIcon = Mall.Core.MallIO.GetImagePath(item.HeadUserIcon);
                    //  var _tmp = Mapper.Map<FightGroupsListModel>(item);
                    var _tmp = item.Map<FightGroupsListModel>();


                    if (item.GroupOrders != null)
                    {
                        _tmp.OrderIds = new List<long>();
                        foreach (var ord in item.GroupOrders)
                            _tmp.OrderIds.Add(ord.OrderId);
                    }
                    result.Add(_tmp);
                }
            }
            return result;
        }
        public static List<FightGroupsListModel> GetCanJoinGroupsSecond(long[] unActiveId, List<FightGroupBuildStatus> Statuses)
        {
          //  Mapper.CreateMap<FightGroupInfo, FightGroupsListModel>();
           // Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            var data = Service.GetCanJoinGroupsSecond(unActiveId, Statuses);
            List<FightGroupsListModel> result = new List<FightGroupsListModel>();
            if (data.Count > 0)
            {
                foreach (var item in data)
                {
                    item.HeadUserIcon = Mall.Core.MallIO.GetImagePath(item.HeadUserIcon);
                    //    var _tmp = Mapper.Map<FightGroupsListModel>(item);
                    var _tmp = item.Map<FightGroupsListModel>();


                    if (item.GroupOrders != null)
                    {
                        _tmp.OrderIds = new List<long>();
                        foreach (var ord in item.GroupOrders)
                            _tmp.OrderIds.Add(ord.OrderId);
                    }
                    result.Add(_tmp);
                }
            }
            return result;
        }
        /// <summary>
        /// 获取参与的拼团
        /// </summary>
        /// <param name="userId">用户编号</param>
        /// <param name="Statuses">参与状态</param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static QueryPageModel<FightGroupsModel> GetJoinGroups(
            long userId
            , List<FightGroupOrderJoinStatus> Statuses = null
            , int PageNo = 1
            , int PageSize = 10
            )
        {
           // Mapper.CreateMap<FightGroupInfo, FightGroupsModel>();
           // Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
          //  var map = Mapper.CreateMap<QueryPageModel<FightGroupInfo>, QueryPageModel<FightGroupsModel>>();
            var data = Service.GetJoinGroups(userId, Statuses, PageNo, PageSize);
            //QueryPageModel<FightGroupsModel> result = Mapper.Map<QueryPageModel<FightGroupsModel>>(data);

            QueryPageModel<FightGroupsModel> result = data.Map<QueryPageModel<FightGroupsModel>>();

            foreach (var item in result.Models)
            {
                item.HeadUserIcon = Mall.Core.MallIO.GetImagePath(item.HeadUserIcon);
                if (item.GroupOrders != null)
                {
                    foreach (var subitem in item.GroupOrders)
                    {
                        subitem.Photo = Mall.Core.MallIO.GetImagePath(subitem.Photo);
                    }
                }
            }
            return result;
        }
        #endregion

        #region 拼团订单
        /// <summary>
        /// 用户在营销活动中已购买数量
        /// </summary>
        /// <param name="activeId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static long GetMarketSaleCountForUserId(long activeId, long userId)
        {
            return Service.GetMarketSaleCountForUserId(activeId, userId);
        }
        /// <summary>
        /// 拼团订单
        /// </summary>
        /// <param name="actionId">活动编号</param>
        /// <param name="orderId">订单编号</param>
        /// <param name="userId">用户编号</param>
        /// <param name="groupId">拼团编号 0表示开新团</param>
        public static FightGroupOrderModel AddOrder(long actionId, long orderId, long userId, long groupId = 0)
        {
            FightGroupOrderInfo data = Service.AddOrder(actionId, orderId, userId, groupId);
            //Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            // FightGroupOrderModel result = Mapper.Map<FightGroupOrderModel>(data);
            FightGroupOrderModel result = data.Map<FightGroupOrderModel>();


            if (result != null)
            {
                result.Photo = Mall.Core.MallIO.GetImagePath(result.Photo);
            }
            return result;
        }
        /// <summary>
        /// 设定加入拼团状态
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="status">状态</param>
        public static FightGroupOrderJoinStatus SetOrderStatus(long orderId, FightGroupOrderJoinStatus status)
        {
            FightGroupOrderJoinStatus result = Service.SetOrderStatus(orderId, status);
            return result;
        }
        /// <summary>
        /// 获取拼团订单
        /// </summary>
        /// <param name="orderId">订单编号</param>
        /// <returns></returns>
        public static FightGroupOrderModel GetOrder(long orderId)
        {
          //  Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            var data = Service.GetOrder(orderId);
            //  FightGroupOrderModel result = Mapper.Map<FightGroupOrderModel>(data);

            FightGroupOrderModel result = data.Map<FightGroupOrderModel>();

            if (result != null)
            {
                result.Photo = Mall.Core.MallIO.GetImagePath(result.Photo);
            }
            return result;
        }

        /// <summary>
        /// 获取参团中的订单数
        /// </summary>
        /// <param name="userId">用户编号</param>
        /// <returns></returns>
        public static int CountJoiningOrder(long userId)
        {
            return Service.CountJoiningOrder(userId);
        }

        #endregion

        /// <summary>
        /// 订单是否可以支付
        /// <para>成团成功后，未完成支付的订单不可付款</para>
        /// <para>成团失败后，未完成支付的订单不可付款</para>
        /// </summary>
        /// <param name="orderId">订单编号</param>
        /// <returns></returns>
        public static bool OrderCanPay(long orderId)
        {
            return Service.OrderCanPay(orderId);
        }

        /// <summary>
        /// 根据原订单号获取拼团订单信息
        /// </summary>
        /// <param name="orderId">原订单号</param>
        /// <returns></returns>
        public static FightGroupOrderInfo GetFightGroupOrderStatusByOrderId(long orderId)
        {
            return Service.GetFightGroupOrderStatusByOrderId(orderId);
        }

        public static List<FightGroupPrice> GetFightGroupPrice()
        {
            if (Cache.Exists(CacheKeyCollection.CACHE_FIGHTGROUP))
                return Cache.Get<List<FightGroupPrice>>(CacheKeyCollection.CACHE_FIGHTGROUP);
            var result = Service.GetFightGroupPrice();
            Cache.Insert(CacheKeyCollection.CACHE_FIGHTGROUP, result, 120);
            return result;
        }

        /// <summary>
        /// 获取用户拼团商品的已购买数量（商品所有拼团活动）
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static Dictionary<long, long> GetMarketSaleCountForProductIdAndUserId(IEnumerable<long> productIds, long userId)
        {
            return Service.GetMarketSaleCountForProductIdAndUserId(productIds, userId);
        }
        
        /// <summary>
        /// 根据拼团活动Id获取拼图销量
        /// </summary>
        /// <param name="activeId">拼团活动Id</param>
        /// <returns></returns>
        public static long GetFightGroupSaleVolumeByActiveId(long activeId)
        {
            return Service.GetFightGroupSaleVolumeByActiveId(activeId);
        }

        public static FightGroupActiveInfo GetFightGroupActiveInfo(long id)
        {
            return Service.GetFightGroupActiveInfo(id);
        }
    }
}
