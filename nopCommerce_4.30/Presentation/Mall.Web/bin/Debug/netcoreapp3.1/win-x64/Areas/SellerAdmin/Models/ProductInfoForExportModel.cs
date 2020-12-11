
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Web.Areas.SellerAdmin.Models
{
    public class ProductInfoForExportModel
    {
        public long Id { get; set; }
        public string CategoryName { get; set; }

        //public long TypeId { get; set; }
        public string BrandName { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string ShortDescription { get; set; }
        public Entities.ProductInfo.ProductSaleStatus SaleStatus { get; set; }
        public System.DateTime AddedDate { get; set; }
        public decimal MarketPrice { get; set; }
        public decimal MinSalePrice { get; set; }
        public bool HasSKU { get; set; }
        public long VistiCounts { get; set; }
        public long SaleCounts { get; set; }
        public Entities.ProductInfo.ProductAuditStatus AuditStatus { get; set; }
        public string AuditStatusText { get; set; }

        public string AuditReason { get; set; }
        //public Nullable<decimal> Weight { get; set; }
        //public Nullable<decimal> Volume { get; set; }
        public int? Quantity { get; set; }
        public string MeasureUnit { get; set; }
        //public int EditStatus { get; set; }

        public List<Entities.SKUInfo> SKUInfo { get; set; }

        /// <summary>
        /// 平台分类名称(虚拟或实物)
        /// </summary>
        public string PlatFormCategoryName{ get; set; }

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
    }
}