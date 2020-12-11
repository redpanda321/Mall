using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.API.Model
{
    public class GiftsOrderAggregateDataModel
    {
        public bool success { get; set; }
        /// <summary>
        /// 所有订单数
        /// </summary>
        public int AllCount { get; set; }
        /// <summary>
        /// 待发货订单数
        /// </summary>
        public int WaitDeliveryCount { get; set; }
        /// <summary>
        /// 待收货订单数
        /// </summary>
        public int WaitReceivingCount { get; set; }
        /// <summary>
        /// 已完成订单数
        /// </summary>
        public int FinishCount { get; set; }
    }
}
