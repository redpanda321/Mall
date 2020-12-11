using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    public enum ExceptionMessages
    {
        /// <summary>
        /// 越权操作数据异常
        /// </summary>
        [Description("非授权操作")]
        UnauthorizedOperation = 1001,

        /// <summary>
        /// 未找到此{0}
        /// </summary>
        [Description("未找到此{0}")]
        NoFound = 2001,


        /// <summary>
        /// 接入码已存在
        /// </summary>
        [Description("接入码已存在")]
        WDGJAlreadyExisted=10001,


        [Description("账号已被冻结")]
        MemberDisabled =4001
    }
}
