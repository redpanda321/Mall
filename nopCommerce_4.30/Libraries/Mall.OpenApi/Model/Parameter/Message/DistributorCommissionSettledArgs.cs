using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.OpenApi.Model.Parameter.Message
{
    public class DistributorCommissionSettledArgs
    {
        public long UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime SettlementDate { get; set; }
    }
}
