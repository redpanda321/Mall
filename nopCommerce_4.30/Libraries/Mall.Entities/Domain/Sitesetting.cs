using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class SiteSettingInfo
    {
        public long Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
