using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.Entities;
using static Mall.Entities.OrderInfo;

namespace Mall.SmallProgAPI.Model
{
    public class OrderRefundGetRefundDetailModel : BaseResultModel
    {
        public OrderRefundGetRefundDetailModel(bool status) : base(status)
        {
        }
        public RefundDetail Data { get; set; }
    }


    public class RefundDetail
    {
        public string AdminRemark { get; set; }
        public string ApplyForTime { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string DealTime { get; set; }
        public string Operator { get; set; }
        public string Reason { get; set; }
        public long RefundId { get; set; }
        public string OrderId { get; set; }
        public long Quantity { get; set; }
        public string RefundMoney { get; set; }
        public string RefundType { get; set; }
        public string ProductName { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNo { get; set; }
        public string BankName { get; set; }
        public string OrderTotal { get; set; }
        public string ShopName { get; set; }
        public bool CanResetActive { get; set; }
        public bool IsOrderRefundTimeOut { get; set; }
        public string FinishTime { get; set; }
        public List<RefundDetailSKU> ProductInfo { get; set; }

        public string ContactPerson { get; set; }
        public string ContactCellPhone { get; set; }
        public List<OrderRefundLogInfo> RefundLogs { get; internal set; }
        public string SellerAuditStatus { get; internal set; }
        public int SellerAuditStatusValue { get; internal set; }

        public IEnumerable<string> VerificationCodeIds { get; set; }

        public OrderTypes OrderType { get; set; }
    }

    public class RefundDetailSKU
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public string SKUContent { get; set; }
        public string Price { get; set; }
        public int Quantity { get; set; }
        public string ThumbnailsUrl { get; set; }
    }

}
