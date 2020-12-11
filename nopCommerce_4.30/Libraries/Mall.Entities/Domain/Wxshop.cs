using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class WxShopInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string Token { get; set; }
        public string FollowUrl { get; set; }
    }
}
