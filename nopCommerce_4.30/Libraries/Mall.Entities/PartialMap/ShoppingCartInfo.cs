using NPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class ShoppingCartInfo
    {
        /// <summary>
        /// 购物车 商品项
        /// </summary>
        [ResultColumn]
        public List<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();

        /// <summary>
        /// 会员Id
        /// </summary>
        [ResultColumn]
        public long MemberId { get; set; }
    }

    /// <summary>
    /// 购物车商品项
    /// </summary>
    public class ShoppingCartItem
    {
        /// <summary>
        /// 购物车商品项Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 门店Id
        /// </summary>
        public long ShopBranchId { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// 商品SKUID
        /// </summary>
        public string SkuId { get; set; }

        /// <summary>
        /// 商品数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 状态 0:正常；1：冻结；2：库存不足
        /// </summary>
        public int Status { get; set; }
    }
}
