using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.Core;

using Mall.Web.Framework;

namespace Mall.OpenApi.Model.Parameter
{
    /// <summary>
    /// 获取用户列表传入参数
    /// </summary>
    public class GetUsersUserParameterModel : BasePageParameterModel
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? start_time { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? end_time { get; set; }
        /// <summary>
        /// 检测参数完整性与合法性
        /// </summary>
        /// <returns></returns>
        public override bool CheckParameter()
        {
            bool result = base.CheckParameter();
            
            return result;
        }
        /// <summary>
        /// 值初始
        /// </summary>
        public override void ValueInit()
        {
            base.ValueInit();
        }
    }
}
