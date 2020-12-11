using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class AgreementInfo
    {
        /// <summary>
        /// 协议枚举
        /// </summary>
        public enum AgreementTypes
        {
            /// <summary>
            /// 买家
            /// </summary>
            [Description("买家")]
            Buyers = 0,

            /// <summary>
            /// 卖家
            /// </summary>
            [Description("卖家")]
            Seller = 1,
            /// <summary>
            /// APP关于我们
            /// </summary>
            [Description("APP关于我们")]
            APP = 2
        }
    }
}
