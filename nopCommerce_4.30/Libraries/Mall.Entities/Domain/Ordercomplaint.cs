using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class OrderComplaintInfo
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public int Status { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public string ShopPhone { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public DateTime ComplaintDate { get; set; }
        public string ComplaintReason { get; set; }
        public string SellerReply { get; set; }
        public string PlatRemark { get; set; }
    }
}
