using System;

namespace Mall.DTO
{
    /// <summary>
    /// 商品统计
    /// </summary>
    public class ProductCategoryStatistic
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public long SaleCounts { get; set; }
        public decimal SaleAmounts { get; set; }
        /// <summary>
        /// 金额份额
        /// </summary>
        public decimal AmountRate { get; set; }
        /// <summary>
        /// 销售量份额
        /// </summary>
        public decimal CountRate { get; set; }
    }
}
