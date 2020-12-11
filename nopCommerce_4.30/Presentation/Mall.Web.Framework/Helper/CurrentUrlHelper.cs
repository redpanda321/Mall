using Mall.Core.Helper;

namespace Mall.Web.Framework
{
    public class CurrentUrlHelper
    {
        /// <summary>
        /// 取站点域名：Scheme://Host
        /// </summary>
        /// <returns></returns>
        public static string CurrentUrlNoPort()
        {
            return WebHelper.GetScheme() + "://" + WebHelper.GetHost();
        }
        /// <summary>
        /// 取站点域名，带端口：Scheme://Host:Port
        /// </summary>
        /// <returns></returns>
        public static string CurrentUrl()
        {
            var scheme = WebHelper.GetScheme().ToLower();
            var port = WebHelper.GetPort();
            if (port == "80" && scheme == "http") { port = ""; }
            if (port == "443" && scheme == "https") { port = ""; }
            port = string.IsNullOrEmpty(port) ? string.Empty : ":" + port;
            return scheme + "://" + WebHelper.GetHost() + port;
        }

        /// <summary>
        /// 取站点Scheme
        /// </summary>
        /// <returns></returns>
        public static string GetScheme()
        {
            return WebHelper.GetScheme();
        }

    }
}