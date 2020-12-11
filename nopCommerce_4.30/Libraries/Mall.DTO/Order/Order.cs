using Mall.CommonModel;
using Mall.Core;
using Mall.Entities;
using System;

namespace Mall.DTO
{
    public class Order
    {
        public long Id { get; set; }

        public string OrderId { get { return Id.ToString(); } }

        public OrderInfo.OrderOperateStatus OrderStatus { get; set; }
        public System.DateTime OrderDate { get; set; }
        public string CloseReason { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public string SellerPhone { get; set; }
        public string SellerAddress { get; set; }
        public string SellerRemark { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string UserRemark { get; set; }
        public string ShipTo { get; set; }
        public string CellPhone { get; set; }
        public int TopRegionId { get; set; }
        public int RegionId { get; set; }
        public string RegionFullName { get; set; }
        public string Address { get; set; }
        public float ReceiveLatitude { get; set; }
        public float ReceiveLongitude { get; set; }
        public string ExpressCompanyName { get; set; }
        public decimal Freight { get; set; }
        public string ShipOrderNumber { get; set; }
        public DateTime? ShippingDate { get; set; }
        public bool IsPrinted { get; set; }
        public string PaymentTypeName { get; set; }
        public string PaymentTypeGateway { get; set; }
        /// <summary>
        /// #组合支付时显示”预存款+其他支付方式“
        /// #线上支付改成显示具体的支付方式（支付宝支付、微信支付、银联支付、预存款）
        /// </summary>
        public string PaymentTypeDesc {
            get {
                System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();
                if (this.PaymentType == OrderInfo.PaymentTypes.Online)
                {
                    if (this.CapitalAmount > 0)
                    {
                        result.Add("预存款支付");
                    }

                    if (!string.IsNullOrWhiteSpace(this.PaymentTypeGateway))
                    {
                        if (this.PaymentTypeGateway.ToLower().Contains("weixin"))
                        {
                            result.Add("微信支付");
                        }
                        else if (this.PaymentTypeGateway.ToLower().Contains("alipay"))
                        {
                            result.Add("支付宝支付");
                        }
                        else if (this.PaymentTypeGateway.ToLower().Contains("unionpay"))
                        {
                            result.Add("银联支付");
                        }
                    }
                    if (result.Count == 0)
                    {
                        result.Add("其他");
                    }
                }
                else if (this.PaymentType == OrderInfo.PaymentTypes.Offline || this.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery)
                {
                    result.Add(this.PaymentType.ToDescription());
                }
                return string.Join("+", result);
            }
        }
        public string GatewayOrderId { get; set; }
        public string PayRemark { get; set; }
        public DateTime? PayDate { get; set; }
        public string InvoiceTitle { get; set; }
        public string InvoiceCode { get; set; }
        public decimal Tax { get; set; }
        public DateTime? FinishDate { get; set; }
        public decimal ProductTotalAmount { get; set; }
        public decimal RefundTotalAmount { get; set; }
        public decimal CommisTotalAmount { get; set; }
        public decimal RefundCommisAmount { get; set; }
        public OrderInfo.ActiveTypes ActiveType { get; set; }
        public Mall.Core.PlatformType Platform { get; set; }
        public decimal DiscountAmount { get; set; }
        /// <summary>
        /// 满减优惠
        /// </summary>
        public decimal FullDiscount { set; get; }
        public InvoiceType InvoiceType { get; set; }
        public decimal IntegralDiscount { get; set; }
        /// <summary>
        /// 预付款
        /// </summary>
        public decimal CapitalAmount { get; set; }
        public string InvoiceContext { get; set; }
        public OrderInfo.OrderTypes? OrderType { get; set; }
        public Entities.OrderInfo.PaymentTypes PaymentType { get; set; }
        public string OrderRemarks { get; set; }
        public DateTime? LastModifyTime { get; set; }
        public Mall.CommonModel.DeliveryType DeliveryType { get; set; }
        public long ShopBranchId { get; set; }
        public string PickupCode { get; set; }
        public int? SellerRemarkFlag { get; set; }
        public int DadaStatus { get; set; }

        /// <summary>
        /// 订单商品总数
        /// </summary>
        public long OrderProductQuantity { get; set; }

        /// <summary>
        /// 订单退货总数
        /// </summary>
        public long OrderReturnQuantity { get; set; }

        /// <summary>
        /// 订单实付金额
        /// 公式： 商品应付+运费+税 - 优惠券金额 - 积分抵扣金额
        /// </summary>
        public decimal OrderTotalAmount { get; set; }

        /// <summary>
        /// 订单金额 （商品应付+运费+税 -优惠券金额） 不包含积分抵扣部分
        /// </summary>
        public decimal OrderAmount { get; set; }

        /// <summary>
        /// 商品实付（商品应付-优惠券的价格）
        /// </summary>
        public decimal ProductTotal { get; set; }

        /// <summary>
        /// 订单实付金额(转为数据库冗余字段)
        /// </summary>
        public decimal TotalAmount { get; set; }


        /// <summary>
        /// 订单实收金额（订单实付金额-退款）(转为数据库冗余字段)
        /// </summary>
        public decimal ActualPayAmount { get; set; }

        /// <summary>
        /// 订单可退金额
        /// </summary>
        public decimal OrderEnabledRefundAmount { get; set; }

        /// <summary>
        /// 需要第三方支付的金额（参照orderinfo中的计算方式）
        /// <para>this.ProductTotalAmount + this.Freight + this.Tax - this.IntegralDiscount - this.DiscountAmount - this.FullDiscount - this.CapitalAmount</para>
        /// </summary>
        public decimal OrderPayAmount { get; set; }
        /// <summary>
        /// 订单实际分佣
        /// </summary>
        public decimal CommisAmount { get; set; }

        /// <summary>
        /// 商家结算金额
        /// </summary>
        public decimal ShopAccountAmount { get; set; }

        /// <summary>
        /// 退款状态
        /// </summary>
        public int? RefundStats { get; set; }
        public string ShowRefundStats { get; set; }

        /// <summary>
        /// 是否包含被删除的商品
        /// </summary>
        public bool HaveDelProduct { get; set; }

        /// <summary>
        /// 是否已过售后期
        /// <para>需要手动补充数据</para>
        /// </summary>
        public bool? IsRefundTimeOut { get; set; }

        /// <summary>
        /// 拼团订单的状态
        /// </summary>
        public FightGroupOrderJoinStatus? FightGroupOrderJoinStatus { get; set; }

        /// <summary>
        /// 拼团是否可退款状态
        /// </summary>
        public bool? FightGroupCanRefund { get; set; }

        /// <summary>
        /// 物流公司名称显示
        /// </summary>
        public string ShowExpressCompanyName { get; set; }

        /// <summary>
        /// 订单
        /// </summary>
        public string CreateTimeStr
        {
            get { return OrderDate.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        public string OrderStatusStr { get { return this.OrderStatus.ToDescription(); } }


        public string PaymentTypeStr { get { return this.PaymentType.ToDescription(); } }

        public string PlatformStr { get { return this.Platform.ToDescription(); } }
        public string OrderStatusText { get; set; }
    }
}
