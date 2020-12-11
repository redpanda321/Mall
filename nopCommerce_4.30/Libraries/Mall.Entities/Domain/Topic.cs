using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class TopicInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string FrontCoverImage { get; set; }
        public string TopImage { get; set; }
        public string BackgroundImage { get; set; }
        public int PlatForm { get; set; }
        public string Tags { get; set; }
        public long ShopId { get; set; }
        public byte IsRecommend { get; set; }
        public string SelfDefineText { get; set; }
    }
}
