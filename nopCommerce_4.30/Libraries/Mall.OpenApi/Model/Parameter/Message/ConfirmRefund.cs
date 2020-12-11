using Mall.Core.Plugins.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.OpenApi.Model.Parameter.Message
{
    public class ConfirmRefund
    {
        public long RefundId { get; set; }
        public string Remark { get; set; }
    }
}
