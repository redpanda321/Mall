using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Web.Models
{
    public class CouponViewModel
    {
        public CouponInfo Coupon { get; set; }

        public DateTime EndTime { get; set; }
        public List<CouponProductInfo> CouponProducts { get; set; }
        public List<ProductInfo> Products{ get; set; }

        public List<CouponSettingInfo> Settings { get; set; }
        public bool CanAddIntegralCoupon { get; set; }

        public bool CanVshopIndex { get; set; }

    }
}