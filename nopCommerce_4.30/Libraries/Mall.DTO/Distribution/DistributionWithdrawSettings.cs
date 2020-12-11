using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class DistributionWithdrawSettings
    {
        /// <summary>
        /// 最小提现额限
        /// </summary>
        public decimal MinLimit { get; set; }
        /// <summary>
        /// 最高提现金额
        /// </summary>
        public decimal MaxLimit { get; set; }
        public string Types { get; set; }
        public bool EnableCapital { get { return Types?.ToLower().Contains("capital") ?? false; } }
        public bool EnableWeChat { get { return Types?.ToLower().Contains("wechat") ?? false; } }
        public bool EnableAlipay { get { return Types?.ToLower().Contains("alipay") ?? false; } }
    }
}
