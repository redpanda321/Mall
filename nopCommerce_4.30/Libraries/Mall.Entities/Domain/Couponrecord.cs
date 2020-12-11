using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class CouponRecordInfo
    {
        public long Id { get; set; }
        public long CouponId { get; set; }
        public string CounponSn { get; set; }
        public DateTime CounponTime { get; set; }
        public string UserName { get; set; }
        public long UserId { get; set; }
        public DateTime? UsedTime { get; set; }
        public long? OrderId { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public int CounponStatus { get; set; }
        public long? WxcodeId { get; set; }
    }
}
