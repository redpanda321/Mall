using AutoMapper;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Nop.Core.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Application
{
    /// <summary>
    /// 拼团逻辑
    /// </summary>
    public class FullDiscountApplication:BaseApplicaion<IFullDiscountService>
    {
        /// <summary>
        /// 当前营销类型
        /// </summary>
        private static MarketType CurMarketType = MarketType.FullDiscount;
       // private static IProductService _iProductService =  EngineContext.Current.Resolve<IProductService>();


        private static IProductService _iProductService =  EngineContext.Current.Resolve<IProductService>();


        #region 满减活动查询
        /// <summary>
        /// 获取活动
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static FullDiscountActive GetActive(long id)
        {
            FullDiscountActive result = null;
            var _adata = Service.GetActive(id);
            //  result = Mapper.Map<Entities.ActiveInfo, FullDiscountActive>(_adata);
            result = _adata.Map<FullDiscountActive>();


            if (result != null)
            {
                var _rlist = Service.GetActiveRules(id);
                //    result.Rules = Mapper.Map<List<FullDiscountRuleInfo>, List<FullDiscountRules>>(_rlist);

                result.Rules = _rlist.Map< List<FullDiscountRules>>();

                var _plist = Service.GetActiveProducts(id);
                //    result.Products = Mapper.Map<List<ActiveProductInfo>, List<FullDiscountActiveProduct>>();

                result.Products = _plist.Map<List<FullDiscountActiveProduct>>();


                if (result.IsAllProduct)
                {
                    result.ProductCount = -1;
                }
                else
                {
                    result.ProductCount = result.Products.Count;
                }
            }

            return result;
        }

        public static List<ActiveProductInfo> GetActiveProductIds(long activeID)
        {
            return Service.GetActiveProducts(activeID);
        }


        /// <summary>
        /// 获取某个店铺的一批商品正在进行的满额减活动
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static List<FullDiscountActive> GetOngoingActiveByProductIds(IEnumerable<long> productIds, long shopId)
        {
            var onGoingActives = Service.GetOngoingActiveByProductIds(productIds, shopId);
            // var result = Mapper.Map<List<Entities.ActiveInfo>, List<FullDiscountActive>>(onGoingActives);
            var result = onGoingActives.Map< List<FullDiscountActive>>();


            return result;
        }


        /// <summary>
        /// 获取某个实体正在参加的满额折
        /// </summary>
        /// <param name="proId"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static FullDiscountActive GetOngoingActiveByProductId(long proId, long shopId, long branchId=0)
        {
            FullDiscountActive result = null;
            if (branchId <= 0)
            {//非门店，则验证是否有限时购
                //限时购不参与满额减（bug需求34735）
                var ltmbuy = ServiceApplication.Create<ILimitTimeBuyService>().GetLimitTimeMarketItemByProductId(proId);
                if (ltmbuy != null)
                {
                    return result;
                }
            }
            var active = Service.GetOngoingActiveByProductId(proId, shopId);
            if (active == null)
                return result;
            //result = Mapper.Map<Entities.ActiveInfo, FullDiscountActive>(active);
            result = active.Map<FullDiscountActive>();


            var _rlist = Service.GetActiveRules(result.Id);
            //   result.Rules = Mapper.Map<List<Entities.FullDiscountRuleInfo>, List<FullDiscountRules>>(_rlist);
            result.Rules = _rlist.Map<List<FullDiscountRules>>();


            return result;
        }

        public static List<FullDiscountRuleInfo> GetActiveRules(long activeID)
        {
            return Service.GetActiveRules(activeID);
        }
        /// <summary>
        /// 获取商家正在参加的满额减
        /// </summary>
        /// <param name="proId"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static List<Entities.ActiveInfo> GetOngoingActiveByShopId(long shopId)
        {
            FullDiscountActiveQuery query = new FullDiscountActiveQuery()
            {
                ShopId = shopId,
                PageNo = 1,
                PageSize = int.MaxValue,
                Status = FullDiscountStatus.Ongoing
            };
            QueryPageModel<FullDiscountActiveList> result = new QueryPageModel<FullDiscountActiveList>();
            var datalist = Service.GetActives(query);

            return datalist.Models;
        }
        /// <summary>
        /// 获取活动列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<FullDiscountActiveList> GetActives(FullDiscountActiveQuery query)
        {
            QueryPageModel<FullDiscountActiveList> result = new QueryPageModel<FullDiscountActiveList>();
            var datalist = Service.GetActives(query);
            result.Total = datalist.Total;
            var actids = datalist.Models.Select(d => d.Id).ToList();
            var fdparg = Service.GetActivesProductCountAggregate(actids);
            result.Models = new List<FullDiscountActiveList>();
            foreach (var item in datalist.Models)
            {
                //   FullDiscountActiveList _data = Mapper.Map<Entities.ActiveInfo, FullDiscountActiveList>(item);
                FullDiscountActiveList _data = item.Map< FullDiscountActiveList>();


                if (_data.IsAllProduct)
                {
                    _data.ProductCount = -1;
                }
                else
                {
                    var _parg = fdparg.FirstOrDefault(d => d.ActiveId == _data.Id);
                    if (_parg != null)
                    {
                        _data.ProductCount = _parg.ProductCount;
                    }
                }
                result.Models.Add(_data);
            }
            return result;
        }
        /// <summary>
        /// 商品是否可以参加满减活动
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="activeId">添加活动使用0</param>
        /// <returns></returns>
        public static bool ProductCanJoinActive(long productId, long activeId)
        {
            return Service.ProductCanJoinActive(productId, activeId);
        }
        /// <summary>
        /// 过滤活动商品编号
        /// <para>返回可以能加商动的商品</para>
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="activeId">添加活动使用0</param>
        /// <param name="shopId">店铺编号</param>
        /// <returns></returns>
        public static List<long> FilterActiveProductId(IEnumerable<long> productIds, long activeId, long shopId)
        {
            return Service.FilterActiveProductId(productIds, activeId, shopId);
        }
        #endregion

        #region 满减活动操作
        public static void AddActive(FullDiscountActive model)
        {
            // Entities.ActiveInfo data = Mapper.Map<FullDiscountActive, Entities.ActiveInfo>(model);
            Entities.ActiveInfo data = model.Map< Entities.ActiveInfo>();


            //   List<Entities.FullDiscountRuleInfo> rules = Mapper.Map<List<FullDiscountRules>, List<Entities.FullDiscountRuleInfo>>(model.Rules);
            List<Entities.FullDiscountRuleInfo> rules = model.Rules.Map<List<Entities.FullDiscountRuleInfo>>();


            // List<Entities.ActiveProductInfo> products = Mapper.Map<List<FullDiscountActiveProduct>, List<Entities.ActiveProductInfo>>(model.Products);
            List<Entities.ActiveProductInfo> products = model.Products.Map< List<Entities.ActiveProductInfo>>();



            //判断活动是否可添加
            if (!Service.CanOperationActive(data, products))
            {
                throw new MallException("有其他冲突活动存在，不可以完成操作");
            }

            Service.AddActive(data, rules, products);
            //值回填
            model.Id = data.Id;
            foreach (var item in model.Rules)
            {
                item.ActiveId = model.Id;
            }
            foreach (var item in model.Products)
            {
                item.ActiveId = model.Id;
            }
        }
        /// <summary>
        /// 更新满减活动
        /// </summary>
        /// <param name="model"></param>
        public static void UpdateActive(FullDiscountActive model)
        {
            //  var data = Mapper.Map<FullDiscountActive, ActiveInfo>(model);
            var data = model.Map< ActiveInfo>();


            //     var rules = Mapper.Map<List<FullDiscountRules>, List<FullDiscountRuleInfo>>(model.Rules);
            var rules = model.Rules.Map< List<FullDiscountRuleInfo>>();


            //     var products = Mapper.Map<List<FullDiscountActiveProduct>, List<ActiveProductInfo>>(model.Products);
            var products = model.Products.Map<List<ActiveProductInfo>>();

            if (data.Id == 0)
            {
                throw new MallException("错误的活动编号");
            }

            //判断活动是否可添加
            if (!Service.CanOperationActive(data, products))
            {
                throw new MallException("有其他冲突活动存在，不可以完成操作");
            }

            Service.UpdateActive(data, rules, products);
        }
        /// <summary>
        /// 删除活动
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteActive(long id)
        {
            Service.DeleteActive(id);
        }
        #endregion

        #region 其他功能
        /// <summary>
        /// 获取可以参与满减活动的商品集
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="productName"></param>
        /// <param name="productCode"></param>
        /// <param name="categroyId"></param>
        /// <param name="selectedProductIds"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static QueryPageModel<Entities.ProductInfo> GetCanJoinProducts(long shopId
            , string productName = null, string productCode = null, long? categoryId = null, bool? isShopCategory = null
            , IEnumerable<long> selectedProductIds = null
            , int activeId = 0
            , int page = 1, int pagesize = 10)
        {
            return Service.GetCanJoinProducts(shopId, productName, productCode, categoryId, isShopCategory, selectedProductIds, activeId, page, pagesize);
        }
       
        #endregion

        #region 系统
        /// <summary>
        /// 满减营销活动费用设置
        /// </summary>
        /// <returns></returns>
        public static decimal GetMarketServicePrice()
        {
            var marketser = MarketApplication.GetServiceSetting(CurMarketType);
            if (marketser == null)
            {
                marketser = SetMarketServicePrice(0.00m);
            }
            return marketser.Price;
        }
        /// <summary>
        /// 设置满减营销活动费用设置
        /// </summary>
        /// <param name="price"></param>
        public static Entities.MarketSettingInfo SetMarketServicePrice(decimal price)
        {
            Entities.MarketSettingInfo marketser = new Entities.MarketSettingInfo() { TypeId = CurMarketType, Price = price };
            MarketApplication.AddOrUpdateServiceSetting(marketser);
            return marketser;
        }
        /// <summary>
        /// 是否已开启满减营销
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
        /// 获取满减营销服务
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
                    if (market != null && market.Id > 0)
                    {
                        result.EndTime = MarketApplication.GetServiceEndTime(market.Id);
                        result.LastBuyPrice = MarketApplication.GetLastBuyPrice(market.Id);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 是否可以使用满减服务
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
        /// 购买满减服务
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
        public static QueryPageModel<MarketServiceBuyRecordModel> GetMarketServiceBuyList(MarketBoughtQuery query)
        {
            var data = MarketApplication.GetBoughtShopList(query);
            var list = data.Models.Select(d => {
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
            return new QueryPageModel<MarketServiceBuyRecordModel>
            {
                Models = list,
                Total = data.Total
            };
        }
        #endregion
    }
}
