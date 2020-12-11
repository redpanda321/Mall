using Mall.CommonModel;
using Mall.DTO;
using System.Collections.Generic;

namespace Mall.IServices
{
    public  interface IWeiActivityWinService : IService
    {
        /// <summary>
        ///  查询中奖信息
        /// </summary>
        QueryPageModel<WeiActivityWinModel> Get(string text,long id,int pageIndex, int pageSize);

        /// <summary>
        /// 添加中奖信息
        /// </summary>
        /// <param name="info">中奖信息实体</param>
        void AddWinner(Entities.WeiActivityWinInfoInfo info);
        /// <summary>
        /// 查询中奖人数
        /// </summary>
        /// <param name="activityId">活动id</param>
        /// <param name="text">查询条件</param>
        /// <returns>人数</returns>
        string GetWinNumber(long activityId, string text);

        /// <summary>
        /// 查询用户中奖信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        List<Entities.WeiActivityWinInfoInfo> GetWinInfo(long userId);
    }
}
