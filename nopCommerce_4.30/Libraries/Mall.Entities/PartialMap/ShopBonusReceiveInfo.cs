using Mall.CommonModel;
using NPoco;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;

namespace Mall.Entities
{
    public partial class ShopBonusReceiveInfo : IBaseCoupon
    {

        public enum ReceiveState
        {
            [Description("未使用")]
            NotUse = 1,

            [Description("已使用")]
            Use = 2,

            [Description("已过期")]
            Expired = 3
        }


        [ResultColumn]
        public long BaseId
        {
            get { return this.Id; }
        }

       
       
        public string BaseShopName { get; set; }
        [ResultColumn]
        public CouponType BaseType
        {
            get { return CouponType.ShopBonus; }
        }
    }
}
