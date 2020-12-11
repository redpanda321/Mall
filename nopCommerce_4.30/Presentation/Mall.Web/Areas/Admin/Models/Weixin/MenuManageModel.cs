using System;
using System.Collections.Generic;

namespace Mall.Web.Areas.Admin.Models
{
    public class MenuManageModel
    {
        public long ID { get; set; }
        public string TopMenuName { get; set; }

        public IEnumerable<Entities.MenuInfo> SubMenu { get; set; }

        public Nullable<Entities.MenuInfo.UrlTypes> LinkType { get; set; }

        public string URL { get; set; }
    }
}