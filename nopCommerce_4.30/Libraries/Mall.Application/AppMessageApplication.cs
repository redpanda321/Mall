using Mall.IServices;
using Mall.Core;
using Mall.DTO;
using Mall.Entities;
using Mall.DTO.QueryModel;
using Mall.CommonModel;
using System.Collections.Generic;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    public class AppMessageApplication
    {
    
        private static IAppMessageService _appMessageService =  EngineContext.Current.Resolve<IAppMessageService>();


        /// <summary>
        /// 未读消息数（30天内）
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static int GetShopNoReadMessageCount(long shopId)
        {
            return _appMessageService.GetShopNoReadMessageCount(shopId);
        }
        /// <summary>
        /// 未读消息数（30天内）
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public static int GetBranchNoReadMessageCount(long shopBranchId)
        {
            return _appMessageService.GetBranchNoReadMessageCount(shopBranchId);
        }

        public static Dictionary<long, int> GetBranchNoReadMessageCount(List<long> branchs)
        {
            return _appMessageService.GetBranchNoReadMessageCount(branchs);
        }

        /// <summary>
        /// 获取消息列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<AppMessageInfo> GetMessages(AppMessageQuery query)
        {
            return _appMessageService.GetMessages(query);
        }
        /// <summary>
        /// 消息状态改已读
        /// </summary>
        /// <param name="id"></param>
        public static void ReadMessage(long id)
        {
            _appMessageService.ReadMessage(id);
        }
        /// <summary>
        /// 新增消息
        /// </summary>
        /// <param name="appMessagesInfo"></param>
        public static void AddAppMessages(AppMessages appMessages)
        {
           // AutoMapper.Mapper.CreateMap<AppMessages, AppMessageInfo>();
            var appMessagesInfo = appMessages.Map<AppMessageInfo>();
            _appMessageService.AddAppMessages(appMessagesInfo);
        }
    }
}
