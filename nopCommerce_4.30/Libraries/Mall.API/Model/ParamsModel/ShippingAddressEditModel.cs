using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.API.Model.ParamsModel
{
    public class ShippingAddressEditModel
    {
        public long id { get; set; }
        public int regionId { get; set; }
        /// <summary>
        /// 小区、大厦
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 楼栋门牌
        /// </summary>
        public string addressDetail { get; set; }
        public string phone { get; set; }
        public string shipTo { get; set; }
        public float longitude { get; set; }
        public float latitude { get; set; }
        /// <summary>
        /// 业务字段，门店ID
        /// </summary>
        public long shopbranchid { get; set; }
    }
}
