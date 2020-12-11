using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class WeixinBasicInfo
    {
        public long Id { get; set; }
        public string Ticket { get; set; }
        public DateTime TicketOutTime { get; set; }
        public string AppId { get; set; }
        public string AccessToken { get; set; }
    }
}
