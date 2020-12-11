using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.Core;
using Mall.DTO;
using Mall.IServices;
using Nop.Core.Infrastructure;

namespace Mall.Application
{

    public class SystemAgreementApplication
    {
        private static ISystemAgreementService _iSystemAgreementService =  EngineContext.Current.Resolve<ISystemAgreementService>();

        /// <summary>
        /// 获取协议信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static Entities.AgreementInfo GetAgreement(Entities.AgreementInfo.AgreementTypes type)
        {
            return _iSystemAgreementService.GetAgreement(type);
        }
        /// <summary>
        /// 添加协议
        /// </summary>
        /// <param name="model"></param>
        public static void AddAgreement(Entities.AgreementInfo model)
        {
            _iSystemAgreementService.AddAgreement(model);
        }
        /// <summary>
        /// 修改协议
        /// </summary>
        /// <param name="model"></param>
        public static bool UpdateAgreement(Entities.AgreementInfo model)
        {
            return _iSystemAgreementService.UpdateAgreement(model);
        }
    }
}
