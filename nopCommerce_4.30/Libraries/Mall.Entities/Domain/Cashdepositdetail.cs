using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class CashDepositDetailInfo
    {
        public long Id { get; set; }
        public long CashDepositId { get; set; }
        public DateTime AddDate { get; set; }
        public decimal Balance { get; set; }
        public string Operator { get; set; }
        public string Description { get; set; }
        public int? RechargeWay { get; set; }
    }
}
