using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class DistributionRankingBatchInfo
    {
        public long Id { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
