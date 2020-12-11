using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class SendMessageRecordInfo
    {
        public long Id { get; set; }
        public int MessageType { get; set; }
        public int ContentType { get; set; }
        public string SendContent { get; set; }
        public string ToUserLabel { get; set; }
        public int SendState { get; set; }
        public DateTime SendTime { get; set; }
    }
}
