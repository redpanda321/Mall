using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class WxAppletFormDataInfo
    {
        public long Id { get; set; }
        public long EventId { get; set; }
        public string EventValue { get; set; }
        public string FormId { get; set; }
        public DateTime EventTime { get; set; }
        public DateTime ExpireTime { get; set; }
    }
}
