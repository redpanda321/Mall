using Mall.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Mall.DTO
{
    public class ShopActiveList
    {
        public ShopActiveList()
        {
            ShopActives = new List<ActiveInfo>();
            ShopCoupons = new List<CouponModel>();
        }

        public List<ActiveInfo> ShopActives { get; set; }

        public decimal FreeFreightAmount { get; set; }

        public bool IsFreeMail { get; set; }

        public List<CouponModel> ShopCoupons { get; set; }
        public decimal MinCouponPrice
        {
            get
            {
                decimal result = 0;
                if (ShopCoupons != null && ShopCoupons.Count > 0)
                {
                    result = ShopCoupons.Min(d => d.Price);
                }
                return result;
            }
        }
        public decimal MaxCouponPrice
        {
            get
            {
                decimal result = 0;
                if (ShopCoupons != null && ShopCoupons.Count > 0)
                {
                    result = ShopCoupons.Max(d => d.Price);
                }
                return result;
            }
        }
    }
}
