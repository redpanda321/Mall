using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ModuleProductInfo
    {
        public long Id { get; set; }
        public long ModuleId { get; set; }
        public long ProductId { get; set; }
        public long DisplaySequence { get; set; }
    }
}
