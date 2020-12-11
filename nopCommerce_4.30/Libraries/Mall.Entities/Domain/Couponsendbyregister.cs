using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class CouponSendByRegisterInfo
    {
        public long Id { get; set; }
        public int Status { get; set; }
        public string Link { get; set; }
    }
}
