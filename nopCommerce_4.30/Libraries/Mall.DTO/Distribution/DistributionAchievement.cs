using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    /// <summary>
    /// 分销业绩
    /// </summary>
    public class DistributionAchievement
    {
        public long MemberId { get; set; }
        /// <summary>
        /// 成交件数
        /// </summary>
        public decimal TotalCount { get; set; }
        /// <summary>
        /// 成交金额
        /// </summary>
        public decimal TotalAmount { get; set; }
    }
}
