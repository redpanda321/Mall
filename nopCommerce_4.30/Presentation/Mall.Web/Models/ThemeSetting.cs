using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Web.Models
{
    /// <summary>
    /// 主题配色设置
    /// </summary>
    public class ThemeSetting
    {
        public long themeId { get; set; }
        public Mall.CommonModel.ThemeType typeId { get; set; }
        public string mainColor { get; set; }
        public string secondaryColor { get; set; }
        public string writingColor { get; set; }
        public string frameColor { get; set; }
        public string classifiedsColor { get; set; }
        /// <summary>
        /// 当前启用的模板ID
        /// </summary>
        public int templateId { get; set; }
    }
}