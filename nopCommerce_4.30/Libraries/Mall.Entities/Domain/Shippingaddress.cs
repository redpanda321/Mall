using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShippingAddressInfo
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public int RegionId { get; set; }
        public string ShipTo { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public string Phone { get; set; }
        public byte IsDefault { get; set; }
        public byte IsQuick { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
    }
}
