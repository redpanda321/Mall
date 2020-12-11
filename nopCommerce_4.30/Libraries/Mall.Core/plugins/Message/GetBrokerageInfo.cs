using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Core.Plugins.Message
{
   public class GetBrokerageInfo
    {
       public string UserName { set; get; }

       /// <summary>
       /// 佣金
       /// </summary>
       public decimal Money { set; get; }

       /// <summary>
       /// 商城名称
       /// </summary>
       public string SiteName { set; get; }

        /// <summary>
        /// 获得日期
        /// </summary>
        public string GetDate { set; get; }

        public MessageOrderType MsgOrderType { get; set; }

        public string OrderId { get; set; }
    }
}
