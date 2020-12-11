using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class WeixinMsgTemplateInfo
    {
        public long Id { get; set; }
        public int MessageType { get; set; }
        public string TemplateNum { get; set; }
        public string TemplateId { get; set; }
        public DateTime UpdateDate { get; set; }
        public byte IsOpen { get; set; }
        public byte UserInWxApplet { get; set; }
    }
}
