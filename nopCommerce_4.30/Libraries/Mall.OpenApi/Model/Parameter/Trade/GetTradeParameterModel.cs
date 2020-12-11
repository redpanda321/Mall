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
    /// 商品/SKU库存修改传入参数
    /// </summary>
    public class GetTradeParameterModel : BaseParameterModel
    {
        /// <summary>
        /// 交易编号
        /// </summary>
        public string tid { get; set; }

        /// <summary>
        /// 检测参数完整性与合法性
        /// </summary>
        /// <returns></returns>
        public override bool CheckParameter()
        {
            bool result= base.CheckParameter();

            long orderId = 0;
            if (string.IsNullOrWhiteSpace(tid))
            {
                throw new MallApiException(OpenApiErrorCode.Biz_Order_ID_is_Invalid, "tid");
            }
            if (!long.TryParse(this.tid, out orderId))
            {
                throw new Mall.Core.MallException(((int)OpenApiErrorCode.Biz_Order_ID_is_Invalid).ToString());
            }
            if (orderId < 1)
            {
                throw new MallApiException(OpenApiErrorCode.Biz_Order_ID_is_Invalid, "tid");
            }

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
