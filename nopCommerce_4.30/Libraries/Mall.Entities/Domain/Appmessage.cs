using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class AppMessageInfo
    {
        public int Id { get; set; }
        public long ShopId { get; set; }
        public long ShopBranchId { get; set; }
        public int TypeId { get; set; }
        public long SourceId { get; set; }
        public string Content { get; set; }
        public byte IsRead { get; set; }
        public DateTime Sendtime { get; set; }
        public string Title { get; set; }
        public DateTime OrderPayDate { get; set; }
    }
}
