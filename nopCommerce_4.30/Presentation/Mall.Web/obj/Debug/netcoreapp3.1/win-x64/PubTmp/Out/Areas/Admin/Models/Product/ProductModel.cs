
using System;
using System.Collections.Generic;

namespace Mall.Web.Areas.Admin.Models.Product
{
    public class ProductModel
    {
        public long id { get; set; }
        public string name { get; set; }

        /// <summary>
        /// 平台分类名称(虚拟或实物)
        /// </summary>
        public string categoryName { get; set; }
        public string brandName { get; set; }
        public decimal price { get; set; }
        public string state { get; set; }
        public string imgUrl { get; set; }

        /// <summary>
        /// 审核状态值
        /// </summary>
        public int auditStatus { get; set; }
        public string url { get; set; }
        public string auditReason { get; set; }

        public string shopName { get; set; }
        /// <summary>
        /// 销量
        /// </summary>
        public long saleCounts { get; set; }
        public int saleStatus { get; set; }

        public string productCode { get; set; }
        public string AddedDate { get; set; }
        public long DisplaySequence { get; set; }
        public long? VirtualSaleCounts { get; set; }
        public sbyte ProductType { get; set; }

        public long stock { get; set; }

        public decimal MinPrice { get; set; }

        public decimal MaxPrice { get; set; }


        public bool HasSKU { get; set; }

        ///// <summary>
        ///// 审核状态
        ///// </summary>
        //public Entities.ProductInfo.ProductAuditStatus AuditStatus { get; set; }

        /// <summary>
        /// 审核状态文本
        /// </summary>
        public string AuditStatusText { get; set; }

        /// <summary>
        /// 审核备注
        /// </summary>
        public string AuditReason { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string MeasureUnit { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public List<Entities.SKUInfo> SKUInfo { get; set; }

        /// <summary>
        /// 商家分类名称
        /// </summary>
        public string ShopCategoryName { get; set; }

        /// <summary>
        /// 运费模板名称
        /// </summary>
        public string FreightTemplateName { get; set; }

        /// <summary>
        /// 是否开启阶梯批发
        /// </summary>
        public bool IsOpenLadder { get; set; }

        /// <summary>
        /// 阶梯价
        /// </summary>
        public List<Entities.ProductLadderPriceInfo> ProductLadderPrice { get; set; }

        /// <summary>
        /// 商品类型(虚拟或实物)
        /// </summary>
        public string ProductTypeName { get; set; }

        /// <summary>
        /// 虚拟商品实体
        /// </summary>
        public Entities.VirtualProductInfo VirtualProduct { get; set; }

        /// <summary>
        /// 市场价
        /// </summary>
        public decimal MarketPrice { get; set; }

        /// <summary>
        /// 门店ID
        /// </summary>
        public long shopBranchId { get; set; }
    }
}