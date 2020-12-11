using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class BusinessCategoryApplyInfo
    {
        public long Id { get; set; }
        public DateTime ApplyDate { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public int AuditedStatus { get; set; }
        public DateTime? AuditedDate { get; set; }
    }
}
