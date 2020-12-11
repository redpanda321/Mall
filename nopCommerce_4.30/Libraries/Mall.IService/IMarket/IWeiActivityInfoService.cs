using Mall.CommonModel;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System.Collections.Generic;

namespace Mall.IServices
{
    public interface IWeiActivityInfoService : IService
    {
        /// <summary>
        ///  查询刮刮卡
        /// </summary>
        QueryPageModel<WeiActivityInfoInfo> Get(WeiActivityQuery query);

        /// <summary>
        ///  添加刮刮卡
        /// </summary>
        long AddActivity(WeiActivityInfoInfo model, List<WeiActivityAwardInfo> awardInfos);

        /// <summary>
        ///  修改刮刮卡
        /// </summary>
        //long UpdateActivity(WeiActivityInfo model);

        /// <summary>
        /// 删除刮刮卡
        /// </summary>
        void DeleteActivity(long id);
        List<WeiActivityWinInfoInfo> GetActivityWin(long activity);
        List<WeiActivityAwardInfo> GetActivityAwards(long activity);

        ///// <summary>
        ///// 获取刮刮卡
        ///// </summary>
        Entities.WeiActivityInfoInfo GetActivityModel(long id, string url = "");
        /// <summary>
        /// 查询是否全部抽完
        /// </summary>
        /// <param name="id">活动Id</param>
        /// <param name="awardId">奖等Id</param>
        /// <returns></returns>
        //int GetProportion(long id, long awardId);
    }
}
