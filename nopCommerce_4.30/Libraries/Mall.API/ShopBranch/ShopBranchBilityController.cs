using Mall.API.Model;
using Mall.Application;
using Mall.CommonModel;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Mall.API
{
    public class ShopBranchBilityController : BaseShopBranchLoginedApiController
    {
        /// <summary>
        /// 获取门店业绩能力数据
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <param name="shopId">门店id</param>
        /// <returns></returns>
        /// 
        [HttpGet("Get")]
        public TradeStatisticModel Get(DateTime? startDate = null, DateTime? endDate = null)
        {
            
            CheckUserLogin();
            var yesterday = DateTime.Now.Date.AddDays(-1);
            var type = 0;
            if (!startDate.HasValue || !endDate.HasValue)
            {
                type = 1;//实时数据
                startDate = DateTime.Now.Date;
                endDate = DateTime.Now;
            }
            var platTradeStatistic = StatisticApplication.GetShopTradeStatistic(CurrentShopBranch.ShopId, CurrentShopBranch.Id, startDate.Value, endDate.Value, type);
            return platTradeStatistic;
        }

        /// <summary>
        /// 获取店铺下某个门店的每天的业绩排行
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetBranchShopFeat")]
        public DataGridModel<BranchShopDayFeat> GetBranchShopFeat(DateTime? startDate = null, DateTime? endDate = null, int pageNo =1,int pageSize=10)
        {
            if (!startDate.HasValue)
            {
                startDate = DateTime.Now.Date.AddDays(-7);
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.Now.Date;
            }
            CheckUserLogin();
            if (startDate.HasValue && startDate.Value < CurrentShopBranch.CreateDate)
            {
                startDate = CurrentShopBranch.CreateDate.Date;
            }
            BranchShopDayFeatsQuery query = new BranchShopDayFeatsQuery();
            query.StartDate = startDate.Value;
            query.EndDate = endDate.Value;
            query.ShopId = CurrentShopBranch.ShopId;
            query.BranchShopId = CurrentShopBranch.Id;
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            var model = Mall.Application.OrderAndSaleStatisticsApplication.GetDayAmountSale(query);
            DataGridModel<BranchShopDayFeat> result = new DataGridModel<BranchShopDayFeat>()
            {
                rows = model.Models,
                total = model.Total
            };
            return result;
        }
    }
}
