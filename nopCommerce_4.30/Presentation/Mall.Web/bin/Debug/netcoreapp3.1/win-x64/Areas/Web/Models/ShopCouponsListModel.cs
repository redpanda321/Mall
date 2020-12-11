using Mall.Entities;

namespace Mall.Web.Areas.Web.Models
{
    public class ShopCouponsListModel
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public decimal Price { get; set; }
        public int PerMax { get; set; }
        public decimal OrderAmount { get; set; }
        public int Num { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public string CouponName { get; set; }
        public System.DateTime CreateTime { get; set; }
        public CouponInfo.CouponReceiveType ReceiveType { get; set; }

        public int UseArea { get; set; }

        public string Remark { get; set; }
    }
}