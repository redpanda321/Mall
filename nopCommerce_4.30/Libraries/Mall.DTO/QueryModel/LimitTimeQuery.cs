using Mall.Entities;
using System;

namespace Mall.DTO.QueryModel
{
    public partial class LimitTimeQuery : QueryBase
    {
        public string ShopName { get; set; }
        public long? ShopId { get; set; }
        public string Title { get; set; }
        public string ProductName { get; set; }
        public int? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public partial class FlashSaleQuery : QueryBase
    {
        public FlashSaleInfo.FlashSaleStatus? AuditStatus { get; set; }

        
        public string ShopName { get; set; }
        public long? ShopId { get; set; }
        public string ItemName { get; set; }
        public string CategoryName { get; set; }
        public int OrderType { get; set; }
        public int OrderKey { get; set; }
        /// <summary>
        /// 是否查询预热条件
        /// </summary>
        public bool IsPreheat { set; get; }
        public int IsStart { get; set; }
        public bool CheckProductStatus { get; set; }
    }
}
