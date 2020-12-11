using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    public enum ExpressStatus:int
    {
        /// <summary>
        /// 
        /// </summary>
        [Description("开启")]
        Open =1,
        /// <summary>
        /// 
        /// </summary>
        [Description("关闭")]
        Close = 0
        
    }
}
