using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    /// <summary>
    /// 周边门店基本信息
    /// </summary>
    public class HomePageShopBranch
    {
        /// <summary>
        /// 门店活动
        /// </summary>
        public ShopActiveList ShopAllActives { get; set; }
        /// <summary>
        /// 门店信息
        /// </summary>
        public ShopBranch ShopBranch { get; set; }
        /// <summary>
        /// 门店商品
        /// </summary>
        public List<Product.Product> Products { get; set; }
    }
}
