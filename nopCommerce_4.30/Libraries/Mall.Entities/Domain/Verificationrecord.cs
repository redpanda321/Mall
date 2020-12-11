using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class VerificationRecordInfo
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public string VerificationCodeIds { get; set; }
        public DateTime VerificationTime { get; set; }
        public string VerificationUser { get; set; }
    }
}
