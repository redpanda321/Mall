using Mall.CommonModel;
using Mall.IServices;
using Mall.Entities;
using NetRube.Data;

namespace Mall.Service.Site
{
    /// <summary>
    /// 商城主题设置
    /// </summary>
    public class ThemeService : ServiceBase, IThemeService
    {
        /// <summary>
        /// 主题设置添加
        /// </summary>
        /// <param name="mTheme">主题设置实体</param>
        public void AddTheme(ThemeInfo mTheme)
        {
            //string imgUrl = string.Empty;
            //var data = Context.ThemeInfo.ToList();
            var data = DbFactory.Default.Get<ThemeInfo>().Exist();
            if (!data)
            {
                //Context.ThemeInfo.Add(mTheme);
                //Context.SaveChanges();
                DbFactory.Default.Add(mTheme);
                Core.Cache.Remove(CacheKeyCollection.Themes);//清除缓存
            }
        }

        /// <summary>
        /// 主题设置数据更改
        /// </summary>
        /// <param name="mTheme">主题设置实体</param>
        public void UpdateTheme(ThemeInfo mTheme)
        {
            //var model = Context.ThemeInfo.FirstOrDefault(s => s.ThemeId == mTheme.ThemeId);
            var model = DbFactory.Default.Get<ThemeInfo>().Where(s => s.ThemeId == mTheme.ThemeId).FirstOrDefault();
            if (model == null)
            {
                model = new ThemeInfo()
                {
                    ThemeId = 1,
                    ClassifiedsColor = "#00ffff",
                    FrameColor = "#ff00ff",
                    MainColor = "#ffff00",
                    SecondaryColor = "#ffff00",
                    TypeId = ThemeType.Customize,
                    WritingColor = "#cc0000"
                };
                //Context.ThemeInfo.Add(model);
                DbFactory.Default.Add(model);
            }
            model.TypeId = mTheme.TypeId;
            model.WritingColor = mTheme.WritingColor;
            model.SecondaryColor = mTheme.SecondaryColor;
            model.MainColor = mTheme.MainColor;
            model.FrameColor = mTheme.FrameColor;
            model.ClassifiedsColor = mTheme.ClassifiedsColor;
            //Context.SaveChanges();
            DbFactory.Default.Update(model);
            Core.Cache.Remove(CacheKeyCollection.Themes);//清除缓存
        }

        /// <summary>
        /// 获取主题设置实体集合
        /// </summary>
        /// <returns></returns>
        public ThemeInfo getTheme()
        {
            ThemeInfo MThemeInfo;
            if (Core.Cache.Exists(CacheKeyCollection.Themes))//如果存在缓存，则从缓存中读取
                MThemeInfo = Core.Cache.Get<ThemeInfo>(CacheKeyCollection.Themes);
            else
            {
                //MThemeInfo = Context.ThemeInfo.FirstOrDefault();
                MThemeInfo = DbFactory.Default.Get<ThemeInfo>().FirstOrDefault();
                Core.Cache.Insert(CacheKeyCollection.Themes, MThemeInfo);
            }
            return MThemeInfo;
        }
    }
}
