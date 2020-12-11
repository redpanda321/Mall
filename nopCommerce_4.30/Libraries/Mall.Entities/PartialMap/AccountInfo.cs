
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class AccountInfo
    {
        /// <summary>
        /// 结算状态
        /// </summary>
        public enum AccountStatus
        {
            /// <summary>
            /// 未结算
            /// </summary>
            [Description("未结算")]
            UnAccount = 0,

            /// <summary>
            /// 已结算
            /// </summary>
            [Description("已结算")]
            Accounted
        }

        /// <summary>
        /// 结算金额
        /// </summary>
        public decimal AccountAmount { get { return 0; /*OrderAmount - RefundAmount - CommissionAmount; */} }
    }
}
