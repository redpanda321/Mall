using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MemberSigninInfo
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public DateTime LastSignTime { get; set; }
        public int DurationDay { get; set; }
        public int DurationDaySum { get; set; }
        public long SignDaySum { get; set; }
    }
}
