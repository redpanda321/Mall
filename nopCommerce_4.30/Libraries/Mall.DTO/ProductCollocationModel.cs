using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class ProductCollocationModel
    {
        /// <summary>
        /// 活动Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 活动名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 主商品Id
        /// </summary>
        public long ProductId { get; set; }

        public long ShopId { get; set; }
        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal Cheap { get; set; }

        /// <summary>
        /// 活动商品
        /// </summary>
        public List<CollocationProducts> Products { get; set; }


    }
}
