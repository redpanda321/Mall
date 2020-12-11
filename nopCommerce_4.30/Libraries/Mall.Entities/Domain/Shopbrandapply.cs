using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopBrandApplyInfo
    {
        public int Id { get; set; }
        public long ShopId { get; set; }
        public long BrandId { get; set; }
        public string BrandName { get; set; }
        public string Logo { get; set; }
        public string Description { get; set; }
        public string AuthCertificate { get; set; }
        public int ApplyMode { get; set; }
        public string Remark { get; set; }
        public int AuditStatus { get; set; }
        public DateTime ApplyTime { get; set; }
        public string PlatRemark { get; set; }
    }
}
