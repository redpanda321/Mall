using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Mall.CommonModel
{
    /// <summary>
    /// 门店状态
    /// </summary>
    public enum ShopBranchStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        [Description("正常")]
        Normal=0,
        /// <summary>
        /// 冻结
        /// </summary>
        [Description("冻结")]
        Freeze=1
    }
}
