using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    public class OrderPostModel
    {
        public long RecieveAddressId { get; set; }
        /// <summary>
        /// 收货地址坐标
        /// </summary>
        public string LatAndLng { get; set; }
        public string CouponIds { get; set; }
        //public int InvoiceType { get; set; }
        //public string InvoiceTitle { get; set; }
        //public string InvoiceCode { get; set; }
        //public string InvoiceContext { get; set; }
        public int Integral { get; set; }
        /// <summary>
        /// 预存款支付金额
        /// </summary>
        public decimal Capital { get; set; }
        public string CollpIds { get; set; }
        public bool IsCashOnDelivery { get; set; }
        public long ActiveId { get; set; }
        public long GroupId { get; set; }

        public long groupActionId { set; get; }

        public int PlatformType { get; set; }

        public OrderShop[] OrderShops { get; set; }

        public object CurrentUser { get; set; }


        public string CartItemIds { set; get; }
        // public long[] CartItemIds { get; set; }
        public bool IsShopbranchOrder { set; get; }
        /// <summary>
        /// 虚拟商品用户信息项
        /// </summary>
        public VirtualProductItem[] VirtualProductItems { get; set; }
        public sbyte ProductType { get; set; }
    }

    public class OrderShop
    {

        public long ShopId { get; set; }
        public OrderSku[] OrderSkus { get; set; }
        public CommonModel.DeliveryType DeliveryType { get; set; }
        public int ShopBrandId { get; set; }
        public string Remark { get; set; }        

        public PostOrderInvoiceInfo PostOrderInvoice { get; set; }
        //public InvoiceType InvoiceType { get; set; }
    }

    public class OrderSku
    {
        public string SkuId { get; set; }

        public int Count { get; set; }
    }

    public class VirtualProductItem
    {
        public string VirtualProductItemName { get; set; }
        public string Content { get; set; }
        public sbyte VirtualProductItemType { get; set; }
        public long OrderId { get; set; }
        public long OrderItemId { get; set; }
    }

    public class PostOrderInvoiceInfo
    {
        public InvoiceType InvoiceType { get; set; }

        public string InvoiceTitle { get; set; }

        public string InvoiceCode { get; set; }

        public string InvoiceContext { get; set; }

        public string RegisterAddress { get; set; }

        public string RegisterPhone { get; set; }

        public string BankName { get; set; }

        public string BankNo { get; set; }

        public string RealName { get; set; }

        public string CellPhone { get; set; }

        public string Email { get; set; }

        public int RegionID { get; set; }

        public string Address { get; set; }
    }
}
