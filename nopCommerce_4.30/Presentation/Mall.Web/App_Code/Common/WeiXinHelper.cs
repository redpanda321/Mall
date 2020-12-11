using Mall.Web.Framework;
using System;
using System.IO;

namespace Mall.Web.App_Code.Common
{
    public class WeiXinHelper
    {
        /// <summary>
        /// 二維碼分享
        /// </summary>
        /// <param name="sharePath">需分享的url</param>
        /// <param name="imgUrl"></param>
        /// <param name="imgCode"></param>
        public static void CreateQCode(string sharePath, out string fullPath, out string imgCode)
        {
            //var curhttp = System.Web.HttpContext.Current;
            string url = CurrentUrlHelper.CurrentUrlNoPort();
            url = url + sharePath;
            fullPath = url;
            var map = Core.Helper.QRCodeHelper.Create(url);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            //  将图片内存流转成base64,图片以DataURI形式显示  
            string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
            ms.Dispose();
            imgCode = strUrl;
        }
    }
}