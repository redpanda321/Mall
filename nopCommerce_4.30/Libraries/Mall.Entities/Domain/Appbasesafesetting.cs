using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class AppbaseSafeSettingInfo
    {
        public long Id { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
    }
}
