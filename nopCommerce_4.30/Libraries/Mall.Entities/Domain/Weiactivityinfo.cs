using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class WeiActivityInfoInfo
    {
        public long Id { get; set; }
        public string ActivityTitle { get; set; }
        public int ActivityType { get; set; }
        public string ActivityDetails { get; set; }
        public string ActivityUrl { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public int ParticipationType { get; set; }
        public int ParticipationCount { get; set; }
        public int ConsumePoint { get; set; }
        public string CodeUrl { get; set; }
        public DateTime AddDate { get; set; }
    }
}
