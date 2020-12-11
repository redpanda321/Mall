using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class ShopLicenseCert
    {
        public long ShopId { get; set; }
        public string BusinessLicenseCert { get; set; }
        public string ProductCert { get; set; }
        public string OtherCert { get; set; }
    }
}
