using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    public enum DistributorRecordType
    {
        /// <summary>
        /// 佣金
        /// </summary>
        [Description("获得佣金")]
        Brokerage=1,
        /// <summary>
        /// 提现
        /// </summary>
        [Description("提现成功")]
        Withdraw =2

    }
}
