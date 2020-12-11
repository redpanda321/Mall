using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FreightAreaContentInfo
    {
        public long Id { get; set; }
        public long FreightTemplateId { get; set; }
        public string AreaContent { get; set; }
        public int FirstUnit { get; set; }
        public float FirstUnitMonry { get; set; }
        public int AccumulationUnit { get; set; }
        public float AccumulationUnitMoney { get; set; }
        public byte IsDefault { get; set; }
    }
}
