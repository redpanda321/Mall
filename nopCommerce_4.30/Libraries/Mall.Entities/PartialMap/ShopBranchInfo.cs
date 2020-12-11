using NPoco;

namespace Mall.Entities
{
    public partial class ShopBranchInfo
    {
        /// <summary>
        /// 门店距离
        /// </summary>
        [ResultColumn]
        public double Distance { get; set; }
        /// <summary>
        /// 门店是否可用
        /// </summary>
        [ResultColumn]
        public bool Enabled { get; set; }
    }
}
