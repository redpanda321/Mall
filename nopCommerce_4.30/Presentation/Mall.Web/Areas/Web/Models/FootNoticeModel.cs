using Mall.Entities;
using System.Collections.Generic;

namespace Mall.Web.Areas.Web.Models
{
    public class FootNoticeModel
    {
        public string CateogryName { get; set; }
        public List<ArticleInfo> List { get; set; }
    }
}