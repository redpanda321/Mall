using Mall.CommonModel;
using Mall.Core;
using Mall.Entities;
using System;
using System.Collections.Generic;

namespace Mall.DTO.Product
{
    public class Product
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public long CategoryId { get; set; }
        public string CategoryPath { get; set; }
        /// <summary>
        ///  一级分类ID
        /// </summary>
        public long CategoryTopId
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.CategoryPath))
                {
                    return 0;
                }
                long mainId = 0;
                if (long.TryParse(this.CategoryPath.Split('|')[0], out mainId))
                {
                    return mainId;
                }
                return 0;
            }

        }
        public long TypeId { get; set; }
        public long BrandId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string ShortDescription { get; set; }
        public Entities.ProductInfo.ProductSaleStatus SaleStatus { get; set; }
        public System.DateTime AddedDate { get; set; }
        public long DisplaySequence { get; set; }
        public decimal? MarketPrice { get; set; }
        public decimal MinSalePrice { get; set; }
        public bool HasSKU { get; set; }
        public long VistiCounts { get; set; }
        public long SaleCounts { get; set; }
        public long? VirtualSaleCounts { get; set; }
        public Entities.ProductInfo.ProductAuditStatus AuditStatus { get; set; }
        public long FreightTemplateId { get; set; }
        public Nullable<decimal> Weight { get; set; }
        public Nullable<decimal> Volume { get; set; }
        public Nullable<int> Quantity { get; set; }
        public string MeasureUnit { get; set; }
        public ProductInfo.ProductEditStatus EditStatus { get; set; }

        public int ConcernedCount { get; set; }

        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImagePath
        {
            get;
            set;
        }

        public string RelativePath
        {
            get;
            set;
        }
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 最大购买数
        /// </summary>
        public int MaxBuyCount { get; set; }

        /// <summary>
        /// 成交数
        /// </summary>
        public long OrderCounts { get; set; }

        /// <summary>
        /// 商品地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }

        public string BrandName
        {
            get;
            set;
        }

        /// <summary>
        /// 是否开启批价
        /// </summary>
        public bool IsOpenLadder { get; set; }
        public string VersionAlias { get; set; }
        public string SizeAlias { get; set; }
        public string ColorAlias { get; set; }
        public string VideoPath { get; set; }
        /// <summary>
        /// 商家商品序号
        /// </summary>
        public int ShopDisplaySequence { get; set; }
        /// <summary>
        /// 商品类型(0=实物商品，1=虚拟商品)
        /// </summary>
        public int ProductType { get; set; }

        #region 表单传参用
        public string CategoryNames { get; set; }

        public int IsCategory { get; set; }

        public long TopId { get; set; }

        public long BottomId { get; set; }
        #endregion

        public string ShowProductState
        {
            get
            {
                string result = "错误数据";
                if (this != null)
                {
                    if (this.AuditStatus == Entities.ProductInfo.ProductAuditStatus.WaitForAuditing)
                    {
                        result = (this.SaleStatus == Entities.ProductInfo.ProductSaleStatus.OnSale
                            ? Entities.ProductInfo.ProductAuditStatus.WaitForAuditing.ToDescription()
                            : Entities.ProductInfo.ProductSaleStatus.InStock.ToDescription());
                    }
                    else
                    {
                        result = this.AuditStatus.ToDescription();
                    }
                }
                return result;
            }
        }

        public ProductDescription Description
        {
            get;
            set;
        }

        public SKU[] SKUS
        {
            get;
            set;
        }

        public ProductAttribute[] Attributes
        {
            get;
            set;
        }

        public SpecificationValue[] Specifications
        {
            get;
            set;
        }

        /// <summary>
        /// 商品有效期类型(0=长期有效，1=自定义日期)
        /// </summary>
        public bool ValidityType { get; set; }
        /// <summary>
        /// 自定义开始日期
        /// </summary>
        public string StartDate { get; set; }
        /// <summary>
        /// 自定义结束日期
        /// </summary>
        public string EndDate { get; set; }
        /// <summary>
        /// 核销码生效类型
        /// </summary>
        public Entities.ProductInfo.VirtualProductEffectiveType EffectiveType { get; set; }
        /// <summary>
        /// 付款完成X小时有效
        /// </summary>
        public int? Hour { get; set; }

        /// <summary>
        /// 虚拟商品退款类型
        /// </summary>
        public Entities.ProductInfo.SupportVirtualRefundType SupportRefundType { get; set; }

        /// <summary>
        /// 使用须知
        /// </summary>
        public string UseNotice { get; set; }
        public Entities.VirtualProductItemInfo[] VirtualProductItemInfo
        {
            get;
            set;
        }

        /// <summary>
        /// 批量价集合
        /// </summary>
        public List<DTO.ProductLadderPrice> LadderPrices { get; set; }

        /// <summary>
        /// 获取图片地址 YZY修改获取不同尺寸图片的方法
        /// </summary>
        /// <param name="imageIndex">图片序号</param>
        /// <param name="imageSize">图片尺寸</param>
        /// <returns></returns>
        public string GetImage(ImageSize imageSize, int imageIndex = 1)
        {
            // return string.Format(ImagePath + "/{0}_{1}.png", imageIndex, (int)imageSize);
            return Core.MallIO.GetProductSizeImage(RelativePath, imageIndex, (int)imageSize);
        }
    }
}
