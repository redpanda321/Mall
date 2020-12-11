using System;
using System.Collections.Generic;

namespace Mall.Core.Plugins.Payment
{
    /// <summary>
    /// 企业付款通知
    /// </summary>
    public class EnterprisePayNotifyInfo
    {
        /// <summary>
        /// 回传信息参数
        /// </summary>
        public string out_trade_no { get; set; }
        /// <summary>
        /// 成功付款详情
        /// </summary>
        public string success_details { get; set; }
        /// <summary>
        /// 失败付款详情
        /// </summary>
        public string fail_details { get; set; }
        /// <summary>
        /// 批次号
        /// </summary>
        public string batch_no { get; set; }
        /// <summary>
        /// 通知时间
        /// </summary>
        public DateTime? notify_time { get; set; }
        /// <summary>
        /// 主业务处理完成后响应内容
        /// 即当主程序相关订单状态完成后，需要响应请求的内容
        /// </summary>
        public string ResponseContentWhenFinished { get; set; }

    }
}
