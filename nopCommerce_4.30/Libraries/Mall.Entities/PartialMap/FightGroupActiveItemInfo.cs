using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class FightGroupActiveItemInfo
    {
        /// <summary>
        /// 规格名称
        /// </summary>

        [ResultColumn]
        public string SkuName { get; set; }
        /// <summary>
        /// 商品售价
        /// </summary>
        [ResultColumn]
        public decimal ProductPrice { get; set; }
        /// <summary>
        /// 商品成本价
        /// </summary>
        [ResultColumn]
        public decimal ProductCostPrice { get; set; }
        /// <summary>
        /// 已售
        ///</summary>
        [ResultColumn]
        public long ProductStock { get; set; }
        /// <summary>
        /// 颜色
        /// </summary>
        [ResultColumn]
        public string Color { get; set; }
        /// <summary>
        /// 尺码
        /// </summary>
        [ResultColumn]
        public string Size { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        [ResultColumn]
        public string Version { get; set; }
        /// <summary>
        /// 显示图片
        /// <para>颜色独有</para>
        /// </summary>
        [ResultColumn]
        public string ShowPic { get; set; }
    }
}
