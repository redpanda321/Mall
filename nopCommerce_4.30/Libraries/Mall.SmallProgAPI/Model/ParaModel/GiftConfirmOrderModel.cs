using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.SmallProgAPI.Model.ParaModel
{
    public class GiftConfirmOrderModel
    {
        public long ID { set; get; }
        public long? AddressId { set; get; }
        public int Count { set; get; }
    }

    public class GiftConfirmOrderOver
    {
        public long OrderId { set; get; }
    }

    
}
