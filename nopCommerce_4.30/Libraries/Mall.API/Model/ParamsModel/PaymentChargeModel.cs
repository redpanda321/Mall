using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.API.Model.ParamsModel
{
    /// <summary>
    /// 添加商品到购物车-参数模型
    /// </summary>
    public class PaymentChargeModel
    {
        public string typeid { get; set; }
        public decimal amount { get; set; }
        public bool ispresent { get; set; }
    }
}
