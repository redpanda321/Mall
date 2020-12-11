using System;
using System.Collections.Generic;
using Mall.Core;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.CommonModel;
using Mall.DTO;
using System.Linq;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    public class CashDepositsApplication:BaseApplicaion
    {
      //  private static ICashDepositsService _iCashDepositsService =  EngineContext.Current.Resolve<ICashDepositsService>();


        private static ICashDepositsService _iCashDepositsService =  EngineContext.Current.Resolve<ICashDepositsService>();


        /// <summary>
        /// 获取保证金列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<CashDeposit> GetCashDeposits(CashDepositQuery query)
        {
            var data = _iCashDepositsService.GetCashDeposits(query);
            var shops = GetService<IShopService>().GetShops(data.Models.Select(p => p.ShopId).ToList());
            var result = data.Models.Select(item =>
            {
                var needPay = _iCashDepositsService.GetNeedPayCashDepositByShopId(item.ShopId);
                var shop = shops.FirstOrDefault(p => p.Id == item.ShopId);
                return new CashDeposit
                {
                    Id = item.Id,
                    ShopName = shop.ShopName,
                    Type = needPay > 0 ? "欠费" : "正常",
                    TotalBalance = item.TotalBalance,
                    CurrentBalance = item.CurrentBalance,
                    Date = item.Date,
                    NeedPay = needPay,
                    EnableLabels = item.EnableLabels,
                };
            }).ToList();

            return new QueryPageModel<CashDeposit>
            {
                Models = result,
                Total = data.Total
            };
        }

        /// <summary>
        /// 根据保证金ID获取明细列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<CashDepositDetailInfo> GetCashDepositDetails(CashDepositDetailQuery query)
        {
            return _iCashDepositsService.GetCashDepositDetails(query);
        }

        /// <summary>
        /// 新增类目保证金
        /// </summary>
        /// <param name="model"></param>
        public static void AddCategoryCashDeposits(CategoryCashDepositInfo model)
        {
            _iCashDepositsService.AddCategoryCashDeposits(model);
        }

        /// <summary>
        /// 根据一级分类删除类目保证金
        /// </summary>
        /// <param name="categoryId"></param>
        public static void DeleteCategoryCashDeposits(long categoryId)
        {
            _iCashDepositsService.DeleteCategoryCashDeposits(categoryId);
        }
        /// <summary>
        /// 根据保证金ID查询实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static CashDepositInfo GetCashDeposit(long id)
        {
            return _iCashDepositsService.GetCashDeposit(id);
        }


        /// <summary>
        /// 插入一条保证金流水号信息
        /// </summary>
        /// <param name="cashDepositDetail"></param>
        public static void AddCashDepositDetails(CashDepositDetailInfo cashDepositDetail)
        {
            _iCashDepositsService.AddCashDepositDetails(cashDepositDetail);
        }

        /// <summary>
        /// 根据店铺ID获取保证金
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static CashDepositInfo GetCashDepositByShopId(long shopId)
        {
            return _iCashDepositsService.GetCashDepositByShopId(shopId);
        }

        /// <summary>
        /// 更新标识的显示状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="EnableLabels"></param>
        public static void UpdateEnableLabels(long id, bool enableLabels)
        {
            _iCashDepositsService.UpdateEnableLabels(id, enableLabels);
        }

        #region 类目保证金
        /// <summary>
        /// 获取店铺应缴保证金
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static Decimal GetNeedPayCashDepositByShopId(long shopId)
        {
            return _iCashDepositsService.GetNeedPayCashDepositByShopId(shopId);
        }

        /// <summary>
        /// 获取分类保证金列表
        /// </summary>
        /// <returns></returns>
        public static List<CategoryCashDepositInfo> GetCategoryCashDeposits()
        {
            return _iCashDepositsService.GetCategoryCashDeposits();
        }
        /// <summary>
        /// 更新所需缴纳保证金
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="CashDeposits">保证金金额</param>
        public static void UpdateNeedPayCashDeposit(long categoryId, decimal CashDeposit)
        {
            _iCashDepositsService.UpdateNeedPayCashDeposit(categoryId, CashDeposit);
        }

        /// <summary>
        /// 开启七天无理由退换货
        /// </summary>
        /// <param name="id"></param>
        public static void OpenNoReasonReturn(long id)
        {
            _iCashDepositsService.OpenNoReasonReturn(id);
        }

        /// <summary>
        /// 关闭七天无理由退换货
        /// </summary>
        /// <param name="id"></param>
        public static void CloseNoReasonReturn(long id)
        {
            _iCashDepositsService.CloseNoReasonReturn(id);
        }


        #endregion

        /// <summary>
        /// 获取提供特殊服务实体
        /// </summary>
        /// <param name="productId"></param>
        public static CashDepositsObligation GetCashDepositsObligation(long productId)
        {
            return _iCashDepositsService.GetCashDepositsObligation(productId);
        }
    }
}
