using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetRube.Data;
using NPoco;

namespace Mall.Entities
{
    public partial class OrderVerificationCodeInfo
    {
        /// <summary>
        /// 核销码二维码图
        /// </summary>
        public string QRCode { get; set; }

        public string SourceCode { get; set; }

        [ResultColumn]
        public DateTime? PayDate { get; set; }

        [ResultColumn]
        public long ShopId { get; set; }

        [ResultColumn]
        public long ShopBranchId { get; set; }
    }
}
