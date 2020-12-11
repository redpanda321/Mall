using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class StatisticOrderCommentInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public int CommentKey { get; set; }
        public decimal CommentValue { get; set; }
    }
}
