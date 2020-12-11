using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class RefundReasonInfo
    {
        public int Id { get; set; }
        public string AfterSalesText { get; set; }
        public int Sequence { get; set; }
    }
}
