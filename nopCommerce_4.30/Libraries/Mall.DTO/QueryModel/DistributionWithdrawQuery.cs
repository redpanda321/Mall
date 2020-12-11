using Mall.CommonModel;
using Mall.DTO.QueryModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class DistributionWithdrawQuery : QueryBase
    {
        public long MemberId { get; set; }
        public string MemberName { get; set; }
        public DateTime? Begin { get; set; }
        public DateTime? End { get; set; }
        public long WithdrawId { get; set; }

        public DistributionWithdrawType? Type { get; set; }
        public DistributionWithdrawStatus? Status { get; set; }
    }
}
