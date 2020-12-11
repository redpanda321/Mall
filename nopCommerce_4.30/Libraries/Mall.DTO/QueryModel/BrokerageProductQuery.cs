using Mall.CommonModel;
using Mall.DTO.QueryModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class BrokerageProductQuery:QueryBase
    {
        public string ProductName { get; set; }
        public long ShopId { get; set; }
        /// <summary>
        /// 分销状态
        /// </summary>
        public DistributionProductStatus? Status { get; set; }
    }
}
