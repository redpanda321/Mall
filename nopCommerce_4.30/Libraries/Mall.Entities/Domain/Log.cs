using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class LogInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string PageUrl { get; set; }
        public DateTime Date { get; set; }
        public string UserName { get; set; }
        public string Ipaddress { get; set; }
        public string Description { get; set; }
    }
}
