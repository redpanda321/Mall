using System;
using System.Collections;
using System.Web.Http;
using Mall.CommonModel;
using Mall.Application;
using Mall.API.Model;
using Mall.DTO.QueryModel;
using Mall.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Mall.API
{
    public class ShopBilityController : BaseShopLoginedApiController
    {
        /// <summary>
        /// 获取店铺业绩能力数据
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <param name="shopId">门店id</param>
        /// <returns></returns>
        /// 

        [HttpGet("Get")]
        public TradeStatisticModel Get(DateTime? startDate = null, DateTime? endDate = null)
        {
            CheckShopManageLogin();
            var shop = CurrentShop;
            var ShopId = shop.Id;
            var yesterday = DateTime.Now.Date.AddDays(-1);
            var type = 0;
            if (!startDate.HasValue || !endDate.HasValue)
            {
                type = 1;//实时数据
                startDate = DateTime.Now.Date;
                endDate = DateTime.Now;
            }
            var platTradeStatistic = StatisticApplication.GetShopTradeStatistic(ShopId, null, startDate.Value, endDate.Value, type);
            return platTradeStatistic;
        }

        /// <summary>
        /// 获取店铺下门店的业绩排行
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetBranchShopFeat")]
        public DataGridModel<BranchShopFeat> GetBranchShopFeat(DateTime? startDate = null, DateTime? endDate = null,int pageNo = 1, int pageSize = 10)
        {
            if (!startDate.HasValue)
            {
                startDate = DateTime.Now.Date;
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.Now;
            }
            else
                endDate = endDate.Value.Date.AddDays(1);
            CheckShopManageLogin();
            var shop = CurrentShop;
            long shopId = shop.Id;
            BranchShopFeatsQuery query = new BranchShopFeatsQuery();
            query.EndDate = endDate;
            query.StartDate = startDate;
            query.ShopId = shopId;
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            var model = OrderAndSaleStatisticsApplication.GetBranchShopFeat(query);
            DataGridModel<BranchShopFeat> result = new DataGridModel<BranchShopFeat>()
            {
                rows = model.Models,
                total = model.Total
            };
            return result;
        }
    }
}
