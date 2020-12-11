using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    /// <summary>
    /// 参数异常
    /// 业务调用参数错误相关异常
    /// </summary>
    public class ParameterException:MessageException
    {
        public ParameterException(ExceptionMessages message)
            : base(message)
        {

        }
    }
}
