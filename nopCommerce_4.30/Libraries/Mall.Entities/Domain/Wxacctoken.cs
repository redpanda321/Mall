using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class WxAccTokenInfo
    {
        public long Id { get; set; }
        public string AppId { get; set; }
        public string AccessToken { get; set; }
        public DateTime TokenOutTime { get; set; }
    }
}
