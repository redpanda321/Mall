using System.Collections.Generic;

namespace Mall.DTO
{
    /// <summary>
    /// 拼团获取规格信息
    /// </summary>
    public class FightGroupGetSkuListModel
    {
        public FightGroupGetSkuListModel()
        {
            skulist = new List<FightGroupActiveItemModel>();
        }

        /// <summary>
        /// 规格信息集
        /// </summary>
        public List<FightGroupActiveItemModel> skulist { get; set; }
        /// <summary>
        /// 商品图片
        /// </summary>
        public string ProductImg { get; set; }
    }
}