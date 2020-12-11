using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class OrderRefundInfo
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long OrderItemId { get; set; }
        public string VerificationCodeIds { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public long UserId { get; set; }
        public string Applicant { get; set; }
        public string ContactPerson { get; set; }
        public string ContactCellPhone { get; set; }
        public string RefundAccount { get; set; }
        public DateTime ApplyDate { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public string ReasonDetail { get; set; }
        public int SellerAuditStatus { get; set; }
        public DateTime SellerAuditDate { get; set; }
        public string SellerRemark { get; set; }
        public int ManagerConfirmStatus { get; set; }
        public DateTime ManagerConfirmDate { get; set; }
        public string ManagerRemark { get; set; }
        public byte IsReturn { get; set; }
        public string ExpressCompanyName { get; set; }
        public string ShipOrderNumber { get; set; }
        public string Payee { get; set; }
        public string PayeeAccount { get; set; }
        public int RefundMode { get; set; }
        public int RefundPayStatus { get; set; }
        public int RefundPayType { get; set; }
        public DateTime? BuyerDeliverDate { get; set; }
        public DateTime? SellerConfirmArrivalDate { get; set; }
        public string RefundBatchNo { get; set; }
        public DateTime? RefundPostTime { get; set; }
        public long ReturnQuantity { get; set; }
        public decimal ReturnPlatCommission { get; set; }
        public int ApplyNumber { get; set; }
        public string CertPic1 { get; set; }
        public string CertPic2 { get; set; }
        public string CertPic3 { get; set; }
    }
}
