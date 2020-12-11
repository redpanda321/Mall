using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class BusinessCategoryApplyDetailInfo
    {
        public long Id { get; set; }
        public decimal CommisRate { get; set; }
        public long CategoryId { get; set; }
        public long ApplyId { get; set; }
    }
}
