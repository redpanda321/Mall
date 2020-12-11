using System;

namespace Mall.DTO.QueryModel
{
    /// <summary>
    /// 门店搜索
    /// </summary>
    public partial class BranchShopDayFeatsQuery : QueryBase
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate { get; set; }

    
        /// <summary>
        /// 店铺ID
        /// </summary>
        public long ShopId { set; get; } 

        /// <summary>
        /// 门店ID
        /// </summary>
        public long BranchShopId { get; set; }
    }
}
