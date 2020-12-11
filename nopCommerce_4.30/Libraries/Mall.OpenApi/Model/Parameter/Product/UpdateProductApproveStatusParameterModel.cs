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
    /// 修改商品销售状态传入参数
    /// </summary>
    public class UpdateProductApproveStatusParameterModel : BaseParameterModel
    {
        /// <summary>
        /// 商品编号
        /// </summary>
        public int num_iid { get; set; }
        /// <summary>
        /// 商品状态
        /// </summary>
        public string approve_status { get; set; }

        /// <summary>
        /// 检测参数完整性与合法性
        /// </summary>
        /// <returns></returns>
        public override bool CheckParameter()
        {
            bool result = base.CheckParameter();
            if (num_iid < 1)
            {
                throw new MallApiException(OpenApiErrorCode.Product_Not_Exists, "num_iid");
            }
            if(string.IsNullOrWhiteSpace(approve_status))
            {
                throw new MallApiException(OpenApiErrorCode.Product_ApproveStatus_Faild, "approve_status");
            }
            return result;
        }
    }
}
