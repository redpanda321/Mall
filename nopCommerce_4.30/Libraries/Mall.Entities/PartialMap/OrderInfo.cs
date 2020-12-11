using NPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.Core;
using Mall.CommonModel;

namespace Mall.Entities
{
    public partial class OrderInfo
    {
        [ResultColumn]
        public string OrderStatusText { get; set; }

        /// <summary>
        /// 订单支付记录
        /// </summary>
        [ResultColumn]
        public OrderPayInfo OrderPay { get; set; }

        /// <summary>
        /// 订单发票记录
        /// </summary>
        [ResultColumn]
        public OrderInvoiceInfo OrderInvoice { get; set; }

        #region 属性
        /// <summary>
        /// 订单可退金额
        /// </summary>
        [ResultColumn]
        public decimal OrderEnabledRefundAmount
        {
            get
            {
                decimal result = 0;
                switch (this.OrderStatus)
                {
                    case OrderOperateStatus.WaitVerification:
                    case OrderOperateStatus.Finish:
                    case OrderOperateStatus.WaitReceiving:
                        result = this.ProductTotalAmount - this.FullDiscount - this.DiscountAmount - this.RefundTotalAmount;   //商品总价 - 优惠券 - 已退金额
                        break;
                    case OrderOperateStatus.WaitDelivery:
                        result = this.ProductTotalAmount + this.Freight + this.Tax - this.DiscountAmount - this.FullDiscount;            //待发货退还运费
                        break;
                    case OrderOperateStatus.WaitSelfPickUp:
                        result = this.ProductTotalAmount - this.DiscountAmount - this.FullDiscount - this.RefundTotalAmount;
                        break;
                }
                return result;
            }
        }
        /// <summary>
        /// 是否可以原路退回
        /// </summary>
        /// <returns></returns>
        public bool CanBackOut()
        {
            bool result = false;
            if (!string.IsNullOrWhiteSpace(this.PaymentTypeGateway))
            {
                if (CapitalAmount <= 0 && (this.PaymentTypeGateway.ToLower().Contains("weixin") || this.PaymentTypeGateway.ToLower().Contains("alipay")))
                {
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// 商品实付（商品应付-优惠券的价格-满额减的价格）
        /// </summary>
        [ResultColumn]
        public decimal ProductTotal { get { return this.ProductTotalAmount - this.DiscountAmount - this.FullDiscount; } }

        /// <summary>
        /// 计算税费金额（商品应付-优惠券的价格-满额减的价格-积分抵扣金额）
        /// </summary>
        [ResultColumn]
        public decimal OrderTotalAmountByTax { get { return this.ProductTotalAmount - this.DiscountAmount - this.FullDiscount - this.IntegralDiscount; } }


        /// <summary>
        /// 订单实付金额
        /// 公式： 商品应付+运费+税 - 优惠券金额 - 积分抵扣金额-满额减金额 (减掉积分抵扣部分)
        /// </summary>
        [ResultColumn]
        public decimal OrderTotalAmount { get { return this.ProductTotalAmount + this.Freight + this.Tax - this.IntegralDiscount - this.DiscountAmount - this.FullDiscount; } }

        /// <summary>
        /// 订单金额 （商品应付+运费+税 -优惠券金额-满额减金额）  (未减掉积分抵扣部分)
        /// </summary>
        [ResultColumn]
        public decimal OrderAmount { get { return this.ProductTotalAmount + this.Freight + this.Tax - this.DiscountAmount - this.FullDiscount; } }

        /// <summary>
        /// 订单待付款金额是否为0
        /// 公式： 商品应付+运费+税 - 优惠券金额 - 积分抵扣金额-满额减金额-预付款支付
        /// </summary>
        [ResultColumn]
        public bool OrderWaitPayAmountIsZero
        {
            get
            {
                return (this.ProductTotalAmount + this.Freight + this.Tax - this.IntegralDiscount - this.DiscountAmount - this.FullDiscount - this.CapitalAmount) == 0;
            }
        }
        /// <summary>
        /// 需要第三方支付的金额
        /// <para>this.ProductTotalAmount + this.Freight + this.Tax - this.IntegralDiscount - this.DiscountAmount - this.FullDiscount - this.CapitalAmount</para>
        /// </summary>
        [ResultColumn]
        public decimal OrderPayAmount
        {
            get
            {
                var amount = this.ProductTotalAmount + this.Freight + this.Tax - this.IntegralDiscount - this.DiscountAmount - this.FullDiscount - this.CapitalAmount;
                if (amount > 0 && this.OrderStatus != OrderOperateStatus.WaitPay && this.OrderStatus != OrderOperateStatus.Close)
                {
                    return amount;
                }
                return 0;
            }
        }

        [ResultColumn]
        public string PaymentTypeDesc
        {
            get
            {
                List<string> result = new List<string>();
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

        /// <summary>
        /// 提交订单时选择的收货地址ID
        /// </summary>
        [ResultColumn]
        public long ReceiveAddressId { get; set; }

        /// <summary>
        /// 拼团订单的状态
        /// </summary>
        [ResultColumn]
        public FightGroupOrderJoinStatus? FightGroupOrderJoinStatus { get; set; }

        /// <summary>
        /// 拼团是否可退款状态
        /// </summary>
        [ResultColumn]
        public bool? FightGroupCanRefund { get; set; }
        #endregion

        /// <summary>
        /// 订单类别
        /// </summary>
        public enum OrderTypes
        {
            /// <summary>
            /// 正常购
            /// </summary>
            [Description("正常购")]
            Normal = 0,
            /// <summary>
            /// 组合购
            /// </summary>
            [Description("组合购")]
            Collocation = 1,
            /// <summary>
            /// 限时购
            /// </summary>
            [Description("限时购")]
            LimitBuy = 2,
            /// <summary>
            /// 拼团
            /// </summary>
            [Description("拼团")]
            FightGroup = 3,

            /// <summary>
            /// 虚拟商品订单
            /// </summary>
            [Description("虚拟")]
            Virtual = 4
        }
        /// <summary>
        /// 
        /// </summary>
        public enum PaymentTypes
        {
            /// <summary>
            /// 未付款时的默认状态
            /// </summary>
            [Description("未付款时的默认状态")]
            None = 0,

            /// <summary>
            /// 线上支付
            /// </summary>
            [Description("线上支付")]
            Online = 1,

            /// <summary>
            /// 平台确认收款之类的
            /// </summary>
            [Description("线下收款")]
            Offline = 2,

            /// <summary>
            /// 货到付款
            /// </summary>
            [Description("货到付款")]
            CashOnDelivery = 3,
        }
        /// <summary>
        /// 订单状态
        /// </summary>
        public enum OrderOperateStatus
        {
            /// <summary>
            /// 待付款 1
            /// </summary>
            [Description("待付款")]
            WaitPay = 1,

            /// <summary>
            /// 待发货 2
            /// </summary>
            [Description("待发货")]
            WaitDelivery = 2,

            /// <summary>
            /// 待收货 3
            /// </summary>
            [Description("待收货")]
            WaitReceiving = 3,

            /// <summary>
            /// 已关闭 4
            /// </summary>
            [Description("已关闭")]
            Close = 4,

            /// <summary>
            /// 已完成 5
            /// </summary>
            [Description("已完成")]
            Finish = 5,

            /// <summary>
            /// 待自提 6
            /// </summary>
            [Description("待自提")]
            WaitSelfPickUp = 6,

            /// <summary>
            /// 未评价 7
            /// </summary>
            [Description("未评价")]
            UnComment = 7,

            /// <summary>
            /// 待核销(待消费)
            /// </summary>
            [Description("待消费")]
            WaitVerification = 8,

            /// <summary>
            /// 历史订单 99
            /// </summary>
            [Description("历史订单")]
            History = 99
        }

        /// <summary>
        /// 配送方式
        /// </summary>
        public enum DeliveryTypes
        {
            /// <summary>
            /// 快递配送
            /// </summary>
            [Description("快递配送")]
            Express = 0,

            /// <summary>
            /// 到店自提
            /// </summary>
            [Description("到店自提")]
            SelfTake = 1,

            /// <summary>
            /// 店员配送
            /// </summary>
            [Description("店员配送")]
            ShopStore = 2
        }

        /// <summary>
        /// 订单活动
        /// </summary>
        public enum ActiveTypes
        {
            /// <summary>
            /// 无活动
            /// </summary>
            [Description("无活动")]
            None

        }
        /// <summary>
        /// 核销码状态
        /// </summary>
        public enum VerificationCodeStatus
        {
            /// <summary>
            /// 待核销
            /// </summary>
            [Description("待核销")]
            WaitVerification = 1,

            /// <summary>
            /// 已核销
            /// </summary>
            [Description("已核销")]
            AlreadyVerification = 2,

            /// <summary>
            /// 退款中
            /// </summary>
            [Description("退款中")]
            Refund = 3,

            /// <summary>
            /// 退款完成
            /// </summary>
            [Description("退款完成")]
            RefundComplete = 4,

            /// <summary>
            /// 已过期
            /// </summary>
            [Description("已过期")]
            Expired = 5
        }


        /// <summary>
        /// Id == OrderId 
        /// </summary>
        [ResultColumn]
        [Obsolete("关联属性移除遗留")]
        public List<OrderItemInfo> OrderItemInfo { get; set; }
    }
}
