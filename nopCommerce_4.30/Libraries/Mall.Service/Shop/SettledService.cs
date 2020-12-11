using Mall.CommonModel;
using Mall.IServices;
using NetRube.Data;
using Mall.Entities;

namespace Mall.Service
{
    public class SettledService : ServiceBase, ISettledService
    {
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="mSettledInfo"></param>
        public void AddSettled(SettledInfo mSettledInfo)
        {
            //var data = Context.SettledInfo.ToList();
            var data = DbFactory.Default.Get<SettledInfo>().ToList();
            if (data.Count == 0)
            {
                //Context.SettledInfo.Add(mSettledInfo);
                //Context.SaveChanges();
                DbFactory.Default.Add(mSettledInfo);
                Core.Cache.Remove(CacheKeyCollection.Settled);//清除缓存
            }
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="mSettledInfo"></param>
        public void UpdateSettled(SettledInfo mSettledInfo)
        {
            //var model = Context.SettledInfo.FirstOrDefault(s => s.ID == mSettledInfo.ID);
            var model = DbFactory.Default.Get<SettledInfo>().Where(s => s.ID == mSettledInfo.ID).FirstOrDefault();
            model.BusinessType = mSettledInfo.BusinessType;
            model.CompanyVerificationType = mSettledInfo.CompanyVerificationType;
            model.IsAddress = mSettledInfo.IsAddress;
            model.IsAgencyCode = mSettledInfo.IsAgencyCode;
            model.IsAgencyCodeLicense = mSettledInfo.IsAgencyCodeLicense;
            model.IsBusinessLicense = mSettledInfo.IsBusinessLicense;
            model.IsBusinessLicenseCode = mSettledInfo.IsBusinessLicenseCode;
            model.IsBusinessScope = mSettledInfo.IsBusinessScope;
            model.IsCity = mSettledInfo.IsCity;
            model.IsPeopleNumber = mSettledInfo.IsPeopleNumber;
            model.IsSAddress = mSettledInfo.IsSAddress;
            model.IsSCity = mSettledInfo.IsSCity;
            model.IsSIDCard = mSettledInfo.IsSIDCard;
            model.IsSIdCardUrl = mSettledInfo.IsSIdCardUrl;
            model.IsSName = mSettledInfo.IsSName;
            model.IsTaxpayerToProve = mSettledInfo.IsTaxpayerToProve;
            model.SelfVerificationType = mSettledInfo.SelfVerificationType;
            model.SettlementAccountType = mSettledInfo.SettlementAccountType;
            model.TrialDays = mSettledInfo.TrialDays;
            //Context.SaveChanges();
            DbFactory.Default.Update(model);
            Core.Cache.Remove(CacheKeyCollection.Settled);//清除缓存
        }

        /// <summary>
        /// 查询,获取第一条数据
        /// </summary>
        /// <returns></returns>
        public SettledInfo GetSettled()
        {
            SettledInfo mSettledInfo;
            if (Core.Cache.Exists(CacheKeyCollection.Settled))//如果存在缓存，则从缓存中读取
                mSettledInfo = Core.Cache.Get<SettledInfo>(CacheKeyCollection.Settled);
            else
            {
                //mSettledInfo = Context.SettledInfo.FirstOrDefault();
                mSettledInfo = DbFactory.Default.Get<SettledInfo>().FirstOrDefault();
                Core.Cache.Insert(CacheKeyCollection.Settled, mSettledInfo);
            }

            return mSettledInfo;
        }
    }
}
