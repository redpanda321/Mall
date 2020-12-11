using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class CapitalDetailInfo
    {
        public long Id { get; set; }
        public long CapitalId { get; set; }
        public int SourceType { get; set; }
        public decimal Amount { get; set; }
        public string SourceData { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
        public decimal PresentAmount { get; set; }
    }
}
