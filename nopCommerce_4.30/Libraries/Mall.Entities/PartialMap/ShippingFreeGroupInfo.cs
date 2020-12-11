using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class ShippingFreeGroupInfo
    {
        List<ShippingFreeRegionInfo> _ShippingFreeRegionInfo = null;
        /// <summary>
        /// Id == GroupId 
        /// </summary>
        [ResultColumn]
        public List<ShippingFreeRegionInfo> ShippingFreeRegionInfo { get; set; }
    }
}
