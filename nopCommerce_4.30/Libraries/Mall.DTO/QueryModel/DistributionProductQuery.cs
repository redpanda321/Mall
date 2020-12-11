using Mall.CommonModel;
using System.Collections.Generic;

namespace Mall.DTO.QueryModel
{
    public class DistributionProductQuery : QueryBase
    {
        /// <summary>
        /// 关键词
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 分销商品状态
        /// </summary>
        public DistributionProductStatus? Status { get; set; }
        /// <summary>
        /// 商家编号
        /// </summary>
        public long? ShopId { get; set; }
        /// <summary>
        /// 商家名称
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 分类
        /// </summary>
        public long? CategoryId { get; set; }

        public List<long> Categories { get; set; }
        /// <summary>
        /// 是否商家分类
        /// </summary>
        public bool isShopCategory { get; set; }
        /// <summary>
        /// 商品范围编号集
        /// </summary>
        public string ProductIds { get; set; }
    }
}
