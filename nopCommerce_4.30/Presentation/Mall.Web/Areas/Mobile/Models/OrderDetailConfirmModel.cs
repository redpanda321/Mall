using Mall.DTO;
using System.Collections.Generic;

namespace Mall.Web.Areas.Mobile.Models
{
    public class OrderDetailConfirmModel
    {
        public List<ShopCartItemModel> products { get; set; }

        public decimal totalAmount { get; set; }

        public decimal Freight { get; set; }

        public dynamic orderAmount { get; set; }

        public decimal integralPerMoney { get; set; }

        public decimal userIntegrals { get; set; }

        public Entities.MemberIntegralInfo memberIntegralInfo { get; set; }
    }
}