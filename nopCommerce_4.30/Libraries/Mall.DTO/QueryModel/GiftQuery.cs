namespace Mall.DTO.QueryModel
{
    public partial class GiftQuery : QueryBase
    {
        /// <summary>
        /// 排序方式
        /// </summary>
        public enum GiftSortEnum
        {
            /// <summary>
            /// 默认按顺序+编号 排序
            /// </summary>
            Default = 0,
            /// <summary>
            /// 按销量(包括虚拟销量)
            /// </summary>
            SalesNumber = 1,
            /// <summary>
            /// 按真实销量
            /// </summary>
            RealSalesNumber = 2
        }
        /// <summary>
        /// 关键字
        /// </summary>
        public string skey { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public Entities.GiftInfo.GiftSalesStatus? status { get; set; }
        /// <summary>
        /// 是否显示删除
        /// </summary>
        public bool? isShowAll { get; set; }
    }
}
