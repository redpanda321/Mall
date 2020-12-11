using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class TopicModuleInfo
    {
        public long Id { get; set; }
        public long TopicId { get; set; }
        public string Name { get; set; }
        public int TitleAlign { get; set; }
    }
}
