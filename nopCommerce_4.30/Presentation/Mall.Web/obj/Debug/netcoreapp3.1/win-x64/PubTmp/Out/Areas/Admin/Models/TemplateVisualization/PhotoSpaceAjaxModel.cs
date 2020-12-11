using System.Collections.Generic;

namespace Mall.Web.Areas.Admin.Models
{
    public class PhotoSpaceAjaxModel
    {
        public string status { get; set; }
        public List<photoContent> data { get; set; }
        public string msg { get; set; }
        public string page { get; set; }
    }


    public class photoContent
    {
        public string name { get; set; }
        public string file { get; set; }
        public string id { get; set; }
    }
}