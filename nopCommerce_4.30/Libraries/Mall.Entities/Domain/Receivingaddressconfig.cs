using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ReceivingAddressConfigInfo
    {
        public long Id { get; set; }
        public string AddressIdCity { get; set; }
        public string AddressId { get; set; }
        public long ShopId { get; set; }
    }
}
