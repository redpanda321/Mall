using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
   public class OrderVerificationCodeModel: OrderVerificationCodeInfo
    {
        public string PayDateText { get; set; }
        public string StatusText { get; set; }
        public string Name { get; set; }
        public string VerificationTimeText { get; set; }

    }
    public class SearchShopAndShopbranchModel
    {
        public string Name { get; set; }

        /// <summary>
        /// 1=商家，2=门店
        /// </summary>
        public sbyte Type { get; set; }
        public long SearchId { get; set; }
    }
}
