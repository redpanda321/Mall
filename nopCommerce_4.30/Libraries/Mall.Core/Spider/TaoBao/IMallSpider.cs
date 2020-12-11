using Mall.Core.Spider;
using Mall.Core.Spider.TaoBao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Core
{
    /// <summary>
    ///  淘宝/天猫商品详情数据抓取
    /// </summary>
    public interface IMallSpider : ISpider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="grabUrl"></param>
        /// <returns></returns>
        List<TaoBaoProductDetailsInfo> GetProductDetailsByUrl(List<string> grabUrl);
    }
}
