using Mall.Core;
using System.Collections.Generic;

namespace Mall.DTO
{
    public class GiftOrderModel
    {
        public GiftOrderModel()
        {
            PlatformType = PlatformType.PC;
        }
        public PlatformType PlatformType { set; get; }
        public Entities.MemberInfo CurrentUser { set; get; }

        public Entities.ShippingAddressInfo ReceiveAddress { get; set; }
        /// <summary>
        /// 用户备注
        /// </summary>
        public string UserRemark { get; set; }
        public IEnumerable<GiftOrderItemModel> Gifts { set; get; }
    }

    public class GiftOrderItemModel
    {
        public long GiftId { get; set; }
        public int Counts { get; set; }
    }
}
