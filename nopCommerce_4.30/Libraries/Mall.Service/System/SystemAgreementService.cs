using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Linq;

namespace Mall.Service
{
    public class SystemAgreementService : ServiceBase, ISystemAgreementService
    {
        /// <summary>
        /// 获取协议信息
        /// </summary>
        /// <param name="Id">协议类型</param>
        /// <returns></returns>
        public AgreementInfo GetAgreement(AgreementInfo.AgreementTypes type)
        {
            return DbFactory.Default.Get<AgreementInfo>().Where(b => b.AgreementType == type).FirstOrDefault();
        }
        /// <summary>
        /// 添加协议信息
        /// </summary>
        /// <param name="model">协议信息</param>
        public void AddAgreement(AgreementInfo model)
        {
            DbFactory.Default.Add(model);
        }

        /// <summary>
        /// 修改协议信息
        /// </summary>
        /// <param name="model">协议信息</param>
        public bool UpdateAgreement(AgreementInfo model)
        {
            var agreement = GetAgreement(model.AgreementType);
            agreement.AgreementType = model.AgreementType;
            agreement.AgreementContent = model.AgreementContent;
            agreement.LastUpdateTime = DateTime.Now;
            //if (Context.SaveChanges() > 0)
            //    return true;
            //else
            //    return false;
            return DbFactory.Default.Update(agreement) > 0;
        }
    }
}
