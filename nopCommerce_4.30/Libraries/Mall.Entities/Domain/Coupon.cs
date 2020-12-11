using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class CouponInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public decimal Price { get; set; }
        public int PerMax { get; set; }
        public decimal OrderAmount { get; set; }
        public int Num { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string CouponName { get; set; }
        public DateTime CreateTime { get; set; }
        public int ReceiveType { get; set; }
        public int NeedIntegral { get; set; }
        public DateTime EndIntegralExchange { get; set; }
        public string IntegralCover { get; set; }
        public int IsSyncWeiXin { get; set; }
        public int WxauditStatus { get; set; }
        public long? CardLogId { get; set; }
        public int UseArea { get; set; }
        public string Remark { get; set; }
    }
}
