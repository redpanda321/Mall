using Mall.DTO;
using System.Collections.Generic;

namespace Mall.API.Model
{
    public class GiftsDetailModel : GiftModel
    {
        public bool success { get; set; }
        /// <summary>
        /// 是否可以购买
        /// </summary>
        public bool CanBuy { get; set; }
        /// <summary>
        /// 不可购买原因
        /// </summary>
        public string CanNotBuyDes { get; set; }
        public List<string> Images { get; set; }
    }
}
