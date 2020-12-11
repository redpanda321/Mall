using Mall.Core;
using Mall.IServices;
using Mall.Entities;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    public class SettledApplication
    {
        //private static ISettledService _iSettledService =  EngineContext.Current.Resolve<ISettledService>();

        private static ISettledService _iSettledService =  EngineContext.Current.Resolve<ISettledService>();


        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="mSettledInfo"></param>
        public static void AddSettled(SettledInfo mSettledInfo)
        {
            _iSettledService.AddSettled(mSettledInfo);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="mSettledInfo"></param>
        public static void UpdateSettled(SettledInfo mSettledInfo)
        {
            _iSettledService.UpdateSettled(mSettledInfo);
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
      public static  SettledInfo GetSettled()
        {
            return _iSettledService.GetSettled();
        }
    }
}
