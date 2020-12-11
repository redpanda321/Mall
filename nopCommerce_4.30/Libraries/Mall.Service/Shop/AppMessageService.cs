using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System;
using Mall.CommonModel;
using NetRube.Data;
using System.Collections.Generic;
using System.Linq;
namespace Mall.Service
{
    public class AppMessageService : ServiceBase, IAppMessageService
    {
        /// <summary>
        /// 商家未读消息数（30天内）
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public int GetShopNoReadMessageCount(long shopId)
        {
            var starttime = DateTime.Now.AddDays(-30).Date;
            //return Context.AppMessagesInfo.Where(d => d.ShopId == shopId && d.IsRead == false && d.sendtime >= starttime).Count();
            return DbFactory.Default.Get<AppMessageInfo>().Where(d => d.ShopId == shopId && d.IsRead == false && d.sendtime >= starttime).Count();
        }
        /// <summary>
        /// 门店未读消息数（30天内）
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public int GetBranchNoReadMessageCount(long shopBranchId)
        {
            var starttime = DateTime.Now.AddDays(-30).Date;
            //return Context.AppMessagesInfo.Where(d => d.ShopBranchId == shopBranchId && d.IsRead == false && d.sendtime >= starttime).Count();
            return DbFactory.Default.Get<AppMessageInfo>().Where(d => d.ShopBranchId == shopBranchId && d.IsRead == false && d.sendtime >= starttime).Count();
        }
        /// <summary>
        /// 门店未读消息数（30天内）
        /// </summary>
        /// <param name="branchs"></param>
        /// <returns></returns>
        public Dictionary<long, int> GetBranchNoReadMessageCount(List<long> branchs)
        {
            var time = DateTime.Now.AddDays(-30).Date;
            return DbFactory.Default.Get<AppMessageInfo>().Where(d => d.ShopBranchId.ExIn(branchs) && d.IsRead == false && d.sendtime >= time)
                .GroupBy(p => p.ShopBranchId)
                .Select(p => new { ShopBranchId = p.ShopBranchId, Count = p.ExCount(false) })
                .ToList<dynamic>()
                .ToDictionary(k => (long)k.ShopBranchId, v => (int)v.Count);
        }

        /// <summary>
        /// 获取消息列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<AppMessageInfo> GetMessages(AppMessageQuery query)
        {
            //var sql = Context.AppMessagesInfo.AsQueryable();
            var sql = DbFactory.Default.Get<AppMessageInfo>();
            if (query.ShopId.HasValue)
            {
                sql.Where(d => d.ShopId == query.ShopId.Value);
            }
            if (query.ShopBranchId.HasValue)
            {
                sql.Where(d => d.ShopBranchId == query.ShopBranchId.Value);
            }
            if (query.StartTime.HasValue)
            {
                sql.Where(d => d.sendtime >= query.StartTime.Value);
            }
            if (query.EndTime.HasValue)
            {
                sql.Where(d => d.sendtime <= query.EndTime.Value);
            }
            if (query.IsRead.HasValue)
            {
                sql.Where(d => d.IsRead == query.IsRead.Value);
            }

            QueryPageModel<AppMessageInfo> result = new QueryPageModel<AppMessageInfo>();
            //int total = 0;
            //result.Models = sql.GetPage(o => o.OrderByDescending(d => d.sendtime), out total, query.PageNo, query.PageSize).ToList();
            var models = sql.OrderByDescending(d => d.sendtime).ToPagedList(query.PageNo, query.PageSize);
            result.Models = models;
            result.Total = models.TotalRecordCount;
            return result;
        }
        /// <summary>
        /// 消息状态改已读
        /// </summary>
        /// <param name="id"></param>
        public void ReadMessage(long id)
        {
            //var msg = Context.AppMessagesInfo.FirstOrDefault(d => d.Id == id);
            //var msg = DbFactory.Default.Get<AppMessageInfo>().Where(d => d.Id == id).FirstOrDefault();
            //if (msg != null)
            //{
            //    msg.IsRead = true;
            //Context.SaveChanges();
            DbFactory.Default.Set<AppMessageInfo>().Set(n => n.IsRead, true).Where(p => p.Id == id).Succeed();
            //}
        }
        /// <summary>
        /// 新增门店App消息
        /// </summary>
        /// <param name="appMessagesInfo"></param>
        public void AddAppMessages(AppMessageInfo appMessagesInfo)
        {
            //Context.AppMessagesInfo.Add(appMessagesInfo);
            //Context.SaveChanges();
            DbFactory.Default.Add(appMessagesInfo);
        }
    }
}
