namespace Mall.Web.Areas.Mobile.Models
{
    public class ProductGetDistributionInfoModel
    {
        /// <summary>
        /// 是否显示最高赚
        /// </summary>
        public bool IsShowBrokerage { get; set; }
        /// <summary>
        /// 最高赚
        /// </summary>
        public decimal Brokerage { get; set; }
        /// <summary>
        /// 分销销量
        /// </summary>
        public long SaleCount { get; set; }
        /// <summary>
        /// 分享地址
        /// </summary>
        public string ShareUrl { get; set; }
        /// <summary>
        /// 微信分享参数
        /// </summary>
        public CommonModel.Model.WeiXinShareArgs WeiXinShareArgs { get; set; }
    }
}