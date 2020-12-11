
using Mall.Entities;
using System.Collections.Generic;
namespace Mall.Web.Areas.SellerAdmin.Models
{
    public class ProductModel
    {
        public long Id { get; set; }


        public decimal Price { get; set; }

        public decimal MinPrice { get; set; }

        public decimal MaxPrice { get; set; }

        public string Image { get; set; }

        public string QrCode { get; set; }
        public string Name { get; set; }

        public long Stock { get; set; }

        public string PublishTime { get; set; }

        public string BrandName { get; set; }
        public long BrandId
        {
            get;
            set;
        }
        public long CategoryId
        {
            get;
            set;
        }


        public string CategoryName { get; set; }

        public string Url { get; set; }

        public int AuditState { get; set; }

        public int SaleState { get; set; }

        public string AuditReason { get; set; }

        public string ProductCode { get; set; }
        public bool IsLimitTimeBuy { get; set; }
        /// <summary>
        /// 是否超出安全库存
        /// </summary>
        public bool IsOverSafeStock { get; set; }
        public long Uid { get; set; }

        public long SaleCount { get; set; }

        public string Unit { get; set; }

        public ICollection<SKUInfo> SKUInfo { get; set; }

        public List<SKuModel> Skus { get; set; }

        public string RelationProducts { get; set; }

        /// <summary>
        /// 最大购买数
        /// </summary>
        public int MaxBuyCount { get; set; }
        /// <summary>
        /// 市场价
        /// </summary>
        public decimal MarketPrice { get; set; }
        /// <summary>
        /// 商家商品序号
        /// </summary>
        public int ShopDisplaySequence { get; set; }
        public long? VirtualSaleCounts { get; set; }

        /// <summary>
        /// 是否开启阶梯价格
        /// </summary>
        public bool IsOpenLadder { get; set; }
        /// <summary>
        /// 商品类型(0=实物，1=虚拟)
        /// </summary>
        public sbyte ProductType { get; set; }
    }

    public class SKuModel
    {
        public string ProductName { get; set; }
        public string Sku { get; set; }
        public string Id { get; set; }
        public long AutoId { get; set; }
        public long Stock { get; set; }
        /// <summary>
        /// 商城价
        /// </summary>
        public decimal SalePrice { get; set; }
        /// <summary>
        /// 市场价
        /// </summary>
        public decimal MarketPrice { get; set; }
        /// <summary>
        /// 是否第一个
        /// </summary>
        public int IsFirst { get; set; }
        public long ProductId { get; set; }
        public int HasSKU { get; set; }
    }

    public class StockModel
    {
        public string SkuId { get; set; }
        public long AutoId { get; set; }
        public long Stock { get; set; }
    }
    public class PriceModel
    {
        public long AutoId { get; set; }
        /// <summary>
        /// 商城价
        /// </summary>
        public decimal SalePrice { get; set; }
        public int HasSku { get; set; }
        public long ProductId { get; set; }
    }
}