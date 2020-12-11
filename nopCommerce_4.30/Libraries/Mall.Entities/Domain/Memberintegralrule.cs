using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MemberIntegralRuleInfo
    {
        public long Id { get; set; }
        public int TypeId { get; set; }
        public int Integral { get; set; }
    }
}
