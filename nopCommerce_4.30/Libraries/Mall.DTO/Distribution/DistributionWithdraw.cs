using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class DistributionWithdraw: DistributionWithdrawInfo
    {
        public MemberInfo Member { get; set; }
    }
}
