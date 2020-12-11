using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class CartItem
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; }

        public DateTime AddTime { get; set; }
        public SKUInfo Sku { get; set; }
        public ProductInfo Product { get; set; }
        public ShopInfo Shop { get; set; }
        /// <summary>
        /// 是否参与限时购
        /// </summary>
        public bool IsLimit { get; set; }
        /// <summary>
        /// 限时购活动id
        /// </summary>
        public long LimitId { get; set; }

        public int ShowStatus { get; set; }
        /// <summary>
        /// 阶梯商品最小批次
        /// </summary>
        public int MinMach { get; set; }

    }
}
