using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopWithdrawInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string CashNo { get; set; }
        public DateTime ApplyTime { get; set; }
        public int Status { get; set; }
        public int CashType { get; set; }
        public decimal CashAmount { get; set; }
        public string Account { get; set; }
        public string AccountName { get; set; }
        public long SellerId { get; set; }
        public string SellerName { get; set; }
        public DateTime? DealTime { get; set; }
        public string ShopRemark { get; set; }
        public string PlatRemark { get; set; }
        public string ShopName { get; set; }
        public string SerialNo { get; set; }
    }
}
