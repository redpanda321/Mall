using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class AttributeValueInfo
    {
        public long Id { get; set; }
        public long AttributeId { get; set; }
        public string Value { get; set; }
        public long DisplaySequence { get; set; }
    }
}
