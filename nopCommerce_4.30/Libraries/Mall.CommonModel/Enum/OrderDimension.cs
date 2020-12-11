using System.ComponentModel;

namespace Mall.CommonModel
{
    public enum OrderDimension : int
    {
        /// <summary>
        /// 下单量
        /// </summary>
        [Description("下单量")]
        OrderCount=2,

        /// <summary>
        /// 下单金额
        /// </summary>
        [Description("下单金额")]
        OrderMoney=3
    }
}
