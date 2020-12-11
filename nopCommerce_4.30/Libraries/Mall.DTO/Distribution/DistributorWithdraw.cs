using Mall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class DistributionApplyWithdraw
    {
        public long MemberId { get; set; }
        public decimal Amount { get; set; }
        public DistributionWithdrawType Type { get; set; }
        public string Password { get; set; }
        public string WithdrawAccount { get; set; }
        public string WithdrawName { get; set; }
    }
}
