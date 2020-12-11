using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Mall.Core;
namespace Mall.Core.Helper
{
    public static class HttpHelper
    {
        private static T Result<T>(string returnText)
        {
            T result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(returnText);
            return result;
        }
        #region Get
        public static T Get<T>(string url, Dictionary<string, string> formData = null, CookieCollection cookies = null)
        {
            var content = Get(url, formData, cookies);
            return Result<T>(content);
        }
        public static string Get(string url, Dictionary<string, string> formData = null, CookieCollection cookies = null)
        {
            return HttpGet(url, formData, cookies);
        }
        #endregion
        #region Post
        public static string Post(string url, Dictionary<string, string> formData = null, Encoding encoding = null, bool IsJsonContentType = false, CookieContainer cookieContainer = null)
        {
            return HttpPost(url, formData, encoding, IsJsonContentType, cookieContainer);
        }
        public static string Post(string url, string jsonData = null, Encoding encoding = null, bool IsJsonContentType = false, CookieContainer cookieContainer = null)
        {
            return HttpPost(url, jsonData, encoding, IsJsonContentType, cookieContainer);
        }
        public static T Post<T>(string url, string jsonData = null, Encoding encoding = null, bool IsJsonContentType = false, CookieContainer cookieContainer = null)
        {
            string content = Post(url, jsonData, encoding, IsJsonContentType, cookieContainer);
            return Result<T>(content);
        }
        public static T Post<T>(string url, Dictionary<string, string> formData = null, Encoding encoding = null, bool IsJsonContentType = false, CookieContainer cookieContainer = null)
        {
            string content = Post(url, formData, encoding, IsJsonContentType, cookieContainer);
            return Result<T>(content);
        }
        #endregion 

        public static string HttpPost(string url, Dictionary<string, string> formData = null, Encoding encoding = null, bool IsJsonContentType = false, CookieContainer cookieContainer = null)
        {
            MemoryStream ms = new MemoryStream();
            FillFormDataStream(formData, ms);//填充formData
            return HttpPost(url, ms, encoding, IsJsonContentType, cookieContainer);
        }

        public static string HttpPost(string url, string jsonData = null, Encoding encoding = null, bool IsJsonContentType = false, CookieContainer cookieContainer = null)
        {
            MemoryStream ms = new MemoryStream();
            FillJsonDataStream(jsonData, ms);//填充formData
            return HttpPost(url, ms, encoding, IsJsonContentType, cookieContainer);
        }

        public static string HttpPost(string url, Stream postStream = null, Encoding encoding = null, bool IsJsonContentType = false, CookieContainer cookieContainer = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentLength = postStream != null ? postStream.Length : 0;
            if (IsJsonContentType)
            {
                request.ContentType = "application/json; charset=UTF-8";
            }
            else
            {
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            }
            request.Accept = "application/json, text/javascript, */*; q=0.01";
            request.KeepAlive = true;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36";

            if (cookieContainer != null)
            {
                request.CookieContainer = cookieContainer;
            }

            #region 输入二进制流
            if (postStream != null)
            {
                postStream.Position = 0;
                //直接写入流
                Stream requestStream = request.GetRequestStream();
                StreamWriter myStreamWriter = new StreamWriter(requestStream, Encoding.UTF8);

                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = postStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                }

                postStream.Seek(0, SeekOrigin.Begin);
                StreamReader sr = new StreamReader(postStream);
                var postStr = sr.ReadToEnd();
                postStream.Close();//关闭
            }
            #endregion

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (cookieContainer != null)
            {
                response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            }

            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader myStreamReader = new StreamReader(responseStream, encoding ?? Encoding.GetEncoding("utf-8")))
                {
                    string retString = myStreamReader.ReadToEnd();
                    return retString;
                }
            }
        }

        public static void FillFormDataStream(Dictionary<string, string> formData, Stream stream)
        {
            string dataString = GetQueryString(formData);
            var formDataBytes = formData == null ? new byte[0] : Encoding.UTF8.GetBytes(dataString);
            stream.Write(formDataBytes, 0, formDataBytes.Length);
            stream.Seek(0, SeekOrigin.Begin);//设置指针读取位置
        }

        public static void FillJsonDataStream(string jsonData, Stream stream)
        {
            var formDataBytes = jsonData == null ? new byte[0] : Encoding.UTF8.GetBytes(jsonData);
            stream.Write(formDataBytes, 0, formDataBytes.Length);
            stream.Seek(0, SeekOrigin.Begin);//设置指针读取位置
        }

        public static string GetQueryString(this Dictionary<string, string> formData)
        {
            if (formData == null || formData.Count == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();

            var i = 0;
            foreach (var kv in formData)
            {
                i++;
                sb.AppendFormat("{0}={1}", kv.Key, kv.Value);
                if (i < formData.Count)
                {
                    sb.Append("&");
                }
            }

            return sb.ToString();
        }

        public static string HttpGet(string url, Dictionary<string, string> formData = null, CookieCollection cookies = null)
        {
            if (formData != null)
            {
                string dataString = GetQueryString(formData);
                if (url.Contains("?"))
                {
                    url += "&";
                }
                else
                {
                    url += "?";
                }
                url += dataString;
            }
            HttpWebRequest request = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                //对服务端证书进行有效性校验（非第三方权威机构颁发的证书，如自己生成的，不进行验证，这里返回true）
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;    //http版本，默认是1.1,这里设置为1.0
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "GET";

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (System.Net.WebException webException)
            {
                var httpResponse = webException.Response as System.Net.HttpWebResponse;
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new MallException("接口无法找到！请确认接口正常：【" + url + "】");
                }
                else
                {
                    throw new MallException("网络请求错误");
                }
            }
            using (Stream s = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                return reader.ReadToEnd();

            }
        }

        /// <summary>
        /// 验证证书
        /// </summary>
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;
            return false;
        }
    }
}
