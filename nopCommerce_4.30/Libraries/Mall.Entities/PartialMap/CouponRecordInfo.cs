using Mall.CommonModel;
using NPoco;
using System;
using System.ComponentModel;
using System.Configuration;

namespace Mall.Entities
{
    public partial class CouponRecordInfo : IBaseCoupon
    {
        public enum CounponStatuses
        {
            [Description("未使用")]
            Unuse,
            [Description("已使用")]
            Used
        }
        /// <summary>
        /// 卡券关联信息
        /// </summary>
        [ResultColumn]
        public WXCardCodeLogInfo WXCardCodeInfo { get; set; }
        [ResultColumn]
        public long BaseId
        {
            get { return this.Id; }
        }




        [ResultColumn]
        public CouponType BaseType
        {
            get { return CouponType.Coupon; }
        }

        [ResultColumn]
        public string BaseShopName
        {
            get { return this.ShopName; }
        }


        /// <summary>
        /// CouponId == Id 
        /// </summary>
        [ResultColumn]
        [Obsolete("关联属性移除遗留")]
        public CouponInfo CouponInfo { get; set; }
    }
}
