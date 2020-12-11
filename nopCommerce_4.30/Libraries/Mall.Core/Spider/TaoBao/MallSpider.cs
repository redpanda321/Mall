using Mall.Core.Spider.TaoBao;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Core
{
   /// <summary>
   /// 
   /// </summary>
    public static class MallSpider
    {
        private static IMallSpider Spider;
        static MallSpider()
        {
            Spider = null;
            Load();
        }
        private static void Load()
        {
            try
            {
                Spider =  EngineContext.Current.Resolve<IMallSpider>();
            }
            catch (Exception ex)
            {
                throw new CacheRegisterException("注册淘宝天猫数据抓取服务异常", ex);
            }
        }

        /// <summary>
        /// 获取商品详情信息
        /// </summary>
        /// <param name="grabUrl">抓取URL地址集合</param>
        public static List<TaoBaoProductDetailsInfo> GetProductDetailsByUrl(List<string> grabUrl)
        {
            return Spider.GetProductDetailsByUrl(grabUrl);
        }
    }
}
