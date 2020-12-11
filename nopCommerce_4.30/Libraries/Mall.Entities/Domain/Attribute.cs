using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class AttributeInfo
    {
        public long Id { get; set; }
        public long TypeId { get; set; }
        public string Name { get; set; }
        public long DisplaySequence { get; set; }
        public byte IsMust { get; set; }
        public byte IsMulti { get; set; }
    }
}
