using Mall.CommonModel;
using Mall.CommonModel;
using Mall.DTO;
using Mall.DTO.Distribution;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System;
using System.Collections.Generic;

namespace Mall.IServices
{
    /// <summary>
    /// 分销服务
    /// </summary>
    public interface IDistributionService : IService
    {
        #region 销售员
        /// <summary>
        /// 添加销售员
        /// </summary>
        void AddDistributor(DistributorInfo data);
        /// <summary>
        /// 获取销售员
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        DistributorInfo GetDistributor(long memberId);

        /// <summary>
        /// 获取销售员基本信息
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        DistributorInfo GetDistributorBase(long member);
        decimal GetNoSettlementAmount(long member);

        /// <summary>
        /// 获取响应层级下属
        /// </summary>
        /// <param name="member"></param>
        /// <param name="level"></param>
        /// <returns><level,[members]></returns>
        Dictionary<int, List<long>> GetSubordinate(long member, int level);

        /// <summary>
        /// 获取下级业绩
        /// </summary>
        /// <returns></returns>
        DistributionAchievement GetSubAchievement(long memberId);

        /// <summary>
        /// 获取会员业绩
        /// </summary>
        /// <param name="members"></param>
        /// <returns></returns>
        List<DistributionAchievement> GetAchievement(List<long> members);

        /// <summary>
        /// 获取最后一次生成的报表
        /// </summary>
        /// <returns></returns>
        DistributionRankingBatchInfo GetLastRankingBatch();
        /// <summary>
        /// 生成排行
        /// </summary>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        void GenerateRanking(DateTime beginDate, DateTime endDate);
        /// <summary>
        /// 删除排行报表
        /// </summary>
        /// <param name="batchs"></param>
        void RemoveRankingBatch(List<long> batchs);
        /// <summary>
        /// 获取所有排行报表
        /// </summary>
        /// <returns></returns>
        List<DistributionRankingBatchInfo> GetRankingBatchs();
        /// <summary>
        /// 获取排行数据
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<DistributionRankingInfo> GetRankings(DistributorRankingQuery query);
        /// <summary>
        /// 获取排行数据（忽略分页）
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        List<DistributionRankingInfo> GetRankingsAll(DistributorRankingQuery query);



        /// <summary>
        /// 获取销售员列表
        /// </summary>
        QueryPageModel<DistributorInfo> GetDistributors(DistributorQuery query);
        /// <summary>
        /// 获取销售员列表(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        List<DistributorInfo> GetDistributorsAll(DistributorQuery query);
        /// <summary>
        /// 获取销售员
        /// </summary>
        /// <param name="members"></param>
        /// <returns></returns>
        List<DistributorInfo> GetDistributors(List<long> members);

        /// <summary>
        /// 获取佣金管理列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<DistributorInfo> GetNewDistributors(DistributorSubQuery query);

        /// <summary>
        /// 获取佣金管理列表(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        List<DistributorInfo> GetNewDistributorsAll(DistributorSubQuery query);

        /// <summary>
        /// 修改销售员信息
        /// </summary>
        /// <param name="data"></param>
        void UpdateDistributor(DistributorInfo data);
        /// <summary>
        /// 获取销售员的直接下级数量
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        int GetDistributorSubNumber(long memberId);
        /// <summary>
        /// 实时同步销售员的直接下级数量
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        void SyncDistributorSubNumber(long memberId);
        /// <summary>
        /// 清退销售员
        /// </summary>
        /// <param name="memberIds"></param>
        void RemoveDistributor(IEnumerable<long> memberIds);
        /// <summary>
        /// 恢复销售员
        /// </summary>
        /// <param name="memberIds"></param>
        void RecoverDistributor(IEnumerable<long> memberIds);
        /// <summary>
        /// 拒绝销售员申请
        /// </summary>
        /// <param name="memberIds"></param>
        /// <param name="remark"></param>
        void RefuseDistributor(IEnumerable<long> memberIds, string remark);
        /// <summary>
        /// 同意销售员申请
        /// </summary>
        /// <param name="memberIds"></param>
        /// <param name="remark"></param>
        void AgreeDistributor(IEnumerable<long> memberIds, string remark);
        #endregion

        #region 销售员等级
        /// <summary>
        /// 添加销售员等级
        /// </summary>
        /// <param name="data"></param>
        void AddDistributorGrade(DistributorGradeInfo data);
        /// <summary>
        /// 修改销售员等级
        /// </summary>
        /// <param name="data"></param>
        void UpdateDistributorGrade(DistributorGradeInfo data);
        /// <summary>
        /// 删除销售员等级
        /// </summary>
        /// <param name="id"></param>
        void DeleteDistributorGrade(long id);
        /// <summary>
        /// 获取销售员等级
        /// </summary>
        /// <returns></returns>
        DistributorGradeInfo GetDistributorGrade(long id);
        /// <summary>
        /// 获取销售员等级列表
        /// </summary>
        /// <returns></returns>
        List<DistributorGradeInfo> GetDistributorGrades(bool IsAvailable = false);
        /// <summary>
        /// 是否己存在同名等级
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool ExistDistributorGradesName(string name, long id);
        /// <summary>
        /// 是否己存在同条件等级
        /// </summary>
        /// <param name="quota"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        bool ExistDistributorGradesQuota(decimal quota, long id);
        #endregion

