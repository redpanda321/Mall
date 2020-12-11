using Mall.Core;
using Mall.IServices;
using Mall.Entities;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    public class MobileHomeTopicApplication
    {
       // private static IMobileHomeTopicService _iMobileHomeTopicService =  EngineContext.Current.Resolve<IMobileHomeTopicService>();

        private static IMobileHomeTopicService _iMobileHomeTopicService =  EngineContext.Current.Resolve<IMobileHomeTopicService>();


        /// <summary>
        /// 获取移动端首页专题设置
        /// </summary>
        /// <param name="platformType">平台类型</param>
        /// <param name="shopId">店铺Id</param>
        /// <returns></returns>
        public static List<MobileHomeTopicInfo> GetMobileHomeTopicInfos(PlatformType platformType, long shopId = 0)
        {
            return _iMobileHomeTopicService.GetMobileHomeTopicInfos(platformType, shopId).ToList();
        }


    }
}
