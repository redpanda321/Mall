using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Web.Models
{
    public class TemplateSetting
    {
        public int Id { get; set; }

        public string CurrentTemplateName { get; set; }

        public string Name { get; set; }
    }

    public class TemplateOtherInfo
    {
        public string ResultHtml { get; set; }
        public string FootHtml { get; set; }
        public string Script { get; set; }
        public string ThemeJson { get; set; }
    }
}