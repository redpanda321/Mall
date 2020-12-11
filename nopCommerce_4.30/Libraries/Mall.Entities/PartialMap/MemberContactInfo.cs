using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class MemberContactInfo
    {
        public enum UserTypes
        {
            /// <summary>
            /// 普通用户
            /// </summary>
            [Description("普通用户")]
            General,

            /// <summary>
            /// 店铺用户
            /// </summary>
            [Description("店铺用户")]
            ShopManager,
        }
    }
}
