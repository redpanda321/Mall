using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class CityExpressConfigInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public byte IsEnable { get; set; }
        public string SourceId { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
    }
}
