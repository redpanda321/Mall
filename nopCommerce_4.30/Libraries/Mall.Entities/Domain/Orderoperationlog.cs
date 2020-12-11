using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class OrderOperationLogInfo
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public string Operator { get; set; }
        public DateTime OperateDate { get; set; }
        public string OperateContent { get; set; }
    }
}
