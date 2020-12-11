using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO.QueryModel
{
    public partial class CommentQuery : QueryBase
    {
        public bool? IsReply { set; get; }

        //店铺ID
        public long ShopID { set; get; }
        //关键字
        public string KeyWords { set; get; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { set; get; }

        public int Rank { set; get; } = -1;
        /// <summary>
        /// 是否有追加评论
        /// </summary>
        public bool HasAppend { set; get; }

        //用户ID
        public long UserID { set; get; }

        //产品ID
        public long ProductID { set; get; }

        public bool? IsHidden { get; set; }
    }

}
