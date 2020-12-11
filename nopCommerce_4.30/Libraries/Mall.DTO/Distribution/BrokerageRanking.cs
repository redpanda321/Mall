using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO.Distribution
{
    /// <summary>
    /// 分销排名
    /// </summary>
    public class BrokerageRanking
    {
        public int Rank { get; set; }

        public DistributorInfo Distributor { get; set; }
        public MemberInfo Member { get; set; }
        public DistributorGradeInfo Grade{ get; set; }

        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public decimal Settlement { get; set; }
        public decimal NoSettlement { get; set; }
    }
}
