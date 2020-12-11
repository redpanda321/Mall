using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class OrderCommentInfo
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CommentDate { get; set; }
        public int PackMark { get; set; }
        public int DeliveryMark { get; set; }
        public int ServiceMark { get; set; }
    }
}
