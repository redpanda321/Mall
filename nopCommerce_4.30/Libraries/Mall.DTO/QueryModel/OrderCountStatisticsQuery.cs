using Mall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mall.Entities.OrderInfo;

namespace Mall.DTO.QueryModel
{
    public class OrderCountStatisticsQuery : QueryBase
    {
        public long UserId { get; set; }
        public long ShopId { get; set; }
        public long ShopBranchId { get; set; }
        public DateTime? OrderDateBegin { get; set; }
        public DateTime? OrderDateEnd { get; set; }
        
        public bool? IsPayed { get; set; }

        public OrderOperateStatus? OrderOperateStatus { get; set; }
        public List<OrderOperateStatus> OrderOperateStatusList { get; set; }

        public bool? IsCommented { get; set; }

        public List<OrderCountStatisticsFields> Fields { get; set; } = new List<OrderCountStatisticsFields>();

        /// <summary>
        /// 是否虚拟订单
        /// </summary>
        public bool? IsVirtual { get; set; }
    }
}
