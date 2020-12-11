using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class CustomerServiceInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public int Tool { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public string AccountCode { get; set; }
        public int TerminalType { get; set; }
        public int ServerStatus { get; set; }
    }
}
