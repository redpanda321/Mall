using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MemberIntegralRecordActionInfo
    {
        public long Id { get; set; }
        public long IntegralRecordId { get; set; }
        public int VirtualItemTypeId { get; set; }
        public long VirtualItemId { get; set; }
    }
}
