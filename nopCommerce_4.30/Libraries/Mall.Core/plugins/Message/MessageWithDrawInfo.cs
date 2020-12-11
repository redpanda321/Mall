using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Core.Plugins.Message
{
    public class MessageWithDrawInfo
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { set; get; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 提现金额
        /// </summary>
        public decimal Amount { set; get; }

        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime ApplyTime { get; set; }

        /// <summary>
        /// 提现方式
        /// </summary>
        public int ApplyType { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 商城名称
        /// </summary>
        public string SiteName { set; get; }
    }
}
