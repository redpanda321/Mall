using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FavoriteShopInfo
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long ShopId { get; set; }
        public string Tags { get; set; }
        public DateTime Date { get; set; }
    }
}
