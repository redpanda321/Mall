using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    public class OperationLogApplication
    {
       // private static IOperationLogService _iOperationLogService =  EngineContext.Current.Resolve<IOperationLogService>();

        private static IOperationLogService _iOperationLogService =  EngineContext.Current.Resolve<IOperationLogService>();


        /// <summary>
        /// 根据查询条件分页获取操作日志信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<Entities.LogInfo> GetPlatformOperationLogs(OperationLogQuery query)
        {
            return _iOperationLogService.GetPlatformOperationLogs(query);
        }

        /// <summary>
        /// 增加平台日志信息
        /// </summary>
        /// <param name="info"></param>
        public static void AddPlatformOperationLog(Entities.LogInfo info)
        {
            _iOperationLogService.AddPlatformOperationLog(info);
        }
        /// <summary>
        /// 增加平台日志信息
        /// </summary>
        /// <param name="info"></param>
        public static void AddSellerOperationLog(Entities.LogInfo info)
        {
            _iOperationLogService.AddSellerOperationLog(info);
        }

        /// <summary>
        ///根据ID删除平台日志信息
        /// </summary>
        /// <param name="id"></param>
        public static void DeletePlatformOperationLog(long id)
        {
            _iOperationLogService.DeletePlatformOperationLog(id);
        }
        public static bool ExistUrl(string pageUrl,long shopId)
        {
            return _iOperationLogService.ExistUrl(pageUrl,shopId);
        }
    }
}
