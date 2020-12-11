using System.Collections.Generic;

namespace Mall.Web.Areas.Admin.Models
{
    public class FootNoticeModel
    {
        public string CateogryName { get; set; }
        public List<Entities.ArticleInfo> List { get; set; }
    }
}