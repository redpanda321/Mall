using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hishop.Open.Api;
using Mall.Core;

using Mall.Web.Framework;

namespace Mall.OpenApi.Model.Parameter
{
    /// <summary>
    /// 获取用户列表传入参数
    /// </summary>
    public class GetUserUserParameterModel : BasePageParameterModel
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string user_name { get; set; }
        /// <summary>
        /// 检测参数完整性与合法性
        /// </summary>
        /// <returns></returns>
        public override bool CheckParameter()
        {
            bool result = base.CheckParameter();
            if (string.IsNullOrWhiteSpace(user_name))
            {
                throw new MallApiException(OpenApiErrorCode.Missing_Parameters, "user_name");
            }
            return result;
        }
    }
}
