using Mall.DTO.QueryModel;
using Mall.CommonModel;
using Mall.Entities;
using System.Collections.Generic;

namespace Mall.IServices
{
    public interface IAppMessageService : IService
    {
        /// <summary>
        /// 商家未读消息数（30天内）
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        int GetShopNoReadMessageCount(long shopId);
        /// <summary>
        /// 门店未读消息数（30天内）
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        int GetBranchNoReadMessageCount(long shopBranchId);
        /// <summary>
        /// 门店未读消息数（30天内）
        /// </summary>
        /// <param name="branchs"></param>
        /// <returns></returns>
        Dictionary<long, int> GetBranchNoReadMessageCount(List<long> branchs);
        /// <summary>
        /// 获取消息列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<AppMessageInfo> GetMessages(AppMessageQuery query);
        /// <summary>
        /// 消息状态改已读
        /// </summary>
        /// <param name="id"></param>
        void ReadMessage(long id);
        /// <summary>
        /// 新增App消息
        /// </summary>
        /// <param name="appMessagesInfo"></param>
        void AddAppMessages(AppMessageInfo appMessagesInfo);
    }
}
