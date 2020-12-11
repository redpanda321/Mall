using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShippingFreeGroupInfo
    {
        public long Id { get; set; }
        public long TemplateId { get; set; }
        public int ConditionType { get; set; }
        public string ConditionNumber { get; set; }
    }
}
