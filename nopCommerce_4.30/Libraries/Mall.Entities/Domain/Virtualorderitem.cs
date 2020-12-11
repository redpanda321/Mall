using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class VirtualOrderItemInfo
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long OrderItemId { get; set; }
        public string VirtualProductItemName { get; set; }
        public string Content { get; set; }
        public byte VirtualProductItemType { get; set; }
    }
}
