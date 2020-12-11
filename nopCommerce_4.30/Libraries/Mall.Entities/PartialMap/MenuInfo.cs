using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class MenuInfo
    {
        /// <summary>
        /// 链接类型
        /// </summary>
        public enum UrlTypes
        {

            /// <summary>
            /// 不链接
            /// </summary>
            [Description("不链接")]
            Nothing = 0,

            /// <summary>
            /// 商城首页
            /// </summary>
            [Description("首页")]
            ShopHome = 1,
            /// <summary>
            /// 微店
            /// </summary>
            [Description("微店")]
            WeiStore = 2,

            /// <summary>
            /// 商城分类
            /// </summary>
            [Description("分类")]
            ShopCategory = 3,

            /// <summary>
            /// 个人中心
            /// </summary>
            [Description("个人中心")]
            MemberCenter = 4,

            /// <summary>
            /// 购物车
            /// </summary>
            [Description("购物车")]
            ShopCart = 5,
            /// <summary>   
            /// 链接
            /// </summary>
            [Description("链接")]
            Linkage = 6
        }
    }
}
