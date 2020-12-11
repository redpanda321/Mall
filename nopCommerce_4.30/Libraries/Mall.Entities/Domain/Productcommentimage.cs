using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ProductCommentImageInfo
    {
        public long Id { get; set; }
        public string CommentImage { get; set; }
        public long CommentId { get; set; }
        public int CommentType { get; set; }
    }
}
