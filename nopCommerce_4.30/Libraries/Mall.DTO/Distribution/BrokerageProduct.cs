using Mall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.Core;
namespace Mall.DTO
{
    public class BrokerageProduct
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public DistributionProductStatus ProductStatus { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public decimal Settlement { get; set; }
        public decimal NoSettlement { get; set; }

        /// <summary>
        /// 所属商家Id
        /// </summary>
        public long ShopId { get; set; }

        /// <summary>
        /// 所属商家名称
        /// </summary>
        public string ShopName { get; set; }
    }
}
