using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopGradeInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int ProductLimit { get; set; }
        public int ImageLimit { get; set; }
        public int TemplateLimit { get; set; }
        public decimal ChargeStandard { get; set; }
        public string Remark { get; set; }
    }
}
