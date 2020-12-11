using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MemberIntegralInfo
    {
        public long Id { get; set; }
        public long MemberId { get; set; }
        public string UserName { get; set; }
        public int HistoryIntegrals { get; set; }
        public int AvailableIntegrals { get; set; }
    }
}
