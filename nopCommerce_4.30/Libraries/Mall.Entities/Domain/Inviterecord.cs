using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class InviteRecordInfo
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string RegName { get; set; }
        public int InviteIntegral { get; set; }
        public int RegIntegral { get; set; }
        public DateTime RegTime { get; set; }
        public long UserId { get; set; }
        public long RegUserId { get; set; }
        public DateTime RecordTime { get; set; }
    }
}
