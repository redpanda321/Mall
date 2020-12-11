using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FreightTemplateInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int SourceAddress { get; set; }
        public string SendTime { get; set; }
        public int IsFree { get; set; }
        public int ValuationMethod { get; set; }
        public int? ShippingMethod { get; set; }
        public long ShopId { get; set; }
    }
}
