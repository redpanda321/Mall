using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    /// <summary>
    /// 授权异常
    /// 操作权限不足或者操作非法数据
    /// </summary>
    public class AuthorizationException:MessageException
    {
        public AuthorizationException(ExceptionMessages message)
            : base(message)
        {

        }
    }
}
