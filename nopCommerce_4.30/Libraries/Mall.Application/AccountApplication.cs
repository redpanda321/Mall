using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System.Collections.Generic;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    /// <summary>
    /// 结算相关服务应用
    /// </summary>
    public class AccountApplication
    {
         private static IAccountService _iAccountService =  EngineContext.Current.Resolve<IAccountService>();


       

        public static QueryPageModel<AccountInfo> GetAccounts(AccountQuery query)
        {
            return _iAccountService.GetAccounts(query);
        }

        public static AccountInfo GetAccount(long id)
        {
            return _iAccountService.GetAccount(id);
        }
        /// <summary>
        /// 根据ID获取多条结算记录
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<AccountInfo> GetAccounts(IEnumerable<long> ids)
        {
            return _iAccountService.GetAccounts(ids);
        }
        /// <summary>
        /// 获取结算订单明细列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<AccountDetailInfo> GetAccountDetails(AccountQuery query)
        {
            return _iAccountService.GetAccountDetails(query);
        }
        /// <summary>
        /// 取服务费用
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<AccountMetaModel> GetAccountMeta(AccountQuery query)
        {
            return _iAccountService.GetAccountMeta(query);
        }
        /// <summary>
        /// 确认结算
        /// </summary>
        /// <param name="id"></param>
        /// <param name="managerRemark"></param>
        public static void ConfirmAccount(long id, string managerRemark)
        {
            _iAccountService.ConfirmAccount(id, managerRemark);
        }
    }
}
