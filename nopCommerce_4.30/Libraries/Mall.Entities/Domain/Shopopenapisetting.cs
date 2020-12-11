using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopOpenApiSettingInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string AppKey { get; set; }
        public string AppSecreat { get; set; }
        public DateTime AddDate { get; set; }
        public DateTime LastEditDate { get; set; }
        public byte IsEnable { get; set; }
        public byte IsRegistered { get; set; }
    }
}
