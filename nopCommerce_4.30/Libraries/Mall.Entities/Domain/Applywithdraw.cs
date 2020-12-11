using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ApplyWithdrawInfo
    {
        public long Id { get; set; }
        public long MemId { get; set; }
        public string NickName { get; set; }
        public string OpenId { get; set; }
        public int ApplyStatus { get; set; }
        public decimal ApplyAmount { get; set; }
        public DateTime ApplyTime { get; set; }
        public DateTime? ConfirmTime { get; set; }
        public DateTime? PayTime { get; set; }
        public string PayNo { get; set; }
        public string OpUser { get; set; }
        public string Remark { get; set; }
        public int ApplyType { get; set; }
        public decimal Poundage { get; set; }
    }
}
