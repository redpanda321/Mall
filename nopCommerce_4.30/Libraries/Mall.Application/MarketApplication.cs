using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using System;

namespace Mall.Application
{
    public class MarketApplication : BaseApplicaion<IMarketService>
    {
        /// <summary>
        /// 取店铺激活的营销活动
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ActiveMarketServiceInfo GetMarketService(long shopId, MarketType type)
        {
            return Service.GetMarketService(shopId, type);
        }

        public static ActiveMarketServiceInfo GetMarketService(long id)
        {
            return Service.GetMarketService(id);
        }


        /// <summary>
        /// 获取服务设置
        /// </summary>
        /// <returns></returns>
        public static MarketSettingInfo GetServiceSetting(MarketType type)
        {
            return Service.GetServiceSetting(type);
        }

        /// <summary>
        /// 获取指定营销类型服务的已购买商家列表
        /// </summary>
        /// <param name="MarketBoughtQuery">营销查询对象</param>
        /// <returns></returns>
        public static QueryPageModel<MarketServiceRecordInfo> GetBoughtShopList(MarketBoughtQuery query)
        {
            return Service.GetBoughtShopList(query);
        }

        /// <summary>
        /// 添加或者更新服务设置
        /// </summary>
        public static void AddOrUpdateServiceSetting(MarketSettingInfo info)
        {
            Service.AddOrUpdateServiceSetting(info);
        }

        /// <summary>
        /// 商家订购服务
        /// </summary>
        /// <param name="monthCount"></param>
        /// <param name="shopId"></param>
        /// <param name="type"></param>
        public static void OrderMarketService(int monthCount, long shopId, MarketType type)
        {
            Service.OrderMarketService(monthCount, shopId, type);
        }

        /// <summary>
        /// 根据购买记录ID获取服务购买记录
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static MarketServiceRecordInfo GetShopMarketServiceRecordInfo(long Id)
        {
            return Service.GetShopMarketServiceRecordInfo(Id);
        }

        public static DateTime GetServiceEndTime(long marketId)
        {
            return Service.GetServiceEndTime(marketId);
        }
        public static decimal GetLastBuyPrice(long marketId)
        {
            return Service.GetLastBuyPrice(marketId);
        }
    }
}
