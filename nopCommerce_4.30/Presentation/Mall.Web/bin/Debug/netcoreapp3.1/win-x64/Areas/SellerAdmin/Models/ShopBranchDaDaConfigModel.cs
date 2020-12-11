using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mall.Web.Areas.SellerAdmin.Models
{
    public class ShopBranchDaDaConfigModel
    {
        public bool IsEnable { get; set; }
        [StringLength(200, ErrorMessage = "不可大于200字符")]
        public string source_id { get; set; }
        [StringLength(200, ErrorMessage = "不可大于200字符")]
        public string app_key { get; set; }
        [StringLength(200, ErrorMessage = "不可大于200字符")]
        public string app_secret { get; set; }
    }
}