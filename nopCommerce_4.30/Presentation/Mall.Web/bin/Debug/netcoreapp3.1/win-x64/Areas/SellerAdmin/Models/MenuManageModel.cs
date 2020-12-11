
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Web.Areas.SellerAdmin.Models
{
    public class MenuManageModel
    {
        public long ID { get; set; }
        public string TopMenuName { get; set; }

        public List<Entities.MenuInfo> SubMenu { get; set; }

        public Nullable<Entities.MenuInfo.UrlTypes> LinkType { get; set; }

        public string URL { get; set; }
    }
}