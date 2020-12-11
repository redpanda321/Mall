using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class DistributorInfo
    {
        public long MemberId { get; set; }
        public long SuperiorId { get; set; }
        public long GradeId { get; set; }
        public int OrderCount { get; set; }
        public string ShopName { get; set; }
        public string ShopLogo { get; set; }
        public short IsShowShopLogo { get; set; }
        public int DistributionStatus { get; set; }
        public DateTime ApplyTime { get; set; }
        public DateTime? PassTime { get; set; }
        public string Remark { get; set; }
        public int SubNumber { get; set; }
        public decimal Balance { get; set; }
        public decimal SettlementAmount { get; set; }
        public decimal FreezeAmount { get; set; }
        public decimal WithdrawalsAmount { get; set; }
        public int ProductCount { get; set; }
        public decimal SaleAmount { get; set; }
        public int SubProductCount { get; set; }
        public decimal SubSaleAmount { get; set; }
    }
}
