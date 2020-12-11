using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopHomeModuleInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string Name { get; set; }
        public byte IsEnable { get; set; }
        public int DisplaySequence { get; set; }
        public string Url { get; set; }
    }
}
