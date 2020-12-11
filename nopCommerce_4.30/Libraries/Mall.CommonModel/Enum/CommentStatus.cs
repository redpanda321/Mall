using System.ComponentModel;

namespace Mall.CommonModel
{
    public enum CommentStatus
    {
        /// <summary>
        /// 已评论
        /// </summary>
        [Description("初评")]
         First = 0,
        /// <summary>
        /// 追加评论
        /// </summary>
        [Description("追加评论")]
        Append = 1,

        /// <summary>
        /// 已评
        /// </summary>
        [Description("已评")]
        Finshed = 2
    }
}
