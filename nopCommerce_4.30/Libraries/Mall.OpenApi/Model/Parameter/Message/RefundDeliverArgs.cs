using Mall.Core.Plugins.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.OpenApi.Model.Parameter.Message
{
    public class RefundDeliverArgs
    {
        public long UserId { get; set; }
        public MessageOrderInfo Info { get; set; }
        public long RefundId { get; set; }
    }
}
