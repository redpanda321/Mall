using Mall.CommonModel;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System;
using System.Collections.Generic;

namespace Mall.IServices
{
    /// <summary>
    /// 拼团服务
    /// </summary>
    public interface IFightGroupService : IService
    {
        #region 拼团活动
        /// <summary>
        /// 新增拼团活动
        /// </summary>
        /// <param name="data"></param>
        void AddActive(FightGroupActiveInfo data);
        /// <summary>
        /// 商品是否可以参加拼团活动
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        bool ProductCanJoinActive(long productId);
        /// <summary>
        /// 更新拼团活动
        /// </summary>
        /// <param name="data"></param>
        void UpdateActive(FightGroupActiveInfo data);
        /// <summary>
        /// 下架拼团活动
        /// </summary>
        /// <param name="id"></param>
        /// <param name="manageRemark">下架原因</param>
        /// <param name="manageId">管理员编号</param>
        void CancelActive(long id, string manageRemark, long manageId);
        /// <summary>
        /// 根据商品ID取活动信息
        /// </summary>
        /// <param name="proId"></param>
        /// <returns></returns>
        FightGroupActiveInfo GetActiveByProId(long proId);

        FightGroupActiveInfo GetFightGroupActiveInfo(long id);

        /// <summary>
        /// 获取拼团活动
        /// </summary>
        /// <param name="id"></param>
        /// <param name="needGetProductCommentNumber">是否需要同步获取商品的评价数量,会自动加载产品信息</param>
        /// <param name="isLoadItems">是否加载节点信息</param>
        /// <param name="isLoadPorductInfo">是否加载产品信息</param>
        /// <returns></returns>
        Entities.FightGroupActiveInfo GetActive(long id, bool needGetProductCommentNumber = false, bool isLoadItems = true, bool isLoadPorductInfo = true);
        List<Entities.FightGroupActiveInfo> GetActive(long[] ids);
        /// <summary>
        /// 使用商品编号获取正在进行的拼团活动编号
        /// <para>0表示无数据</para>
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        long GetActiveIdByProductId(long productId);

        FightGroupActiveInfo GetActiveIdByProductIdAndShopId(long productId, long shopId);
        List<long> GetActiveIdByProductIds(long[] productIds);
        /// <summary>
        /// 获取拼团活动项
        /// </summary>
        /// <param name="activeId"></param>
        /// <returns></returns>
        List<Entities.FightGroupActiveItemInfo> GetActiveItems(long activeId);
        /// <summary>
        /// 获取活动信息集
        /// </summary>
        /// <returns></returns>
        QueryPageModel<FightGroupActiveInfo> GetActives(FightGroupActiveQuery query);
        #endregion

