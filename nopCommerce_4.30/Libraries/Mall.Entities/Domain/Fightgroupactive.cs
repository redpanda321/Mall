using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class FightGroupActiveInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string IconUrl { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int LimitedNumber { get; set; }
        public decimal LimitedHour { get; set; }
        public int LimitQuantity { get; set; }
        public int GroupCount { get; set; }
        public int OkGroupCount { get; set; }
        public DateTime AddTime { get; set; }
        public int ManageAuditStatus { get; set; }
        public string ManageRemark { get; set; }
        public DateTime? ManageDate { get; set; }
        public long? ManagerId { get; set; }
        public int ActiveTimeStatus { get; set; }
    }
}
