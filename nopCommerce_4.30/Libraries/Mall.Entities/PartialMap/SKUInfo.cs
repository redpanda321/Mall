using NPoco;

namespace Mall.Entities
{
    public partial class SKUInfo
    {
        /// <summary>
        /// 颜色别名
        /// </summary>
        [ResultColumn]
        public string ColorAlias { get; set; }
        /// <summary>
        /// 尺码别名
        /// </summary>
        [ResultColumn]
        public string SizeAlias { get; set; }
        /// <summary>
        /// 规格别名
        /// </summary>
        [ResultColumn]
        public string VersionAlias { set; get; }
    }
}
