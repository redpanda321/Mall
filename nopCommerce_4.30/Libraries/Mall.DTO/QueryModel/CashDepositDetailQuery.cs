using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO.QueryModel
{
    public partial class CashDepositDetailQuery:QueryBase
    {
        public long CashDepositId { get; set; }
        public string Operator { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
