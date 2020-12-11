using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class AutoreplyInfo
    {
        public int Id { get; set; }
        public string RuleName { get; set; }
        public string Keyword { get; set; }
        public int MatchType { get; set; }
        public string TextReply { get; set; }
        public int IsOpen { get; set; }
        public int ReplyType { get; set; }
        public int ReplyContentType { get; set; }
        public string MediaId { get; set; }
    }
}
