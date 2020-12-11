using System;
using Mall.Application;

namespace Mall.Web.Framework
{
    public class UserCookieEncryptHelper
    {
        private static string _userCookieKey;
        private static object _locker = new object();

        /// <summary>
        /// 用户标识Cookie加密
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns>返回加密后的Cookie值</returns>
        public static string Encrypt(long userId, string role, int iTimeOut = 0)
        {
            if (_userCookieKey == null)
                lock (_locker)
                    if (_userCookieKey == null)
                        _userCookieKey = GetUserCookieKey();

            string text = string.Empty;
            try
            {
                var iTokenTimeOut = 24;
                if (iTimeOut > 0)
                {
                    iTokenTimeOut = iTimeOut;
                }
                else
                {
                    var strTokenTimeOut = System.Configuration.ConfigurationManager.AppSettings["TokenTimeOut"];
                    int.TryParse(strTokenTimeOut, out iTokenTimeOut);
                }
                var strExpireTime = DateTime.Now.AddHours(iTokenTimeOut).ToString("s");
                string plainText = string.Format("{0},{1},{2},{3}", _userCookieKey, role, userId, strExpireTime);
                text = Core.Helper.SecureHelper.AESEncrypt(plainText, _userCookieKey);
                text = Core.Helper.SecureHelper.EncodeBase64(text);
                return text;
            }
            catch (Exception ex)
            {
                Core.Log.Error(string.Format("加密用户标识Cookie出错", text), ex);
                throw;
            }
        }

        /// <summary>
        /// 用户标识Cookie解密
        /// </summary>
        /// <param name="userIdCookie">用户IdCookie密文</param>
        /// <returns></returns>
		public static long Decrypt(string userIdCookie, string role, bool checkexpire = false)
        {
            if (_userCookieKey == null)
                lock (_locker)
                    if (_userCookieKey == null)
                        _userCookieKey = GetUserCookieKey();

            string plainText = string.Empty;
            long userId = 0;
            try
            {
                if (!string.IsNullOrWhiteSpace(userIdCookie))
                {
                    userIdCookie = System.Web.HttpUtility.UrlDecode(userIdCookie);
                    userIdCookie = Core.Helper.SecureHelper.DecodeBase64(userIdCookie);
                    plainText = Core.Helper.SecureHelper.AESDecrypt(userIdCookie, _userCookieKey);//解密
                    var temp = plainText.Split(',');
                    if (temp.Length == 4)
                    {
                        if (temp[0] == _userCookieKey && temp[1].Equals(role, StringComparison.OrdinalIgnoreCase) && long.TryParse(temp[2], out userId) && (!checkexpire || DateTime.Parse(temp[3]) > DateTime.Now))//暂时去掉时间判断DateTime.Parse(temp[3]) > DateTime.Now 
                            return userId;
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Log.Error(string.Format("解密用户标识Cookie出错，Cookie密文：{0}", userIdCookie), ex);
            }

            return userId;
        }

        private static string GetUserCookieKey()
        {
            var settings = SiteSettingApplication.SiteSettings;
            if (string.IsNullOrEmpty(settings.UserCookieKey))
            {
                settings.UserCookieKey = Core.Helper.SecureHelper.MD5(Guid.NewGuid().ToString());
                SiteSettingApplication.SaveChanges();
            }

            return settings.UserCookieKey;
        }
    }
}
