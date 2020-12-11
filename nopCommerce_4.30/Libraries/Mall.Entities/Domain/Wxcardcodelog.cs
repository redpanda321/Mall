using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class WxCardCodeLogInfo
    {
        public long Id { get; set; }
        public long CardLogId { get; set; }
        public string CardId { get; set; }
        public string Code { get; set; }
        public DateTime SendTime { get; set; }
        public int CodeStatus { get; set; }
        public DateTime? UsedTime { get; set; }
        public int CouponType { get; set; }
        public long CouponCodeId { get; set; }
        public string OpenId { get; set; }
    }
}
