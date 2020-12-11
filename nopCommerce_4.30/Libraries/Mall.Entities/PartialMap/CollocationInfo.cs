using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class CollocationInfo
    {
        /// <summary>
        /// 商品标识
        /// </summary>
        [ResultColumn]
        public long ProductId { set; get; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        [ResultColumn]
        public string ShopName { set; get; }

        /// <summary>
        /// 组合购状态
        /// </summary>
        [ResultColumn]
        public string Status
        {
            get
            {

                if (this.EndTime < DateTime.Now)
                {
                    return "已结束";
                }
                else if (this.StartTime > DateTime.Now)
                {
                    return "未开始";
                }
                else
                {
                    return "进行中";
                }
            }
        }
    }
}
