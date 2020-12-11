using Mall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using Mall.DTO;
using Mall.Application;
using System.Runtime.Serialization.Json;
using System.Text;
using Mall.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Mall.Core.Helper;

namespace Mall.Web.Areas.Admin.Controllers
{
    [PCAuthorizationAttribute]
    public class ThemeController : BaseAdminController
    {
        const string _themesettings = "~/Areas/Admin/Views/PageSettings/themesetting.json";
        /// <summary>
        /// 当前启用的模板ID
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public ActionResult Index(int tid = 0)
        {


            ViewBag.tid = tid;
            Theme themeInfo = new Theme();
            if (System.IO.File.Exists(IOHelper.GetMapPath(_themesettings)))
            {
                string currentTheme = System.IO.File.ReadAllText(IOHelper.GetMapPath(_themesettings));//读取当前模板应用的主题配色
                List<ThemeSetting> curThemeObjs = ParseFormJson<List<ThemeSetting>>(currentTheme);
                if (curThemeObjs != null && curThemeObjs.Count > 0)
                {
                    var info = curThemeObjs.FirstOrDefault(a => a.templateId == tid);
                    if (info != null)
                    {
                        themeInfo.ThemeId = info.themeId;
                        themeInfo.ClassifiedsColor = info.classifiedsColor;
                        themeInfo.FrameColor = info.frameColor;
                        themeInfo.MainColor = info.mainColor;
                        themeInfo.SecondaryColor = info.secondaryColor;
                        themeInfo.TypeId = info.typeId;
                        themeInfo.WritingColor = info.writingColor;
                    }
                }
            }
            return View(themeInfo);
        }


        /// <summary>
        /// 修改主题设置
        /// </summary>
        /// <param name="templateId">当前启用的模板ID</param>
        /// <param name="id">主键ID</param>
        /// <param name="typeId">0、默认；1、自定义主题</param>
        /// <param name="MainColor">主色</param>
        /// <param name="SecondaryColor">商城辅色</param>
        /// <param name="WritingColor">字体颜色</param>
        /// <param name="FrameColor">边框颜色</param>
        /// <param name="ClassifiedsColor">边框栏颜色</param>
        /// <returns></returns>
        public JsonResult updateTheme(int templateId, long id, int typeId, string MainColor = "", string SecondaryColor = "", string WritingColor = "", string FrameColor = "", string ClassifiedsColor = "")
        {
            Theme mVTheme = new Theme()
            {
                ThemeId = id,
                TypeId = (Mall.CommonModel.ThemeType)typeId,
                MainColor = MainColor,
                SecondaryColor = SecondaryColor,
                WritingColor = WritingColor,
                FrameColor = FrameColor,
                ClassifiedsColor = ClassifiedsColor
            };

            ThemeApplication.SetTheme(mVTheme);

            #region 保存当前主题配色设置[前端页面使用]且保存到主题配色的Json文件
            ThemeSetting theme = new ThemeSetting()
            {
                classifiedsColor = mVTheme.ClassifiedsColor,
                frameColor = mVTheme.FrameColor,
                mainColor = mVTheme.MainColor,
                secondaryColor = mVTheme.SecondaryColor,
                themeId = mVTheme.ThemeId,
                typeId = mVTheme.TypeId,
                writingColor = mVTheme.WritingColor,
                templateId = templateId
            };
            List<ThemeSetting> curThemeObjs = new List<ThemeSetting>();
            string path = IOHelper.GetMapPath(_themesettings);
            if (System.IO.File.Exists(path))
            {
                string currentTheme = System.IO.File.ReadAllText(path);//读取当前模板应用的主题配色
                curThemeObjs = ParseFormJson<List<ThemeSetting>>(currentTheme);
                if (curThemeObjs != null && curThemeObjs.Count > 0)
                {
                    var info = curThemeObjs.FirstOrDefault(a => a.templateId == templateId);
                    if (info != null)
                        curThemeObjs.Remove(info);
                }
            }
            curThemeObjs.Add(theme);

            string fullName = IOHelper.GetMapPath(_themesettings);
            string themeJson = ObjectToJson<List<ThemeSetting>>(curThemeObjs, Encoding.UTF8);
            using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write))
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(themeJson);
                fs.Write(buffer, 0, buffer.Length);
            }
            #endregion

            return Json(new
            {
                status = 1
            });
        }

        public static string ObjectToJson<T>(Object jsonObject, Encoding encoding)
        {
            string result = String.Empty;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                serializer.WriteObject(ms, jsonObject);
                result = encoding.GetString(ms.ToArray());
            }
            return result;
        }

        public static T ParseFormJson<T>(string szJson)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(szJson)))
            {
                DataContractJsonSerializer dcj = new DataContractJsonSerializer(typeof(T));
                return (T)dcj.ReadObject(ms);
            }
        }
    }
}