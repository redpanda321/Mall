using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class CategoryCashDepositInfo
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public decimal NeedPayCashDeposit { get; set; }
        public byte EnableNoReasonReturn { get; set; }
    }
}
