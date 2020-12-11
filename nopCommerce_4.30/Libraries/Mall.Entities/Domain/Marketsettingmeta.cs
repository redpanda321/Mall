using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MarketSettingMetaInfo
    {
        public int Id { get; set; }
        public int MarketId { get; set; }
        public string MetaKey { get; set; }
        public string MetaValue { get; set; }
    }
}
