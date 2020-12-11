using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MemberIntegralRecordInfo
    {
        public long Id { get; set; }
        public long MemberId { get; set; }
        public string UserName { get; set; }
        public int TypeId { get; set; }
        public int Integral { get; set; }
        public DateTime RecordDate { get; set; }
        public string ReMark { get; set; }
    }
}
