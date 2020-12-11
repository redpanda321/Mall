using Mall.CommonModel;
using Mall.DTO;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System.Linq;
using NetRube.Data;
using System.Collections.Generic;

namespace Mall.Service
{
    public class AccountService : ServiceBase, IAccountService
    {
        public QueryPageModel<AccountInfo> GetAccounts(AccountQuery query)
        {
            //IQueryable<AccountInfo> complaints = Context.AccountInfo.AsQueryable();
            var complaints = DbFactory.Default.Get<AccountInfo>();

            #region 条件组合
            if (query.Status.HasValue)
            {
                complaints.Where(item => query.Status == item.Status);
            }
            if (query.ShopId.HasValue)
            {
                complaints.Where(item => query.ShopId == item.ShopId);
            }
            if (!string.IsNullOrEmpty(query.ShopName))
            {
                complaints.Where(item => item.ShopName.Contains(query.ShopName));
            }
            #endregion

            //int total;
            //complaints = complaints.GetPage(d => d.OrderByDescending(o => o.Id), out total, query.PageNo, query.PageSize);
            var models = complaints.OrderByDescending(o => o.Id).ToPagedList(query.PageNo, query.PageSize);

            QueryPageModel<AccountInfo> pageModel = new QueryPageModel<AccountInfo>() { Models = models, Total = models.TotalRecordCount };
            return pageModel;
        }

        public AccountInfo GetAccount(long id)
        {
            return DbFactory.Default.Get<AccountInfo>().Where(p => p.Id == id).FirstOrDefault();
        }
        /// <summary>
        /// 根据ID获取多条结算记录
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<AccountInfo> GetAccounts(IEnumerable<long> ids)
        {
            return DbFactory.Default.Get<AccountInfo>().Where(p => p.Id.ExIn(ids)).ToList();
        }
        public QueryPageModel<AccountDetailInfo> GetAccountDetails(AccountQuery query)
        {
            //IQueryable<AccountDetailInfo> accountDetails = Context.AccountDetailInfo.Where(item => item.OrderType == query.EnumOrderType && item.AccountId == query.AccountId);
            var accountDetails = DbFactory.Default.Get<AccountDetailInfo>().Where(item => item.OrderType == query.EnumOrderType && item.AccountId == query.AccountId);
            if (query.StartDate.HasValue)
            {
                accountDetails.Where(item => item.Date >= query.StartDate);
            }
            if (query.EndDate.HasValue)
            {
                accountDetails.Where(item => item.Date < query.EndDate);
            }
            //int total;
            //accountDetails = accountDetails.GetPage(d => d.OrderByDescending(o => o.Id), out total, query.PageNo, query.PageSize);
            var models = accountDetails.OrderByDescending(o => o.Id).ToPagedList(query.PageNo, query.PageSize);

            QueryPageModel<AccountDetailInfo> pageModel = new QueryPageModel<AccountDetailInfo>() { Models = models, Total = models.TotalRecordCount };
            return pageModel;
        }

        public void ConfirmAccount(long id, string managerRemark)
        {
          
            DbFactory.Default.Set<AccountInfo>().Set(n => n.Status, AccountInfo.AccountStatus.Accounted).Set(n => n.Remark, managerRemark).Where(p => p.Id == id).Succeed();
        }


        public QueryPageModel<AccountMetaModel> GetAccountMeta(AccountQuery query)
        {
            var result = DbFactory.Default.Get<AccountInfo>().LeftJoin<AccountMetaInfo>((ai, ami) => ai.Id == ami.AccountId).Where(a => a.Id == query.AccountId);
            if (query.StartDate.HasValue)
            {
                result.Where(a => a.StartDate >= query.StartDate);
            }
            if (query.EndDate.HasValue)
            {
                result.Where(a => a.EndDate < query.EndDate);
            }
            result = result.Select<AccountMetaInfo>(p => new
            {
                AccountId = p.AccountId,
                Id = p.Id,
                EndDate = p.ServiceEndTime,
                StartDate = p.ServiceStartTime,
                MetaKey = p.MetaKey,
                MetaValue = p.MetaValue
            });

            
            var metaInfo = result.ToPagedList<AccountMetaModel>(query.PageNo, query.PageSize);

            return new QueryPageModel<AccountMetaModel>() { Models = metaInfo, Total = metaInfo.TotalRecordCount };
        }
    }
}
