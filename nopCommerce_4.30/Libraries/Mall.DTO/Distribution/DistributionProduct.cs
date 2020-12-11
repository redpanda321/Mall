using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.Core;
using Mall.CommonModel;

namespace Mall.DTO.Distribution
{
    /// <summary>
    /// 分销商品状态
    /// </summary>
    public class DistributionProduct
    {
        /// <summary>
        /// 编号
        ///</summary>
        public long Id { get; set; }
        /// <summary>
        /// 商品编号
        ///</summary>
        public long ProductId { get; set; }
        /// <summary>
        /// 店铺编号
        ///</summary>
        public long ShopId { get; set; }
        /// <summary>
        /// 商品分销状态
        ///</summary>
        public DistributionProductStatus ProductStatus { get; set; }
        public string ShowProductStatus
        {
            get
            {
                return ProductStatus.ToDescription();
            }
        }
        /// <summary>
        /// 一级分佣比
        /// <para>使用时请除100</para>
        ///</summary>
        public decimal BrokerageRate1 { get; set; }
        /// <summary>
        /// 二级分佣比
        /// <para>使用时请除100</para>
        ///</summary>
        public decimal BrokerageRate2 { get; set; }
        /// <summary>
        /// 三级分佣比
        /// <para>使用时请除100</para>
        ///</summary>
        public decimal BrokerageRate3 { get; set; }
        /// <summary>
        /// 成交件数(销量)
        ///</summary>
        public long SaleCount { get; set; }
        /// <summary>
        /// 成交金额
        ///</summary>
        public decimal SaleAmount { get; set; }
        /// <summary>
        /// 已结算金额
        ///</summary>
        public decimal SettlementAmount { get; set; }
        /// <summary>
        /// 未结算金额
        /// </summary>
        public decimal NoSettlementAmount { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 最低售价
        /// </summary>
        public decimal MinSalePrice { get; set; }
        /// <summary>
        /// 最高售价
        /// </summary>
        public decimal MaxSalePrice { get; set; }
        /// <summary>
        /// 最高赚
        /// </summary>
        public decimal MaxBrokerage
        {
            get
            {
                decimal result = 0;
                decimal maxrate = BrokerageRate1;
                if (BrokerageRate2 > maxrate && MaxDistributionLevel > 1)
                {
                    maxrate = BrokerageRate2;
                }
                if (BrokerageRate3 > maxrate && MaxDistributionLevel > 2)
                {
                    maxrate = BrokerageRate3;
                }
                result = decimal.Parse((MaxSalePrice * maxrate / (decimal)100).ToString("F2"));
                return result;
            }
        }
        /// <summary>
        /// 图片目录
        /// </summary>
        public string ImagePath { get; set; }
        /// <summary>
        /// 默认图
        /// </summary>
        public string DefaultImage { get; set; }
        /// <summary>
        /// 库存
        /// </summary>
        public long Stock { get; set; }
        /// <summary>
        /// 广告语
        /// </summary>
        public string ShortDescription { get; set; }
        /// <summary>
        /// 最大分销级
        /// </summary>
        public int MaxDistributionLevel { get; set; }
    }
}
