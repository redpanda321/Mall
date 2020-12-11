using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class HomeFloorInfo
    {
        public long Id { get; set; }
        public string FloorName { get; set; }
        public string SubName { get; set; }
        public long DisplaySequence { get; set; }
        public byte IsShow { get; set; }
        public int StyleLevel { get; set; }
        public string DefaultTabName { get; set; }
        public int CommodityStyle { get; set; }
        public int DisplayMode { get; set; }
    }
}
