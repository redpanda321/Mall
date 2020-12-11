using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class IntegralMallAdInfo
    {
        public long Id { get; set; }
        public int ActivityType { get; set; }
        public long ActivityId { get; set; }
        public string Cover { get; set; }
        public int ShowStatus { get; set; }
        public int ShowPlatform { get; set; }
    }
}
