using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MessageLogInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string TypeId { get; set; }
        public string MessageContent { get; set; }
        public DateTime SendTime { get; set; }
    }
}
