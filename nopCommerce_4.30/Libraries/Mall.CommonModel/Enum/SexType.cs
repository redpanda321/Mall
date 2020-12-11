using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    public enum SexType
    {
        /// <summary>
        /// 
        /// </summary>
        [Description("保密")]
        None=0,
        /// <summary>
        /// 男
        /// </summary>
        [Description("男")]
        Male = 1,
        [Description("女")]
        Female = 2
    }
}
