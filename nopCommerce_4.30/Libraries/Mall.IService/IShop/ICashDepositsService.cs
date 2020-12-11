using Mall.CommonModel;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System;
using System.Collections.Generic;

namespace Mall.IServices
{
    public interface ICashDepositsService : IService
    {
        /// <summary>
        /// 获取保证金列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<CashDepositInfo> GetCashDeposits(CashDepositQuery query);

        /// <summary>
        /// 根据保证金ID获取明细列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<CashDepositDetailInfo> GetCashDepositDetails(CashDepositDetailQuery query);

        /// <summary>
        /// 新增类目保证金
        /// </summary>
        /// <param name="model"></param>
        void AddCategoryCashDeposits(CategoryCashDepositInfo model);

        /// <summary>
        /// 根据一级分类删除类目保证金
        /// </summary>
        /// <param name="categoryId"></param>
        void DeleteCategoryCashDeposits(long categoryId);
        /// <summary>
        /// 根据保证金ID查询实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        CashDepositInfo GetCashDeposit(long id);

        /// <summary>
        /// 商家余额支付保证金资金帐号变动记录
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="amount"></param>
        /// <param name="remark"></param>
        /// <param name="detailId"></param>
        bool ShopAccountRecord(long shopId, decimal amount, string remark, string detailId);

        /// <summary>
        /// 第一次充值保证金时插入记录
        /// </summary>
        /// <param name="cashDeposit"></param>
        void AddCashDeposit(CashDepositInfo cashDeposit, List<CashDepositDetailInfo> details);

        /// <summary>
        /// 插入一条保证金流水号信息
        /// </summary>
        /// <param name="cashDepositDetail"></param>
        void AddCashDepositDetails(CashDepositDetailInfo cashDepositDetail);

        /// <summary>
        /// 根据店铺ID获取保证金
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        CashDepositInfo GetCashDepositByShopId(long shopId);

        /// <summary>
        /// 更新标识的显示状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="EnableLabels"></param>
        void UpdateEnableLabels(long id, bool enableLabels);

        #region 类目保证金
        /// <summary>
        /// 获取店铺应缴保证金
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        Decimal GetNeedPayCashDepositByShopId(long shopId);

        /// <summary>
        /// 获取分类保证金列表
        /// </summary>
        /// <returns></returns>
        List<CategoryCashDepositInfo> GetCategoryCashDeposits();
        /// <summary>
        /// 更新所需缴纳保证金
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="CashDeposits">保证金金额</param>
        void UpdateNeedPayCashDeposit(long categoryId, decimal CashDeposit);

        /// <summary>
        /// 开启七天无理由退换货
        /// </summary>
        /// <param name="id"></param>
        void OpenNoReasonReturn(long id);

        /// <summary>
        /// 关闭七天无理由退换货
        /// </summary>
        /// <param name="id"></param>
        void CloseNoReasonReturn(long id);


        #endregion

        /// <summary>
        /// 获取提供特殊服务实体
        /// </summary>
        /// <param name="productId"></param>
        CashDepositsObligation GetCashDepositsObligation(long productId);
    }
}
