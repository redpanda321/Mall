using NPoco;
using System.ComponentModel;
using System.Configuration;

namespace Mall.Entities
{
    public partial class ShippingAddressInfo
    {
        public ShippingAddressInfo()
        {
            NeedUpdate = true;
        }

        [ResultColumn]
        public string RegionFullName { get; set; }

        [ResultColumn]
        public string RegionIdPath { set; get; }

        [ResultColumn]
        public bool NeedUpdate { get; set; }

        [ResultColumn]
        public int IsNeedUpdate { get { return NeedUpdate ? 1 : 0; } }

        [ResultColumn]
        public bool CanDelive { get; set; }
        [ResultColumn]
        public long shopBranchId { get; set; }
    }
}
