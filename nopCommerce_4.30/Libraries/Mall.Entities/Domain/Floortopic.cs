using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FloorTopicInfo
    {
        public long Id { get; set; }
        public long FloorId { get; set; }
        public int TopicType { get; set; }
        public string TopicImage { get; set; }
        public string TopicName { get; set; }
        public string Url { get; set; }
    }
}
