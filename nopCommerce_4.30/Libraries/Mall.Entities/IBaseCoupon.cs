using System;

namespace Mall.Entities
{
    // 1.因历史遗留问题无法采用继承方式，只能选择接口
    // 2.为了不影响数据库字段采用冗余方式抽象出优惠类
    /// <summary>
    /// 优惠模型
    /// </summary>
    public interface IBaseCoupon
    {
      

 

       

        CouponType BaseType { get; }

        string BaseShopName { get; }

       

       

    }

    public enum CouponType
    {
        /// <summary>
        /// 优惠卷
        /// </summary>
        Coupon = 0,

        /// <summary>
        /// 商家红包
        /// </summary>
        ShopBonus = 1
    }
}
