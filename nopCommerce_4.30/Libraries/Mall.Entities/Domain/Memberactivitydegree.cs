using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MemberActivityDegreeInfo
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public short OneMonth { get; set; }
        public short ThreeMonth { get; set; }
        public short SixMonth { get; set; }
        public DateTime? OneMonthEffectiveTime { get; set; }
        public DateTime? ThreeMonthEffectiveTime { get; set; }
        public DateTime? SixMonthEffectiveTime { get; set; }
    }
}
