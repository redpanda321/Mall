using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopShipperInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public byte IsDefaultSendGoods { get; set; }
        public byte IsDefaultGetGoods { get; set; }
        public byte? IsDefaultVerification { get; set; }
        public string ShipperTag { get; set; }
        public string ShipperName { get; set; }
        public int RegionId { get; set; }
        public string Address { get; set; }
        public string TelPhone { get; set; }
        public string Zipcode { get; set; }
        public string WxOpenId { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
    }
}
