using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class OpenIdInfo
    {
        public long Id { get; set; }
        public string OpenId1 { get; set; }
        public DateTime SubscribeTime { get; set; }
        public byte IsSubscribe { get; set; }
    }
}
