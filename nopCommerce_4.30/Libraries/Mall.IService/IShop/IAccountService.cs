using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.CommonModel;
using Mall.DTO;
using System.Collections.Generic;

namespace Mall.IServices
{
    public interface IAccountService : IService
    {
        /// <summary>
        /// 获取结算列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<AccountInfo> GetAccounts(AccountQuery query);

        /// <summary>
        /// 获取单个结算
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        AccountInfo GetAccount(long id);
        /// <summary>
        /// 根据ID获取多条结算记录
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        List<AccountInfo> GetAccounts(IEnumerable<long> ids);

        /// <summary>
        /// 获取结算订单明细列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<AccountDetailInfo> GetAccountDetails(AccountQuery query);


        /// <summary>
        /// 取服务费用
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<AccountMetaModel> GetAccountMeta(AccountQuery query);


        /// <summary>
        /// 确认结算
        /// </summary>
        /// <param name="id"></param>
        /// <param name="managerRemark"></param>
        void ConfirmAccount(long id, string managerRemark);
    }
}
