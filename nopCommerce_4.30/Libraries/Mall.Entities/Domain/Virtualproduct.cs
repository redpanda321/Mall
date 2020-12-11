using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class VirtualProductInfo
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public short ValidityType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public byte EffectiveType { get; set; }
        public int Hour { get; set; }
        public byte SupportRefundType { get; set; }
        public string UseNotice { get; set; }
    }
}
