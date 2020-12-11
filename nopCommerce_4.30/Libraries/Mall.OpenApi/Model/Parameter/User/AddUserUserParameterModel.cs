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
    public class AddUserUserParameterModel : BasePageParameterModel
    {
        public string user_name { get; set; }
        public string password { get; set; }
        public DateTime? created { get; set; }
        public string real_name { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public string avatar { get; set; }
        public string sex { get; set; }
        public DateTime? birthday { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string district { get; set; }
        public string town { get; set; }
        public string address { get; set; }
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
