using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    /// <summary>
    /// 店铺门店评分
    /// </summary>
    public class ShopStoreServiceMark
    {
        /// <summary>
        /// 包装
        /// </summary>
        public decimal PackMark { get; set; }
        /// <summary>
        /// 发货
        /// </summary>
        public decimal DeliveryMark { get; set; }
        /// <summary>
        /// 服务
        /// </summary>
        public decimal ServiceMark { get; set; }
        /// <summary>
        /// 综合
        /// </summary>
        public decimal ComprehensiveMark
        {
            get
            {
                return decimal.Round((PackMark + DeliveryMark + ServiceMark) / 3, 2);
            }
        }
    }
}
