using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Web.Areas.SellerAdmin.Models
{
    public class CouponsContent
    {
        public long game_id { get; set; }
        public string title { get; set; }
        public string create_time { get; set; }
        public string link { get; set; }
        public string pc_link { get; set; }
        public int type { get; set; }
        public string price { get; set; }
        public string shopName { get; set; }
        public string condition { get; set; }
        public string endTime { get; set; }
    }
}