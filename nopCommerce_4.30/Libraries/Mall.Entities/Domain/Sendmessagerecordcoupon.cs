using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class SendMessageRecordCouponInfo
    {
        public long Id { get; set; }
        public long MessageId { get; set; }
        public long CouponId { get; set; }
    }
}
