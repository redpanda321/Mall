using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class SpecificationValueInfo
    {
        public long Id { get; set; }
        public int Specification { get; set; }
        public long TypeId { get; set; }
        public string Value { get; set; }
    }
}
