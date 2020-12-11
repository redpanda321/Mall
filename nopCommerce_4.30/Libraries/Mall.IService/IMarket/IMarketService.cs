using Mall.CommonModel;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System;

namespace Mall.IServices
{
    public interface IMarketService : IService
    {
        /// <summary>
        /// 取店铺激活的营销活动
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        ActiveMarketServiceInfo GetMarketService(long shopId, MarketType type);

        ActiveMarketServiceInfo GetMarketService(long id);
        /// <summary>
        /// 获取服务设置
        /// </summary>
        /// <returns></returns>
        MarketSettingInfo GetServiceSetting(MarketType type);

        /// <summary>
        /// 获取指定营销类型服务的已购买商家列表
        /// </summary>
        /// <param name="MarketBoughtQuery">营销查询对象</param>
        /// <returns></returns>
        QueryPageModel<MarketServiceRecordInfo> GetBoughtShopList(MarketBoughtQuery query);

        /// <summary>
        /// 添加或者更新服务设置
        /// </summary>
        void AddOrUpdateServiceSetting(MarketSettingInfo info);

        /// <summary>
        /// 商家订购服务
        /// </summary>
        /// <param name="monthCount"></param>
        /// <param name="shopId"></param>
        /// <param name="type"></param>
       void  OrderMarketService(int monthCount, long shopId,MarketType type);

        /// <summary>
        /// 根据购买记录ID获取服务购买记录
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
       MarketServiceRecordInfo GetShopMarketServiceRecordInfo(long Id);

        /// <summary>
        /// 获取服务终止日期
        /// </summary>
        /// <param name="marketId"></param>
        /// <returns></returns>
        DateTime GetServiceEndTime(long marketId);

        /// <summary>
        /// 获取最近一次购买的价格
        /// </summary>
        /// <param name="marketId"></param>
        /// <returns></returns>
        decimal GetLastBuyPrice(long marketId);

    }
}
