using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopHomeModuleProductInfo
    {
        public long Id { get; set; }
        public long HomeModuleId { get; set; }
        public long ProductId { get; set; }
        public int DisplaySequence { get; set; }
    }
}
