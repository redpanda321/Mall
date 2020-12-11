using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class InvoiceTitleInfo
    {

        /// <summary>
        /// 地址全路径名称
        /// </summary>
        [ResultColumn]
        public string RegionFullName { get; set; }

        /// <summary>
        /// 发票寄出天数
        /// </summary>
        [ResultColumn]
        public string InvoiceDay { get; set; }
    }
}
