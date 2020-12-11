using Mall.CommonModel;

namespace Mall.DTO.QueryModel
{
    public partial class MarketBoughtQuery : QueryBase
    {
        /// <summary>
        /// 购买类型
        /// </summary>
        public MarketType? MarketType { get; set; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }

        /// <summary>
        /// 购买记录ID
        /// </summary>
        public long Id { set; get; }

    }
}
