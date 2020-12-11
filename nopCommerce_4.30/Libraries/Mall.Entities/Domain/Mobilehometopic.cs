using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MobileHomeTopicInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public int Platform { get; set; }
        public long TopicId { get; set; }
        public int Sequence { get; set; }
    }
}
