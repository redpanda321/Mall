using Mall.DTO.QueryModel;
using Mall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Application
{
    public class ComplaintApplication : BaseApplicaion<IComplaintService>
    {
        /// <summary>
        /// 获取投诉数量
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static int GetOrderComplaintCount(ComplaintQuery query)
        {
            return Service.GetOrderComplaintCount(query);
        }
    }
}
