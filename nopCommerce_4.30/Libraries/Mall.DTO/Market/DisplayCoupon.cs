using Mall.CommonModel;
using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class DisplayCoupon
    {
        public CommonModel.CouponType Type { get; set; }

        public decimal Price { get; set; }

        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public decimal Limit { get; set; }
        public DateTime EndTime { get; set; }

        public ShopBonusInfo.UseStateType UseState { get; set; }
    }
}
