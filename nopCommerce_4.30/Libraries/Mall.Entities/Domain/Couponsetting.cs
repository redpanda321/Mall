using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class CouponSettingInfo
    {
        public int Id { get; set; }
        public int PlatForm { get; set; }
        public long CouponId { get; set; }
        public int Display { get; set; }
    }
}
