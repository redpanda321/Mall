using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.DTO;
using Mall.Entities;

namespace Mall.SmallProgAPI.Model
{
    public class OrderRefundGetReturnDetailModel : BaseResultModel
    {
        public OrderRefundGetReturnDetailModel(bool status) : base(status)
        {
        }
        public ReturnDetail Data { get; set; }
    }

    public class ReturnDetail
    {
        public bool isDiscard;
        public bool isUnAudit;
        public bool isReturnGoods;

        public string SkuId { get; set; }
        public string Cellphone { get; set; }
        public string AdminRemark { get; set; }
        public string ShipAddress { get; set; }
        public string ShipTo { get; set; }
        public string ApplyForTime { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string DealTime { get; set; }
        public string FinishTime { get; set; }
        public string UserSendGoodsTime { get; set; }
        public string ConfirmGoodsTime { get; set; }
        public string Operator { get; set; }
        public string Reason { get; set; }
        public long ReturnId { get; set; }
        public string ShipOrderNumber { get; set; }
        public string OrderId { get; set; }
        public long Quantity { get; set; }
        public string OrderTotal { get; set; }
        public bool IsOnlyRefund { get; set; }
        public string RefundMoney { get; set; }
        public string RefundType { get; set; }
        public List<string> UserCredentials { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNo { get; set; }
        public string BankName { get; set; }
        public string ShopName { get; set; }
        public bool CanResetActive { get; set; }
        public bool IsOrderRefundTimeOut { get; set; }
        public List<RefundDetailSKU> ProductInfo { get; set; }

        public string ContactPerson { get; set; }
        public string ContactCellPhone { get; set; }
        public string ManagerConfirmStatus { get; internal set; }
        public int ManagerConfirmStatusValue { get; internal set; }
        public string ManagerConfirmDate { get; internal set; }
        public string ManagerRemark { get; internal set; }
        public string SellerAuditStatus { get; internal set; }
        public int SellerAuditStatusValue { get; internal set; }
        public string SellerConfirmArrivalDate { get; internal set; }
        public string SellerRemark { get; internal set; }
        public string SellerAuditDate { get; internal set; }
        public string BuyerDeliverDate { get; internal set; }
        public string ExpressCompanyName { get; internal set; }
        public DateTime ApplyDate { get; internal set; }
        public List<OrderRefundlog> RefundLogs { get; internal set; }
    }
}
