using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class OrderItemInfo
    {
        [ResultColumn]
        public string ProductCode { get; set; }

        [ResultColumn]
        public long FreightId { set; get; }

        [ResultColumn]
        //总共的运费
        public decimal Freight { set; get; }

        [ResultColumn]
        public string ColorAlias { get; set; }
        [ResultColumn]
        public string SizeAlias { get; set; }
        [ResultColumn]
        public string VersionAlias { get; set; }
        /// <summary>
        /// 平台佣金
        /// </summary>
        [ResultColumn]
        public decimal PlatCommission { get; set; }
    }
}
