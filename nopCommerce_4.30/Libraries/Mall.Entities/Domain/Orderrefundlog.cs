using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class OrderRefundLogInfo
    {
        public long Id { get; set; }
        public long RefundId { get; set; }
        public string Operator { get; set; }
        public DateTime OperateDate { get; set; }
        public string OperateContent { get; set; }
        public int ApplyNumber { get; set; }
        public short Step { get; set; }
        public string Remark { get; set; }
    }
}
