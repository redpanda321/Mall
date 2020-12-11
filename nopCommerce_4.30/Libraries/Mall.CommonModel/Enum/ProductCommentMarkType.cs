using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Mall.CommonModel
{
    /// <summary>
    /// 商品评价搜索状态
    /// </summary>
    public enum ProductCommentMarkType
    {
        /// <summary>
        /// 默认
        /// </summary>
        [Description("默认")]
        Default = 0,
        /// <summary>
        /// 差评
        /// </summary>
        [Description("差评")]
        Low = 2,
        /// <summary>
        /// 中评
        /// </summary>
        [Description("中评")]
        Medium = 3,
        /// <summary>
        /// 好评
        /// </summary>
        [Description("好评")]
        High = 4,
        /// <summary>
        /// 有图
        /// </summary>
        [Description("有图")]
        HasImage = 11,
        /// <summary>
        /// 追评
        /// </summary>
        [Description("追评")]
        Append = 6,
    }
}
