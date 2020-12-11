using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class BrowsingHistoryInfo
    {
        public long Id { get; set; }
        public long MemberId { get; set; }
        public long ProductId { get; set; }
        public DateTime BrowseTime { get; set; }
    }
}
