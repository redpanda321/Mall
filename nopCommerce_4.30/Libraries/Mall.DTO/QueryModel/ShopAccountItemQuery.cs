using Mall.CommonModel;
using System;

namespace Mall.DTO.QueryModel
{
    /// <summary>
    /// 店铺收支明细查询实体
    /// </summary>
    public class ShopAccountItemQuery:QueryBase
    {
        /// <summary>
        /// 店铺ID
        /// </summary>
        public long? ShopId { set; get; }
		/// <summary>
		/// 店铺名称
		/// </summary>
		public string ShopName { get; set; }
        /// <summary>
        /// 店铺收支类型
        /// </summary>
        public ShopAccountType? ShopAccountType { set; get; }

        /// <summary>
        /// 是否收入
        /// </summary>
        public bool? IsIncome { set; get; }


        /// <summary>
        /// 收支时间区间起始
        /// </summary>

        public DateTime? TimeStart { set; get; }

        /// <summary>
        /// 收支时间区间结束
        /// </summary>
        public DateTime? TimeEnd { set; get; }

    }

    /// <summary>
    /// 平台收支明细查询实体
    /// </summary>
    public class PlatAccountItemQuery : QueryBase
    {
        /// <summary>
        /// 类型
        /// </summary>
        public PlatAccountType? PlatAccountType { set; get; }

        /// <summary>
        /// 收支时间区间起始
        /// </summary>

        public DateTime? TimeStart { set; get; }

        /// <summary>
        /// 收支时间区间结束
        /// </summary>
        public DateTime? TimeEnd { set; get; }
    }
}
