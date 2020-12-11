using Mall.CommonModel;
using Mall.DTO.Distribution;
using Mall.Entities;
using System;

namespace Mall.Web.Areas.SellerAdmin.Models
{
    public class DistributionProductListModel
    {
        /// <summary>
        /// 编号
        ///</summary>
        public long BrokerageId { get; set; }
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
        /// <summary>
        /// 一级分佣比
        ///</summary>
        public decimal BrokerageRate1 { get; set; }
        /// <summary>
        /// 二级分佣比
        ///</summary>
        public decimal BrokerageRate2 { get; set; }
        /// <summary>
        /// 三级分佣比
        ///</summary>
        public decimal BrokerageRate3 { get; set; }
        /// <summary>
        /// 成交件数
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
        public decimal MaxBrokerage { get; set; }
        /// <summary>
        /// 所属分类
        /// </summary>
        public long CategoryId { get; set; }
        /// <summary>
        /// 所属分类名称
        /// </summary>
        public string CategoryName { get; set; }
    }
}