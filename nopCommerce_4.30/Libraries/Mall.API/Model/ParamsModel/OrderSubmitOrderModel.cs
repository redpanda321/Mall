using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.API.Model.ParamsModel
{
    public class OrderSubmitOrderModel
    {
        public string skuIds { get; set; }
        public string counts { get; set; }
        public long recieveAddressId { get; set; }
        public string couponIds { get; set; }
        public int integral { get; set; }
        public bool isCashOnDelivery { get; set; }
        public int invoiceType { get; set; }
        public string invoiceTitle { get; set; }
        public string invoiceContext { get; set; }
        public string invoiceCode { get; set; }
        /// <summary>
        /// 订单备注
        /// </summary>
        public string orderRemarks { get; set; }
        /// <summary>
        /// 用户APP选择门店自提时用到
        /// </summary>
        public string  jsonOrderShops { get; set; }
        /// <summary>
        /// 预存款
        /// </summary>
        public decimal CapitalAmount { get; set; }
        /// <summary>
        /// 支付密码
        /// </summary>
        public string PayPwd { get; set; }
        /// <summary>
        /// 虚拟商品用户信息项
        /// </summary>
        public string VirtualProductItems { get; set; }
        public sbyte ProductType { get; set; }
    }
}
