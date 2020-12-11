using Mall.CommonModel;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System;
using System.Collections.Generic;

namespace Mall.IServices
{
    public interface IStatisticsService : IService
    {
        /// <summary>
        /// 获取会员新增数据(日期)
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        List<ChartDataItem<DateTime,int>> GetMemberChart(DateTime begin, DateTime end);
        List<ChartDataItem<long, int>> GetAreaMapByOrderCount(DateTime begin, DateTime end);
        List<ChartDataItem<long, decimal>> GetAreaMapByOrderAmount(DateTime begin, DateTime end);

        /// <summary>
        /// 获取新增店铺图表
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns></returns>
        List<ChartDataItem<DateTime, int>> GetNewShopChart(DateTime begin, DateTime end);

        /// <summary>
        /// 获取店铺订单量排行数据
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="rankSize"></param>
        /// <returns>[ShopId,OrderCount,ShopName]</returns>
        List<ChartDataItem<long, int>> GetShopRankingByOrderCount(DateTime begin, DateTime end, int rankSize);
        /// <summary>
        /// 获取店铺销售额排行数据
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="rankSize"></param>
        /// <returns>[ShopId,SaleAmount,ShopName]</returns>
        List<ChartDataItem<long, decimal>> GetShopRankingBySaleAmount(DateTime begin, DateTime end, int rankSize);
        /// <summary>
        /// 获取店铺流量数据
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        List<ChartDataItem<DateTime, int>> GetShopFlowChart(long shop, DateTime begin, DateTime end);
        /// <summary>
        /// 获取门店转化率(SaleCounts/VistiCounts)
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        List<ChartDataItem<DateTime, decimal>> GetConversionRate(long shop, DateTime begin, DateTime end);
        /// <summary>
        /// 获取商品销量数据
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="rankSize"></param>
        /// <returns>[ProductId,SaleCount,ProductName]</returns>
        List<ChartDataItem<long, int>> GetProductChartBySaleCount(long shop, DateTime begin, DateTime end, int rankSize);

        /// <summary>
        /// 获取商品销量数据
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="rankSize"></param>
        /// <returns>[ProductId,SaleAmount,ProductName]</returns>
        List<ChartDataItem<long, decimal>> GetProductChartBySaleAmount(long shop, DateTime begin, DateTime end, int rankSize);

        /// <summary>
        /// 获取商品销量数据
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="rankSize"></param>
        /// <returns>[ProductId,Visti,ProductName]</returns>
        List<ChartDataItem<long, long>> GetProductChartByVisti(long shop, DateTime begin, DateTime end, int rankSize);

        /// <summary>
        /// 获取门店销量数据
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        List<ChartDataItem<DateTime, int>> GetShopBySaleCount(long shop, DateTime begin, DateTime end);

        /// <summary>
        /// 添加平台浏览量(人数 UV)
        /// </summary>
        /// <param name="dt"></param>
        void AddPlatVisitUser(DateTime dt,long num);
        /// <summary>
        /// /添加店铺浏览量(人数 UV)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="shopId"></param>
        void AddShopVisitUser(DateTime dt, long shopId, long num);

        /// <summary>
        /// 添加门店浏览量(人数 UV)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="shopId"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="num"></param>
        void AddShopBranchVisitUser(DateTime dt, long shopId, long shopBranchId, long num);

        /// <summary>
        /// 添加商品浏览量(人数 UV)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="pid"></param>
        void AddProductVisitUser(DateTime dt, long pid, long shopId, long num);
        /// <summary>
        /// 添加商品浏览量(PV)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="pid"></param>
        void AddProductVisit(DateTime dt, long pid, long shopId, long num);
        /// <summary>
        /// 取时间段内商品浏览记录
        /// </summary>
        /// <param name="startDt"></param>
        /// <param name="endDt"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        QueryPageModel<ProductStatisticModel> GetProductVisits(ProductStatisticQuery query);
      
        /// <summary>
        /// 获取商品销售统计(按分类分组)
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        List<ProductCategoryStatistic> GetProductCategoryStatistic(long shop, DateTime begin, DateTime end);
        /// <summary>
        /// 取时间段内店铺浏览记录
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="shop"></param>
        /// <returns></returns>
        List<TradeStatisticModel> GetShopVisits(long shop,long? shopbranchId, DateTime begin, DateTime end);

        /// <summary>
        /// 取时间段内店铺实时数据
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="shopbranchId"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        List<TradeStatisticModel> GetShopVisitsByRealTime(long shop, long? shopbranchId, DateTime begin, DateTime end);
        /// <summary>
        /// 取时间段内平台访问记录
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        List<TradeStatisticModel> GetPlatVisits(DateTime begin, DateTime end);
    }
}
