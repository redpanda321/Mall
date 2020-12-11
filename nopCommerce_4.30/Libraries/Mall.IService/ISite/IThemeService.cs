using Mall.Entities;

/**
 * 商城主题设置
 * 2016-05-16
 * **/
namespace Mall.IServices
{
    /// <summary>
    /// 商城主题设置
    /// </summary>
    public interface IThemeService : IService
    {
        /// <summary>
        /// 商城主题设置添加
        /// </summary>
        /// <param name="mTheme">主题实体类</param>
        void AddTheme(ThemeInfo mTheme);

        /// <summary>
        ///  商城主题设置修改
        /// </summary>
        /// <param name="mTheme">主题实体类</param>
        void UpdateTheme(ThemeInfo mTheme);

        /// <summary>
        /// 获取商城主题
        /// </summary>
        /// <returns></returns>
        ThemeInfo getTheme();
    }
}
