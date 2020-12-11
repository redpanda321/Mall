using Mall.DTO;
using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.SmallProgAPI.Model
{
    public class GiftsOrderDtoModel
    {
        public bool success { get; set; }
        public long Id { get; set; }
        public GiftOrderInfo.GiftOrderStatus OrderStatus { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string UserRemark { get; set; }
        public string ShipTo { get; set; }
        public string CellPhone { get; set; }
        public Nullable<int> TopRegionId { get; set; }
        public Nullable<int> RegionId { get; set; }
        public string RegionFullName { get; set; }
        public string Address { get; set; }
        public string ExpressCompanyName { get; set; }
        public string ShipOrderNumber { get; set; }
        public Nullable<System.DateTime> ShippingDate { get; set; }
        public System.DateTime OrderDate { get; set; }
        public Nullable<System.DateTime> FinishDate { get; set; }

        public string OrderDateStr { get; set; }
        public Nullable<int> TotalIntegral { get; set; }
        public string CloseReason { get; set; }
        /// <summary>
        /// 显示订单状态
        /// </summary>
        public string ShowOrderStatus { get; set; }
        /// <summary>
        /// 物流公司名称显示
        /// </summary>
        public string ShowExpressCompanyName { get; set; }

        public List<GiftOrderItemDtoModel> Items { get; set; }
    }
    public class GiftOrderItemDtoModel
    {
        public long Id { get; set; }
        public Nullable<long> OrderId { get; set; }
        public long GiftId { get; set; }
        public int Quantity { get; set; }
        public Nullable<int> SaleIntegral { get; set; }
        public string GiftName { get; set; }
        public Nullable<decimal> GiftValue { get; set; }
        public string ImagePath { get; set; }
        public string DefaultImage { get; set; }
    }
    public class GiftsDetailModel : GiftModel
    {
        public bool success { get; set; }
        /// <summary>
        /// 是否可以购买
        /// </summary>
        public bool CanBuy { get; set; }
        /// <summary>
        /// 不可购买原因
        /// </summary>
        public string CanNotBuyDes { get; set; }
        public List<string> Images { get; set; }

        public string EndDateStr { get; set; }
    }
}
