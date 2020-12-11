using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShippingFreeRegionInfo
    {
        public long Id { get; set; }
        public long TemplateId { get; set; }
        public long GroupId { get; set; }
        public int RegionId { get; set; }
        public string RegionPath { get; set; }
    }
}
