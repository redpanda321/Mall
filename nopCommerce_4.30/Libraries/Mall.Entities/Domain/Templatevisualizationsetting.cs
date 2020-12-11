using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class TemplateVisualizationSettingInfo
    {
        public long Id { get; set; }
        public string CurrentTemplateName { get; set; }
        public long ShopId { get; set; }
    }
}
