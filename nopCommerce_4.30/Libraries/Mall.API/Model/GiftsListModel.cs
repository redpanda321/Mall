using Mall.DTO;
using System.Collections.Generic;

namespace Mall.API.Model
{
    /// <summary>
    /// 获取礼品列表模型
    /// </summary>
    public class GiftsListModel
    {
        public bool success { get; set; }
        /// <summary>
        /// 礼品列表
        /// <para>一页数据</para>
        /// </summary>
        public List<GiftModel> DataList { get; set; }
        /// <summary>
        /// 礼品每页显示量
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 礼品总数
        /// </summary>
        public int total { get; set; }
        /// <summary>
        /// 礼品总页数
        /// </summary>
        public int MaxPage { get; set; }
    }
}
