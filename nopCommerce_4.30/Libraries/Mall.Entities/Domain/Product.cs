using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ProductInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public long CategoryId { get; set; }
        public string CategoryPath { get; set; }
        public byte ProductType { get; set; }
        public long TypeId { get; set; }
        public long BrandId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string ShortDescription { get; set; }
        public int SaleStatus { get; set; }
        public ProductAuditStatus AuditStatus { get; set; }
        public DateTime AddedDate { get; set; }
        public long DisplaySequence { get; set; }
        public string ImagePath { get; set; }
        public decimal MarketPrice { get; set; }
        public decimal MinSalePrice { get; set; }
        public byte HasSku { get; set; }
        public long VistiCounts { get; set; }
        public long SaleCounts { get; set; }
        public long FreightTemplateId { get; set; }
        public decimal Weight { get; set; }
        public decimal Volume { get; set; }
        public int Quantity { get; set; }
        public string MeasureUnit { get; set; }
        public int EditStatus { get; set; }
        public short IsDeleted { get; set; }
        public int MaxBuyCount { get; set; }
        public short IsOpenLadder { get; set; }
        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }
        public int ShopDisplaySequence { get; set; }
        public long VirtualSaleCounts { get; set; }
        public string VideoPath { get; set; }
    }
}
