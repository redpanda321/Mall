using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    /// <summary>
    /// 会员收货地址
    /// </summary>
    public class ShippingAddress
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public int RegionId { get; set; }
        public string ShipTo { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public bool IsDefault { get; set; }
        public bool IsQuick { get; set; }
        public Nullable<float> Longitude { get; set; }
        public Nullable<float> Latitude { get; set; }
        public string AddressDetail { get; set; }
        public string RegionFullName { get; set; }
        public string RegionIdPath { set; get; }

        public bool CanDelive { get; set; }
        public long shopBranchId { get; set; }
        public bool NeedUpdate { get; set; }
    }
}
