using Mall.CommonModel;
using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mall.Entities.OrderInfo;

namespace Mall.DTO.QueryModel
{
    public class DistributionBrokerageQuery : QueryBase
    {
        /// <summary>
        /// 分销员ID
        /// </summary>
        public long DistributorId { get; set; }
    }
}
