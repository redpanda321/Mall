using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class CouponSendByRegisterDetailedInfo
    {
        public long Id { get; set; }
        public long CouponRegisterId { get; set; }
        public long CouponId { get; set; }
    }
}
