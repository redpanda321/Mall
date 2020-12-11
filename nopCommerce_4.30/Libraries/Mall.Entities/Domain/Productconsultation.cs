using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ProductConsultationInfo
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ConsultationContent { get; set; }
        public DateTime ConsultationDate { get; set; }
        public string ReplyContent { get; set; }
        public DateTime? ReplyDate { get; set; }
    }
}
