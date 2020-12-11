using Mall.CommonModel;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System.Collections.Generic;

namespace Mall.IServices
{

    public interface IMemberIntegralConversionFactoryService : IService
    {
        IConversionMemberIntegralBase Create(MemberIntegralInfo.IntegralType type, int use = 0);
    }

    public interface IMemberIntegralService: IService
    {
        /// <summary>
        ///  //用户积分记录增加
        /// </summary>
        /// <param name="model"></param>
        /// <param name="conversionMemberIntegralEntity"></param>
        void AddMemberIntegral(MemberIntegralRecordInfo model, IConversionMemberIntegralBase conversionMemberIntegralEntity = null);
        /// <summary>
        /// 增加用户积分记录，不增加历史积分
        /// </summary>
        /// <param name="model"></param>
        /// <param name="conversionMemberIntegralEntity"></param>
        void AddMemberIntegralNotAddHistoryIntegrals(MemberIntegralRecordInfo model, IConversionMemberIntegralBase conversionMemberIntegralEntity = null);
        /// <summary>
        /// 通过多个RecordAction，增加用户积分
        /// </summary>
        /// <param name="model"></param>
        /// <param name="conversionMemberIntegralEntity"></param>
        void AddMemberIntegralByRecordAction(MemberIntegralRecordInfo model, IConversionMemberIntegralBase conversionMemberIntegralEntity = null);

        /// <summary>
        /// 获取用户积分列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<MemberIntegralInfo> GetMemberIntegralList(IntegralQuery query);


        /// <summary>
        /// 根据用户ID获取用户的积分信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        MemberIntegralInfo GetMemberIntegral(long userId);


        /// <summary>
        /// 根据用户ID获取用户的积分信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<MemberIntegralInfo> GetMemberIntegrals(IEnumerable<long> userIds);

        /// <summary>
        /// 获取单个用户的积分记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<MemberIntegralRecordInfo> GetIntegralRecordList(IntegralRecordQuery query);

        /// <summary>
        /// 获取单个用户的积分记录,前台使用
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<MemberIntegralRecordInfo> GetIntegralRecordListForWeb(IntegralRecordQuery query);

        /// <summary>
        ///是否有过登录积分记录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool HasLoginIntegralRecord(long userId);
        /// <summary>
        /// 根据订单号获取晒单积分记录
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        List<MemberIntegralRecordActionInfo> GetIntegralRecordAction(IEnumerable<long> virtualItemIds, MemberIntegralInfo.VirtualItemType type);

        List<MemberIntegralRecordActionInfo> GetIntegralRecordAction(long record);

        /// <summary>
        /// 设置积分规则
        /// </summary>
        /// <param name="info"></param>
        void SetIntegralRule(IEnumerable<MemberIntegralRuleInfo> info);

        /// <summary>
        /// 设置积分兑换规则
        /// </summary>
        void SetIntegralChangeRule(MemberIntegralExchangeRuleInfo info);

        /// <summary>
        /// 获取积分兑换规则
        /// </summary>
        /// <returns></returns>
        MemberIntegralExchangeRuleInfo GetIntegralChangeRule();

        /// <summary>
        /// 获取积分规则
        /// </summary>
        /// <returns></returns>
        List<MemberIntegralRuleInfo> GetIntegralRule();

        /// <summary>
        /// 获取累计积分分组
        /// </summary>
        /// <returns></returns>
        UserIntegralGroupModel GetUserHistroyIntegralGroup(long userId);
    }
}
