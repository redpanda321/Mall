using Mall.Core;
using Mall.DTO;
using Mall.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Mall.API.Model
{
    public class OrderRefundApiModel : OrderRefund
    {
        static OrderRefundApiModel()
        {
            //AutoMapper.Mapper.CreateMap<OrderRefund, OrderRefundApiModel>();
        }

        public List<DTO.OrderItem> OrderItem { get; set; }
        public decimal OrderPayAmount
        {
            get
            {
                decimal result = 0;
                if (OrderItem != null && OrderItem.Count > 0)
                {
                    result = OrderItem.Sum(d => d.RealTotalPrice - d.CouponDiscount-d.FullDiscount);
                }
                return result;
            }
        }
        public decimal OrderTotalAmount { get; set; }

        public string StatusDescription { get; set; }

        public int Status { get; set; }

        public string RefundModelDescription
        {
            get
            {
                return RefundMode.ToDescription();
            }
        }

        public string UserName { get; set; }

        public string UserCellPhone { get; set; }

        public string RefundPayTypeDescription
        {
            get
            {
                return RefundPayType.ToDescription();
            }
        }

        public string[] CertPics { get; set; }
    }

    /// <summary>
    /// 订单退款回复实体
    /// </summary>
    public class OrderRefundReplyModel
    {
        public long RefundId { get; set; }

        public OrderRefundInfo.OrderRefundAuditStatus AuditStatus { get; set; }

        public string SellerRemark { get; set; }
    }
}
