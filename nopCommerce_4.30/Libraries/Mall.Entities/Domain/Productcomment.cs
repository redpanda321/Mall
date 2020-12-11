using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ProductCommentInfo
    {
        public long Id { get; set; }
        public long SubOrderId { get; set; }
        public long ProductId { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ReviewContent { get; set; }
        public DateTime ReviewDate { get; set; }
        public int ReviewMark { get; set; }
        public string ReplyContent { get; set; }
        public DateTime? ReplyDate { get; set; }
        public string AppendContent { get; set; }
        public DateTime? AppendDate { get; set; }
        public string ReplyAppendContent { get; set; }
        public DateTime? ReplyAppendDate { get; set; }
        public byte IsHidden { get; set; }
    }
}
