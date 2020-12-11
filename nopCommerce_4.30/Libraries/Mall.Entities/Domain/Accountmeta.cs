using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class AccountMetaInfo
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public string MetaKey { get; set; }
        public string MetaValue { get; set; }
        public DateTime ServiceStartTime { get; set; }
        public DateTime ServiceEndTime { get; set; }
    }
}
