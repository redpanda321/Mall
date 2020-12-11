using Mall.Entities;

namespace Mall.Web.Areas.Mobile.Models
{
    public class OrderRefundListModel
    {
        public long Vshopid { get; set; }
        public OrderRefundInfo RefundInfo { get; set; }

        public string ShopName { get; set; }


    }
}