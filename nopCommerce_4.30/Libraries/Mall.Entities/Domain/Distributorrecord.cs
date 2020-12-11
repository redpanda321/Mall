using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class DistributorRecordInfo
    {
        public long Id { get; set; }
        public long MemberId { get; set; }
        public byte Type { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
