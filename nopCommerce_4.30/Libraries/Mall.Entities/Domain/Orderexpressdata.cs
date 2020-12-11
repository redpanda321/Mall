using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class OrderExpressDataInfo
    {
        public long Id { get; set; }
        public string CompanyCode { get; set; }
        public string ExpressNumber { get; set; }
        public string DataContent { get; set; }
    }
}
