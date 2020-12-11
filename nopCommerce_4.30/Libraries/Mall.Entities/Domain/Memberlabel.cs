using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MemberLabelInfo
    {
        public long Id { get; set; }
        public long MemId { get; set; }
        public long LabelId { get; set; }
    }
}
