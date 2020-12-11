using Mall.DTO;

namespace Mall.Web.Areas.Mobile.Models
{
    public class FightGroupActiveDetailModel:Mall.DTO.FightGroupActiveModel
    {
        /// <summary>
        /// 分享图片
        /// </summary>
        public string ShareImage { get; set; }
        /// <summary>
        /// 分享标题
        /// </summary>
        public string ShareTitle { get; set; }
        /// <summary>
        /// 分享链接
        /// </summary>
        public string ShareUrl { get; set; }
        /// <summary>
        /// 分享描述
        /// </summary>
        public string ShareDesc { get; set; }

        public int BonusCount { get; set; }

        public decimal BonusGrantPrice { get; set; }
        public decimal BonusRandomAmountStart { get; set; }
        public decimal BonusRandomAmountEnd { get; set; }

        public FullDiscountActive FullDiscount { get; set; }
    }
}