using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    public enum SaleDimension : int
    {
        /// <summary>
        /// 订单量
        /// </summary>
        [Description("销售量")]
        Count = 1,
        /// <summary>
        /// 销售额
        /// </summary>
        [Description("销售额")]
        Amount=2,
        /// <summary>
        /// 访问量
        /// </summary>
        [Description("访问量")]
        Visti =3,
    }
}
