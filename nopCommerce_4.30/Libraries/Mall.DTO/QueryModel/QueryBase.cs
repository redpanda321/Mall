using Mall.Entities;
using System;
using System.Linq.Expressions;

namespace Mall.DTO.QueryModel
{
    public partial class QueryBase
    {
        public QueryBase()
        {
            PageNo = 1;
            PageSize = 10;
        }

        /// <summary>
        /// 页号
        /// </summary>
        public int PageNo { get; set; }

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; }


        /// <summary>
        /// 排序属性
        /// </summary>
        public string Sort { get; set; } = string.Empty;

        /// <summary>
        /// 是否升序
        /// </summary>
        public bool IsAsc { get; set; }

        #region 列表插件兼容字段
        [Obsolete("插件兼容字段,请使用PageNo")]
        public int Page { get { return PageNo; } set { PageNo = value; } }
        [Obsolete("插件兼容字段,请使用PageSize")]
        public int Rows { get { return PageSize; } set { PageSize = value; } }
        #endregion

    }
}
