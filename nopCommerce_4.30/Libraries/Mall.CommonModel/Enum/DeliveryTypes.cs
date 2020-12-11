using System.ComponentModel;

namespace Mall.CommonModel
{
    /// <summary>
    /// 配送方式
    /// </summary>
    public enum DeliveryType
	{
		/// <summary>
		/// 快递配送
		/// </summary>
		[Description("快递配送")]
		Express=0,

		/// <summary>
		/// 到店自提
		/// </summary>
		[Description("到店自提")]
		SelfTake = 1,

        /// <summary>
        /// 店员配送
        /// </summary>
        [Description("店员配送")]
        ShopStore = 2,
        /// <summary>
        /// 同城合作物流
        /// </summary>
        [Description("同城合作物流")]

        CityExpress = 3,
    }
}
