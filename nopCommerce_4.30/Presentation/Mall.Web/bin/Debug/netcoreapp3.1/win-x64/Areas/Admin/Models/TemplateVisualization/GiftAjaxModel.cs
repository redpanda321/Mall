using System.Collections.Generic;

namespace Mall.Web.Areas.Admin.Models
{
    public class GiftAjaxModel
    {
        public int status { get; set; }
        public List<GiftContent> list { get; set; }
        public string page { get; set; }
    }


    public class GiftContent
    {
        public long item_id { get; set; }
        public string title { get; set; }
        public string adddate { get; set; }
        public string enddate { get; set; }
        public string link { get; set; }
        public string pic { get; set; }
        public string needintegral { get; set; }
        public string limtquantity { get; set; }
        public string stockquantity { get; set; }
        public string realsales { get; set; }
    }
}