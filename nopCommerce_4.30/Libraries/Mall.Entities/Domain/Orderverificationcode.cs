using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class OrderVerificationCodeInfo
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long OrderItemId { get; set; }
        public byte Status { get; set; }
        public string VerificationCode { get; set; }
        public DateTime? VerificationTime { get; set; }
        public string VerificationUser { get; set; }
    }
}
