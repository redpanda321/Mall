using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class SendMessageRecordCouponSnInfo
    {
        public long Id { get; set; }
        public long MessageId { get; set; }
        public string CouponSn { get; set; }
    }
}
