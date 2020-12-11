using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    /// <summary>
    /// 库存操作类型
    /// </summary>
    public enum StockOptionType
    {
        /// <summary>
        /// 正常修改
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 增加
        /// </summary>
        Add = 1,
        /// <summary>
        /// 减少
        /// </summary>
        Reduce = 2
    }
}
