using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class WxCardLogInfo
    {
        public long Id { get; set; }
        public string CardId { get; set; }
        public string CardTitle { get; set; }
        public string CardSubTitle { get; set; }
        public string CardColor { get; set; }
        public int AuditStatus { get; set; }
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public int CouponType { get; set; }
        public long CouponId { get; set; }
    }
}
