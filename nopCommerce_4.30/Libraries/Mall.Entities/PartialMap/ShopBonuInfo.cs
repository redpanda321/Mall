using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class ShopBonusInfo
    {
        public enum UseStateType
        {
            [Description("没有限制")]
            None = 1,

            [Description("满X元使用")]
            FilledSend = 2
        }
    }
}
