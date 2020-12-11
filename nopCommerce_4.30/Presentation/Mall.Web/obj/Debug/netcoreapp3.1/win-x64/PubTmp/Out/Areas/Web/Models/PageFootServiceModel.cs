using System.Collections.Generic;

namespace Mall.Web.Areas.Web.Models
{
    public class PageFootServiceModel
    {
        public string CateogryName { get; set; }

        public IEnumerable<Mall.Entities.ArticleInfo> Articles { get; set; }

    }
}