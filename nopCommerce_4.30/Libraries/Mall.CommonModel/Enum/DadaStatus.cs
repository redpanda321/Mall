using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    public  enum DadaStatus
    {
        /// <summary>
        /// 待接单
        /// </summary>
        [Description("待接单")]
        WaitOrder = 1,

        /// <summary>
        /// 待取货
        /// </summary>
        [Description("待取货")]
        WaitTake = 2,

        /// <summary>
        /// 配送中
        /// </summary>
        [Description("配送中")]
        Distribution = 3,

        /// <summary>
        /// 已完成
        /// </summary>
        [Description("已完成")]
        Finished = 4,

        /// <summary>
        /// 已取消
        /// </summary>
        [Description("已取消")]
        Cancel = 5,

        /// <summary>
        /// 已过期
        /// </summary>
        [Description("已过期")]
        Expired = 7,

        /// <summary>
        /// 指派单
        /// </summary>
        [Description("指派单")]
        Assigned = 8,

        /// <summary>
        /// 系统故障订单发布失败
        /// </summary>
        [Description("系统故障订单发布失败")]
        Failure = 1000
    }
}
