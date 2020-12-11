using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class InviteRuleInfo
    {
        public long Id { get; set; }
        public int InviteIntegral { get; set; }
        public int RegIntegral { get; set; }
        public string ShareTitle { get; set; }
        public string ShareDesc { get; set; }
        public string ShareIcon { get; set; }
        public string ShareRule { get; set; }
    }
}