        #region 拼团详情
        /// <summary>
        /// 获取拼团
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="groupId">团编号</param>
        /// <returns></returns>
        FightGroupInfo GetGroup(long activeId, long groupId);
        /// <summary>
        /// 获取拼团详情列表
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="Statuses">状态集</param>
        /// <param name="StartTime">开始时间</param>
        /// <param name="EndTime">结束时间</param>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        QueryPageModel<FightGroupInfo> GetGroups(
        long activeId,
        List<FightGroupBuildStatus> Statuses = null,
        DateTime? StartTime = null,
        DateTime? EndTime = null,
        int PageNo = 1,
        int PageSize = 10
        );
        List<FightGroupInfo> GetCanJoinGroupsFirst(List<FightGroupBuildStatus> Statuses, int PageNo = 1, int PageSize = 5);
        List<FightGroupInfo> GetCanJoinGroupsSecond(long[] unActiveId, List<FightGroupBuildStatus> Statuses);
        /// <summary>
        /// 获取参与的拼团
        /// </summary>
        /// <param name="userId">用户编号</param>
        /// <param name="Statuses">参与状态</param>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        QueryPageModel<FightGroupInfo> GetJoinGroups(
            long userId
            , List<FightGroupOrderJoinStatus> Statuses = null
            , int PageNo = 1
            , int PageSize = 10
            );
        /// <summary>
        /// 设定拼团失败
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="groupId">团编号</param>
        void SetGroupFailed(long activeId, long groupId);
        /// <summary>
        /// 自动关闭过期拼团
        /// </summary>
        void AutoCloseGroup();
        /// <summary>
        /// 开团
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="userId">用户编号</param>
        /// <returns></returns>
        FightGroupInfo AddGroup(long activeId, long userId);
        /// <summary>
        /// 参团
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="groupId">团组编号</param>
        /// <returns></returns>
        FightGroupInfo JoinGroup(long activeId, long groupId);
        /// <summary>
        /// 是否可以参团
        /// </summary>
        /// <param name="activeId"></param>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool CanJoinGroup(long activeId, long groupId, long userId);
        /// <summary>
        /// 检测并更新拼团状态
        /// </summary>
        /// <param name="activeId"></param>
        /// <param name="groupId"></param>
        void CheckAndUpdateGroupStatus(long activeId, long groupId);
        #endregion

        #region 拼团订单
        /// <summary>
        /// 根据拼团活动Id和团组Id获取用户
        /// </summary>
        /// <param name="activeId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        List<FightGroupOrderInfo> GetActiveUsers(long activeId, long groupId);
        /// <summary>
        /// 用户在营销活动中已购买数量
        /// </summary>
        /// <param name="activeId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        long GetMarketSaleCountForUserId(long activeId, long userId);
        /// <summary>
        /// 获取用户拼团商品的已购买数量（商品所有拼团活动）
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Dictionary<long, long> GetMarketSaleCountForProductIdAndUserId(IEnumerable<long> productIds, long userId);
        /// <summary>
        /// 根据用户id获取拼团订单
        /// </summary>
        /// <param name="userID">用户id</param>
        /// <returns></returns>
        QueryPageModel<FightGroupOrderInfo> GetFightGroupOrderByUser(int PageNo, int PageSize, long userID, List<FightGroupOrderJoinStatus> status = null);
        /// <summary>
        /// 设定加入拼团状态
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="status">状态</param>
        FightGroupOrderJoinStatus SetOrderStatus(long orderId, FightGroupOrderJoinStatus status);
        /// <summary>
        /// 拼团订单
        /// </summary>
        /// <param name="actionId">活动编号</param>
        /// <param name="orderId">订单编号</param>
        /// <param name="userId">用户编号</param>
        /// <param name="groupId">拼团编号 0表示开新团</param>
        FightGroupOrderInfo AddOrder(long actionId, long orderId, long userId, long groupId = 0);
        /// <summary>
        /// 发送提示消息
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        void SendMessage(long orderId, FightGroupOrderJoinStatus status);
        /// <summary>
        /// 根据订单流水Id获取拼团订单详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        FightGroupOrderInfo GetFightGroupOrderById(long id);
        /// <summary>
        /// 根据原订单号获取拼团订单信息
        /// </summary>
        /// <param name="orderId">原订单号</param>
        /// <returns></returns>
        FightGroupOrderInfo GetFightGroupOrderStatusByOrderId(long orderId);
        /// <summary>
        /// 订单是否可以支付
        /// <para>成团成功后，未完成支付的订单不可付款</para>
        /// <para>成团失败后，未完成支付的订单不可付款</para>
        /// </summary>
        /// <param name="orderId">订单编号</param>
        /// <returns></returns>
        bool OrderCanPay(long orderId);
        /// <summary>
        /// 获取拼团订单
        /// </summary>
        /// <param name="orderId">订单编号</param>
        /// <returns></returns>
        FightGroupOrderInfo GetOrder(long orderId);
        /// <summary>
        /// 获取参团中的订单数
        /// </summary>
        /// <param name="userId">用户编号</param>
        /// <returns></returns>
        int CountJoiningOrder(long userId);
        /// <summary>
        /// 新增拼团订单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        long AddGroupOrder(FightGroupOrderInfo data);
        /// <summary>
        /// 付款成功后更新拼团订单状态
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        void UpdateGroupOrderStatus(FightGroupOrderInfo data);
        /// <summary>
        /// 根据团组Id获取订单数据
        /// </summary>
        /// <param name="statuses"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        List<FightGroupOrderInfo> GetFightGroupOrderByGroupId(List<FightGroupOrderJoinStatus> statuses, long groupId);
        #endregion
        /// <summary>
        /// 修改活动库存
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="skuId"></param>
        /// <param name="stockChange">库存量 正数补充 负数消耗</param>
        void UpdateActiveStock(long actionId, string skuId, long stockChange);

        List<FightGroupPrice> GetFightGroupPrice();

        /// <summary>
        /// 根据拼团活动Id获取拼图销量
        /// </summary>
        /// <param name="activeId">拼团活动Id</param>
        /// <returns></returns>
        long GetFightGroupSaleVolumeByActiveId(long activeId);

    }
}
