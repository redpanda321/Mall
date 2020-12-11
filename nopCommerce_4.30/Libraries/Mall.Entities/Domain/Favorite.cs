using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FavoriteInfo
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long ProductId { get; set; }
        public string Tags { get; set; }
        public DateTime Date { get; set; }
    }
}
