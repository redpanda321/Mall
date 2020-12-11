using System;

namespace Mall.DTO.QueryModel
{
    /// <summary>
    /// 消息搜索条件
    /// </summary>
    public class AppMessageQuery : QueryBase
    {
        /// <summary>
        /// 商家编号
        /// </summary>
        public long? ShopId { get; set; }
        /// <summary>
        /// 门店编号
        /// </summary>
        public long? ShopBranchId { get; set; }
        /// <summary>
        /// 消息类型
        /// </summary>
        public int? MessageType { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 是否已读
        /// </summary>
        public bool? IsRead { get; set; }        
    }
}
