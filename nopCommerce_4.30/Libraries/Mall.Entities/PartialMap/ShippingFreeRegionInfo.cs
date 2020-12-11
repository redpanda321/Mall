using Mall.CommonModel;
using NPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class ShippingFreeRegionInfo
    {
        /// <summary>
        /// 省会ID
        /// </summary>
        [ResultColumn]
        public int ProvinceId { get; set; }

        /// <summary>
        /// 省会名称
        /// </summary>
        [ResultColumn]
        public string ProvinceName { set; get; }
        /// <summary>
        /// 市ID
        /// </summary>
        [ResultColumn]
        public Nullable<int> CityId { get; set; }

        /// <summary>
        /// 市区名称
        /// </summary>
        [ResultColumn]
        public string CityName { set; get; }
        /// <summary>
        /// 县/区ID
        /// </summary>
        [ResultColumn]
        public Nullable<int> CountyId { get; set; }

        /// <summary>
        /// 县区名称
        /// </summary>
        [ResultColumn]
        public string CountyName { set; get; }

        /// <summary>
        /// 乡镇ID
        /// </summary>
        [ResultColumn]
        public string TownIds { get; set; }

        /// <summary>
        /// 乡镇名称
        /// </summary>
        [ResultColumn]
        public string TownNames { set; get; }
        [ResultColumn]
        public List<int> RegionSubList { get; set; }
    }
}
