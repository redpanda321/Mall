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
    public class MemberCapitalApplyWithDrawModel
    {
        public MemberCapitalApplyWithDrawModel()
        {
            this.applyType = 1;
        }
        public string openid { get; set; }
        public string nickname { get; set; }
        public decimal amount { get; set; }
        public string pwd { get; set; }
        public int applyType { get; set; }
    }
}
