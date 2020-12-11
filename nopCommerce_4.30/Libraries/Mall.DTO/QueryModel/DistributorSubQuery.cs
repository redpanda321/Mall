using Mall.DTO.QueryModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class DistributorSubQuery : QueryBase
    {
        /// <summary>
        /// 包含未结算金额
        /// </summary>
        public bool IncludeNoSettlementAmount { get; set; } = false;

        public string MemberName { get; set; }
        public string ShopName { get; set; }
        /// <summary>
        /// 上级ID
        /// </summary>
        public long SuperiorId { get; set; }
        /// <summary>
        /// 查询等级
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 下单会员
        /// </summary>
        public List<long> Members { get; set; }

        /// <summary>
        /// 获得全部
        /// </summary>
        public bool IsAll { get; set; } = false;
    }
}
