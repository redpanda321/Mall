using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopRenewRecordInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string Operator { get; set; }
        public DateTime OperateDate { get; set; }
        public string OperateContent { get; set; }
        public int OperateType { get; set; }
        public decimal Amount { get; set; }
    }
}
