using Mall.CommonModel;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;

namespace Mall.Service
{
    public class OperationLogService : ServiceBase, IOperationLogService
    {
        public QueryPageModel<LogInfo> GetPlatformOperationLogs(OperationLogQuery query)
        {
            var logs = DbFactory.Default.Get<LogInfo>().Where(item => item.ShopId == query.ShopId);
            if (!string.IsNullOrWhiteSpace(query.UserName))
            {
                logs.Where(item => query.UserName == item.UserName);
            }
            if (query.StartDate.HasValue)
            {
                logs.Where(item => item.Date >= query.StartDate.Value);
            }
            if (query.EndDate.HasValue)
            {
                var end = query.EndDate.Value.AddDays(1);
                logs.Where(item => item.Date <= end);
            }
            var rets = logs.OrderByDescending(o => o.Id).ToPagedList(query.PageNo, query.PageSize);
            QueryPageModel<LogInfo> pageModel = new QueryPageModel<LogInfo>() { Models = rets, Total = rets.TotalRecordCount };
            return pageModel;
        }

        public void AddPlatformOperationLog(LogInfo model)
        {
            model.ShopId = 0;
            DbFactory.Default.Add(model);
        }

        public void AddSellerOperationLog(LogInfo model)
        {
            if (model.ShopId != 0)
            {
                model.ShopId = model.ShopId;
                DbFactory.Default.Add(model);
            }
            else
            {
                throw new Mall.Core.MallException("日志获取店铺ID错误");
            }
        }


        public void DeletePlatformOperationLog(long id)
        {
            throw new NotImplementedException();
        }

        public bool ExistUrl(string pageUrl, long shopId)
        {
            return DbFactory.Default.Get<LogInfo>().Where(a => a.PageUrl == pageUrl && a.ShopId == shopId).Exist();
        }
    }
}
