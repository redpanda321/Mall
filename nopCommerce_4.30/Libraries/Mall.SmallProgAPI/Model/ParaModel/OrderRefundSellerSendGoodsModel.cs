using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.SmallProgAPI.Model.ParaModel
{
    /// <summary>
    /// 售后买家寄货
    /// </summary>
    public class OrderRefundSellerSendGoodsModel
    {
        public long Id { get; set; }
        public string ExpressCompanyName { get; set; }
        public string ShipOrderNumber { get; set; }
    }
}
