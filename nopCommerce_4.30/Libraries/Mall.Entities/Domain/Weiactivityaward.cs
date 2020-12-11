using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class WeiActivityAwardInfo
    {
        public long Id { get; set; }
        public long ActivityId { get; set; }
        public int AwardLevel { get; set; }
        public int AwardType { get; set; }
        public int AwardCount { get; set; }
        public float Proportion { get; set; }
        public int Integral { get; set; }
        public long BonusId { get; set; }
        public long CouponId { get; set; }
    }
}
