using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Mall.Web.Models;

namespace Mall.Web.Areas.SellerAdmin.Models
{
    /// <summary>
    /// 未使用
    /// </summary>
    public class ShopDetailViewModel
    {
        public ShopModel ShopModelInfo { get; set; }
        public string CompanyRegionIds { get; set; }
        public string BusinessLicenseCert { get; set; }
        public string[] BusinessLicenseCerts { get; set; }
        public string[] ProductCerts { get; set; }
        public string[] OtherCerts { get; set; }
    }
}