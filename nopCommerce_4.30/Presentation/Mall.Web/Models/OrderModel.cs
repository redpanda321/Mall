using Mall.CommonModel;
using System;
using System.Collections.Generic;

namespace Mall.Web.Models
{
    public class OrderModel
    {
        public long OrderId { set; get; }

        public string OrderStatus { get; set; }

        public string OrderDate { get; set; }

        public long ShopId { set; get; }

        public string ShopName { set; get; }

        public long UserId { set; get; }

        public string UserName { set; get; }

        public decimal TotalPrice { get; set; }

        public string PaymentTypeName { get; set; }

        public int PlatForm { get; set; }

        public string PlatformText { get; set; }

        public string IconSrc { get; set; }

        public CommonModel.DeliveryType DeliveryType { get; set; }

        public int? RefundStats { get; set; }

        public string RefundStatusText { get; set; }

        public string PaymentTypeGateway { get; set; }

        public int OrderState { get; set; }

        public DateTime? PayDate { get; set; }

        public string PaymentTypeStr { get; set; }

        /// <summary>
        ///商家 备注
        /// </summary>

        public string SellerRemark { set; get; }

        public int? SellerRemarkFlag { set; get; }

        public Entities.OrderInfo.PaymentTypes PaymentType { get; set; }
        public Entities.OrderInfo.OrderTypes? OrderType { get; set; }
        /// <summary>
        /// 是否可以发货
        /// </summary>
        public bool CanSendGood { get; set; }
        /// <summary>
        /// 拼团状态
        /// <para>拼团订单独有</para>
        /// </summary>
        public FightGroupOrderJoinStatus FightGroupJoinStatus { get; set; }
        /// <summary>
        /// 拼团是否可以退款
        /// </summary>
        public bool FightGroupCanRefund { get; set; }

        /// <summary>
        /// 支付流水号
        /// </summary>
        public string GatewayOrderId { get; set; }

        /// <summary>
        /// 收款人
        /// </summary>
        public string Payee { get; set; }

        public string RegionFullName { get; set; }

        public string Address { get; set; }

        public float ReceiveLatitude { get; set; }
        public float ReceiveLongitude { get; set; }
        public string LatAndLng { get { return ReceiveLatitude + "," + ReceiveLongitude; } set { } }


        public string UserRemark { get; set; }

        public string CellPhone { get; set; }

        public string ShopBranchName { get; set; }

        public string ShipOrderNumber { get; set; }

        public List<Mall.DTO.OrderItem> OrderItems { get; set; }
        /// <summary>
        /// 门店ID
        /// </summary>
        public long ShopBranchId { get; set; }
        public int RegionId { get; set; }
        /// <summary>
        /// 是否虚拟订单
        /// </summary>
        public bool IsVirtual { get; set; }
    }
}