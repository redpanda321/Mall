using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class PaymentConfigInfo
    {
        public long Id { get; set; }
        public byte IsCashOnDelivery { get; set; }
    }
}
