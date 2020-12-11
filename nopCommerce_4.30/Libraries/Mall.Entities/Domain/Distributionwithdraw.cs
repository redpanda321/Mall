using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class DistributionWithdrawInfo
    {
        public long Id { get; set; }
        public long MemberId { get; set; }
        public string WithdrawName { get; set; }
        public string WithdrawAccount { get; set; }
        public int WithdrawStatus { get; set; }
        public decimal Amount { get; set; }
        public DateTime ApplyTime { get; set; }
        public DateTime? ConfirmTime { get; set; }
        public DateTime? PayTime { get; set; }
        public string PayNo { get; set; }
        public string Operator { get; set; }
        public string Remark { get; set; }
        public int WithdrawType { get; set; }
        public decimal Poundage { get; set; }
    }
}
