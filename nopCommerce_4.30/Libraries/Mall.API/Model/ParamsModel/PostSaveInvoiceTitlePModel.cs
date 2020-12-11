using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.API.Model.ParamsModel
{
    /// <summary>
    /// 删除发票抬头
    /// </summary>
    public class PostSaveInvoiceTitlePModel
    {
        public string name { get; set; }
        public string code { get; set; }
    }
}
