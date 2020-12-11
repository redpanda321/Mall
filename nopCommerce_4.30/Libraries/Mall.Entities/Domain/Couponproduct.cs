using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class CouponProductInfo
    {
        public int Id { get; set; }
        public long CouponId { get; set; }
        public long ProductId { get; set; }
    }
}
