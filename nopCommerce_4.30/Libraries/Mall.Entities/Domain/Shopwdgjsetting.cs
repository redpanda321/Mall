using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopWdgjSettingInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string UCode { get; set; }
        public string USign { get; set; }
        public byte IsEnable { get; set; }
    }
}
