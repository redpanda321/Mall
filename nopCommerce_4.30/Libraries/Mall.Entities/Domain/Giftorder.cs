using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class GiftOrderInfo
    {
        public long Id { get; set; }
        public int OrderStatus { get; set; }
        public long UserId { get; set; }
        public string UserRemark { get; set; }
        public string ShipTo { get; set; }
        public string CellPhone { get; set; }
        public int TopRegionId { get; set; }
        public int RegionId { get; set; }
        public string RegionFullName { get; set; }
        public string Address { get; set; }
        public string ExpressCompanyName { get; set; }
        public string ShipOrderNumber { get; set; }
        public DateTime? ShippingDate { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public int TotalIntegral { get; set; }
        public string CloseReason { get; set; }
    }
}
