using Mall.CommonModel;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Application
{
    public class ConsultationApplication:BaseApplicaion<IConsultationService>
    {
        /// <summary>
        /// 获取咨询数
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static int GetConsultationCount(ConsultationQuery query)
        {
            return Service.GetConsultationCount(query);
        }
        /// <summary>
        /// 获取咨询数据
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ProductConsultationInfo> GetConsultations(ConsultationQuery query)
        {
            return Service.GetConsultations(query);
        }
    }
}
