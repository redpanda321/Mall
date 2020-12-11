using Mall.CommonModel;

namespace Mall.DTO.QueryModel
{
    public class ProductCommentQuery : QueryBase
    {
        public long? ShopId { set; get; }
        public long? ProductId { get; set; }
        public long? ShopBranchId { get; set; }
        /// <summary>
        /// 是否回复
        /// </summary>
        public bool? IsReply { get; set; }
        public ProductCommentMarkType? CommentType { set; get; }
        public bool ShowHidden { get; set; } = false;//默认不显示隐藏评论

        /// <summary>
        /// 查询条件是否为O2O
        /// </summary>
        public bool IsO2O { get; set; } = false;
    }
}
