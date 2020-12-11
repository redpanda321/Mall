using System;
using System.Collections.Generic;

namespace Mall.API.Model
{
    public class ProductDetailModelForMobie
    {
        public ProductInfoModel Product { get; set; }
        public ShopInfoModel Shop { get; set; }
        public CollectionSKU Color { get; set; }
        public CollectionSKU Size { get; set; }
        public CollectionSKU Version { get; set; }
        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }
        public string ShowPrice { get; set; }
        public int MaxSaleCount { get; set; }
        public string Title { get; set; }
        public DTO.CashDepositsObligation CashDepositsServer { get; set; }
        public string ProductAddress { get; set; }
        public Mall.Entities.FreightTemplateInfo FreightTemplate { get; set; }
        public string VShopLog { get; set; }

        /// <summary>
        /// 商品运费文本
        /// </summary>
        public string FreightStr { get; set; }

    }

    public class ProductInfoModel
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        public long ProductId { get; set; }
        /// <summary>
        /// 商品状态
        /// </summary>
        public Entities.ProductInfo.ProductSaleStatus ProductSaleStatus { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public Entities.ProductInfo.ProductAuditStatus AuditStatus { get; set; }
        /// <summary>
        /// 商品图片地址
        /// </summary>
        public List<string> ImagePath { get; set; }
        /// <summary>
        /// 主图缩略图，用于分享
        /// </summary>
        public List<string> ThumbnailPath { get; set; }
        /// <summary>
        /// 商品名
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 商品市场价
        /// </summary>
        public decimal MarketPrice { get; set; }
        /// <summary>
        /// 商品的简单描述
        /// </summary>
        public string ShortDescription { get; set; }

        /// <summary>
        /// 商品描述
        /// </summary>
        public string ProductDescription { get; set; }
        /// <summary>
        /// 商品最低价
        /// </summary>
        public decimal MinSalePrice { get; set; }

        /// <summary>
        /// 限时购价格
        /// </summary>
        public decimal LimitBuyPrice { get; set; }

        /// <summary>
        /// 限时购价格（区间价）
        /// </summary>
        public string LimitBuyPriceInterval { get; set; }
        /// <summary>
        /// 是否收藏
        /// </summary>
        public bool IsFavorite { get; set; }

        /// <summary>
        /// 关注数
        /// </summary>
        public int Favorites { get; set; }
        /// <summary>
        /// 咨询数
        /// </summary>
        public int Consultations { get; set; }
        /// <summary>
        /// 商品评论数
        /// </summary>
        public int CommentCount { get; set; }
        /// <summary>
        /// 好评率
        /// </summary>
        public int NicePercent { get; set; }

        /// <summary>
        /// 是否真正参与限时购
        /// </summary>
        public bool IsOnLimitBuy { get; set; }
        public long SaleCounts { get; set; }
        public string MeasureUnit { get; set; }

        /// <summary>
        /// 是否开启阶梯价
        /// </summary>
        public bool IsOpenLadder { get; set; }

        /// <summary>
        /// 最小批量
        /// </summary>
        public int MinMath { get; set; }
        public bool IsSaleCountOnOff { get; set; }
        /// <summary>
        /// 主图视频
        /// </summary>
        public string VideoPath { get; set; }

    }
    public class CollectionSKU : List<ProductSKU>
    {
        public override string ToString()
        {
            string str = "";
            foreach (var item in this)
            {
                str += item.Value + ",";
            }
            return str.TrimEnd(',');
        }
    }

    public class ProductSKU
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public long SkuId { get; set; }
        public string EnabledClass { get; set; }
        public string SelectedClass { get; set; }
        public string Img { get; set; }
    }

    public class HotProductInfo
    {
        public string ImgPath { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int SaleCount { get; set; }
        public long Id { get; set; }

    }

    public class ShopInfoModel
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public decimal ProductMark { get; set; }
        public decimal PackMark { get; set; }
        public decimal ServiceMark { get; set; }
        public decimal ComprehensiveMark { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        public decimal FreeFreight { get; set; }

        /// <summary>
        /// 店铺在售商品数
        /// </summary>
        public long ProductNum { get; set; }

        /// <summary>
        /// 店铺优惠券数
        /// </summary>
        public long CouponCount { get; set; }

        /// <summary>
        /// 微店Id
        /// </summary>
        public long VShopId { get; set; }

        /// <summary>
        /// 宝贝描述得分
        /// </summary>
        public decimal ProductAndDescription { get; set; }
        /// <summary>
        /// 商家服务得分
        /// </summary>
        public decimal SellerServiceAttitude { get; set; }
        /// <summary>
        /// 发货物流得分
        /// </summary>
        public decimal SellerDeliverySpeed { get; set; }

        public long FavoriteShopCount { get; set; }
        public bool IsFavorite { get; set; }
    }

    public class VirtualProductModel
    {
        /// <summary>
        /// 有效期类型，0长期有效，1自定义日期
        /// </summary>
        public int ValidityType { get; set; }
        /// <summary>
        /// 自定义开始时间
        /// </summary>
        public string StartDate { get; set; }
        /// <summary>
        /// 自定义结束时间
        /// </summary>
        public string EndDate { get; set; }
        /// <summary>
        /// 支持退款类型(1=支持有效期内退款,2=过期和有效期内都可退,3=不支持退款)
        /// </summary>
        public sbyte SupportRefundType { get; set; }
        public string UseNotice { get; set; }
        /// <summary>
        /// 商品是否过期
        /// </summary>
        public bool IsOverdue { get; set; }
        public sbyte EffectiveType { get; set; }
        public int Hour { get; set; }
    }

    public class VirtualProductItemModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Entities.ProductInfo.VirtualProductItemType Type { get; set; }
        public bool Required { get; set; }

    }
}