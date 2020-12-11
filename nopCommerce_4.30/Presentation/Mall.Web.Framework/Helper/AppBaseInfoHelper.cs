using System;
using Mall.Application;

namespace Mall.Web.Framework
{
    public class AppBaseInfoHelper
    {
        private string _AppKey { get; set; }
        /// <summary>
        /// Key
        /// </summary>
        public string AppKey
        {
            get
            {
                return _AppKey;
            }
        }
        private string _AppSecret { get; set; }
        /// <summary>
        /// 公钥
        /// </summary>
        public string AppSecret
        {
            get
            {
                return _AppSecret;
            }
        }
        public AppBaseInfoHelper(string appkey)
        {
            _AppKey = appkey;
            if (string.IsNullOrWhiteSpace(_AppKey))
            {
                throw new MallApiException(ApiErrorCode.Missing_App_Key, "app_key");
            }
            _AppSecret = "";
            try
            {
                _AppSecret = AppBaseApplication.GetAppSecret(_AppKey);
            }
            catch (Exception ex)
            {
                throw new MallApiException(ApiErrorCode.Invalid_App_Key, "app_key");
            }
            if (string.IsNullOrWhiteSpace(_AppSecret))
            {
                throw new MallApiException(ApiErrorCode.Insufficient_ISV_Permissions, "not set app_secreat");
            }
        }
    }
}