        #region 分销商品
        /// <summary>
        /// 获取默认分佣比
        /// </summary>
        /// <param name="shopId"></param>
        DistributionShopRateConfigInfo GetDefaultBrokerageRate(long shopId);
        /// <summary>
        /// 修改默认分佣比
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="data"></param>
        void UpdateDefaultBrokerageRate(long shopId, DistributionShopRateConfigInfo data);
        /// <summary>
        /// 重置非开放等级分佣比
        /// </summary>
        /// <param name="maxLevel"></param>
        void ResetDefaultBrokerageRate(int maxLevel);
        /// <summary>
        /// 获取佣金流水
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<DistributorRecordInfo> GetRecords(DistributorRecordQuery query);
        /// <summary>
        /// 获取提现记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        DistributionWithdrawInfo GetWithdraw(long id);
        /// <summary>
        /// 获取提现记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<DistributionWithdrawInfo> GetWithdraws(DistributionWithdrawQuery query);
        /// <summary>
        /// 获取提现记录(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        List<DistributionWithdrawInfo> GetWithdrawsAll(DistributionWithdrawQuery query);
        /// <summary>
        /// 新增提现记录
        /// </summary>
        /// <param name="model"></param>
        void ApplyWithdraw(DistributionWithdrawInfo model);
        /// <summary>
        /// 审核提现
        /// </summary>
        /// <param name="withdrawId"></param>
        /// <param name="operatorName"></param>
        /// <param name="remark"></param>
        void AuditingWithdraw(long withdrawId, string operatorName, string remark);
        /// <summary>
        /// 支付失败(回滚到待审核状态)
        /// </summary>
        /// <param name="withdrawId"></param>
        /// <param name="remark"></param>
        void FailWithdraw(long withdrawId, string remark);
        void SuccessWithdraw(long withdrawId, string payNo);
        /// <summary>
        /// 拒绝提现
        /// </summary>
        /// <param name="withdrawId"></param>
        /// <param name="operatorName"></param>
        /// <param name="remark"></param>
        void RefusedWithdraw(long withdrawId, string operatorName, string remark);

        /// <summary>
        /// 推广分销商品
        /// </summary>
        /// <param name="productIds"></param>
        void AddSpreadProducts(IEnumerable<long> productIds, long shopId);
        // 取消推广分销商品
        void RemoveSpreadProducts(IEnumerable<long> productIds, long? shopId = null);
        /// <summary>
        /// 设置商品分佣比
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="BrokerageRate1"></param>
        /// <param name="BrokerageRate2"></param>
        /// <param name="BrokerageRate3"></param>
        void SetProductBrokerageRate(long productId, decimal BrokerageRate1, decimal BrokerageRate2 = 0, decimal BrokerageRate3 = 0);
        /// <summary>
        /// 获取分销商品
        /// </summary>
        DistributionProduct GetProduct(long productId);
        /// <summary>
        /// 获取所有分销中的商品编号
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        List<long> GetAllDistributionProductIds(long shopId);
        /// <summary>
        /// 获取分销商品
        /// </summary>
        QueryPageModel<DistributionProduct> GetProducts(DistributionProductQuery query);
        /// <summary>
        /// 分销商品(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        List<DistributionProduct> GetProductsAll(DistributionProductQuery query);
        /// <summary>
        /// 获取有分销商品的一级分类
        /// </summary>
        /// <returns></returns>
        List<CategoryInfo> GetHaveDistributionProductTopCategory();
        #endregion

        #region 分销业绩
        /// <summary>
        /// 订单分佣比处理（待付款）
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="canSelfBuy"></param>
        /// <param name="maxLevel"></param>
        void TreatedOrderDistribution(long orderId, bool canSelfBuy, int maxLevel);
        /// <summary>
        /// 处理订单分佣记录（收款）
        /// <para>付款、货到付款完成订单、售后</para>
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="needOperaDistributor">是否处理销售员清退情况</param>
        /// <param name="status">处理后分佣条目的状态</param>
        /// <param name="orderItemId"></param>
        void TreatedOrderDistributionBrokerage(long orderId, bool needOperaDistributor, long? orderItemId = null);
        /// <summary>
        /// 订单分佣状态处理
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        /// <param name="orderItemId"></param>
        void SetBrokerageStatus(long orderId,DistributionBrokerageStatus status, long? orderItemId = null);
        /// <summary>
        /// 移除分佣
        /// <para>订单关闭</para>
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="orderItemId"></param>
        void RemoveBrokerageByOrder(long orderId, long? orderItemId = null);
        /// <summary>
        /// 分销订单列表
        /// </summary>
        /// <param name="query"></param>
        QueryPageModel<BrokerageOrder> GetBrokerageOrders(BrokerageOrderQuery query);
        /// <summary>
        /// 分销订单列表(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        List<BrokerageOrder> GetBrokerageOrdersAll(BrokerageOrderQuery query);
        /// <summary>
        /// 分销商品业绩列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<BrokerageProduct> GetBrokerageProduct(BrokerageProductQuery query);
        /// <summary>
        /// 获取分销明细列表(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        List<BrokerageProduct> GetBrokerageProductAll(BrokerageProductQuery query);
        /// <summary>
        /// 销售员分销订单集
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<OrderInfo> GetDistributorBrokerageOrderList(DistributionBrokerageQuery query);
        /// <summary>
        /// 通过订单项编号获取分佣数据集
        /// </summary>
        /// <param name="orderItemIds"></param>
        /// <returns></returns>
        List<DistributionBrokerageInfo> GetDistributionBrokerageByOrderItemIds(IEnumerable<long> orderItemIds);
        /// <summary>
        /// 通过订单编号获取分佣数据集
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        List<DistributionBrokerageInfo> GetDistributionBrokerageByOrderIds(IEnumerable<long> orderIds);
        /// <summary>
        /// 获取订单的分销佣金之和
        /// </summary>
        /// <param name="orderid"></param>
        /// <returns></returns>
        decimal GetDistributionBrokerageAmount(long orderid);
        #endregion


    }
}
