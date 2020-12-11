using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class SiteSigninConfigInfo
    {
        public long Id { get; set; }
        public byte IsEnable { get; set; }
        public int DayIntegral { get; set; }
        public int DurationCycle { get; set; }
        public int DurationReward { get; set; }
    }
}
