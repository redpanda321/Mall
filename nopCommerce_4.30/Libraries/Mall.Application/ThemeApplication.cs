using Mall.IServices;
using AutoMapper;
using Mall.DTO;
using Mall.Core;
using Nop.Core.Infrastructure;

/**
 * 主题设置实现
 * 2016-05-16
 * **/
namespace Mall.Application
{
    public  class ThemeApplication 
    {

        private static IThemeService _iThemeService=  EngineContext.Current.Resolve<IThemeService>();

        /// <summary>
        /// 商城主题设置设置,如果有数据即修改
        /// </summary>
        /// <param name="mTheme">主题实体类</param>
        public static void SetTheme(Mall.DTO.Theme mVTheme)
        {
            new Mall.Entities.MemberInfo();

           // Mapper.CreateMap<Mall.DTO.Theme, Mall.Entities.ThemeInfo>();
            //var mTheme = Mapper.Map<Mall.DTO.Theme, Mall.Entities.ThemeInfo>(mVTheme);

            var mTheme = mVTheme.Map< Mall.Entities.ThemeInfo>();


            if (mVTheme.ThemeId <= 0)
            {

                _iThemeService.AddTheme(mTheme);
            }
            else
            {
                _iThemeService.UpdateTheme(mTheme);
            }

        }

        /// <summary>
        /// 获取商城主题
        /// </summary>
        /// <returns></returns>
        public static Mall.DTO.Theme getTheme()
        {
            Mall.Entities.ThemeInfo mTheme = _iThemeService.getTheme();
            Mall.DTO.Theme mVTheme = new Theme();
            if (mTheme != null)
            {

                //    Mapper.CreateMap<Mall.Entities.ThemeInfo, Mall.DTO.Theme>();
                //    mVTheme = Mapper.Map<Mall.Entities.ThemeInfo, Mall.DTO.Theme>(mTheme);

                mVTheme = mTheme.Map< Mall.DTO.Theme>();

            }
            else
            {
                mVTheme.TypeId = Mall.CommonModel.ThemeType.Defaults;
            }

            return mVTheme;
        }


    }
}
