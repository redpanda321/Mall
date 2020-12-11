using Mall.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.API.Model
{
    public class ShopOrderGetOrderModel : Order
    {
        public bool CanDaDaExpress { get; set; }
    }
}
