using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FlashSaleRemindInfo
    {
        public long Id { get; set; }
        public string OpenId { get; set; }
        public DateTime RecordDate { get; set; }
        public long FlashSaleId { get; set; }
    }
}
