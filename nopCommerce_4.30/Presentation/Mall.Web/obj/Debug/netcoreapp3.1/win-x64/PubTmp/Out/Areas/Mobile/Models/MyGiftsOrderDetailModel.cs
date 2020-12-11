using Mall.DTO;
using Mall.Entities;
using System;
using System.Collections.Generic;

namespace Mall.Web.Areas.Mobile.Models
{
    public class MyGiftsOrderDetailModel
    {
        public GiftOrderInfo OrderData { get; set; }

        public List<GiftOrderItemInfo> OrderItems { get; set; }
        public ExpressData ExpressData { get; set; }
    }
}
