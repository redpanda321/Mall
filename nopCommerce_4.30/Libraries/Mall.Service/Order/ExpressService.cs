using Mall.Core;
using Mall.Core.Plugins;
using Mall.Core.Plugins.Express;
using Mall.IServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

using Mall.Entities;
using NetRube.Data;
using Mall.DTO;
using Mall.Application;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Xml;
using Mall.Core.Helper;

namespace Mall.Service
{
    public class ExpressService : ServiceBase, IExpressService
    {

        const string RELATEIVE_PATH = "/Plugins/Express/";

        public IEnumerable<ExpressInfoInfo> GetAllExpress()
        {
            var express = DbFactory.Default.Get<ExpressInfoInfo>().ToList();
            return express;
        }

        public ExpressInfoInfo GetExpress(string name)
        {
            var express = DbFactory.Default.Get<ExpressInfoInfo>().Where(e => e.Name == name).FirstOrDefault();
            return express;
        }

        public IEnumerable<ExpressElementInfo> GetExpressElements(long expressid)
        {
            var elements = DbFactory.Default.Get<ExpressElementInfo>().Where(e => e.ExpressId == expressid).ToList();
            return elements;
        }

        public IEnumerable<ExpressInfoInfo> GetRecentExpress(long shopId, int takeNumber)
        {
            var expressNames = DbFactory.Default.Get<OrderInfo>().Where(item => item.ShopId == shopId && item.ExpressCompanyName.ExIfNull("") != "").OrderByDescending(item => item.Id).Select(item => item.ExpressCompanyName).Take(takeNumber).ToList<string>();
            //所有开启的运费模板
            var allExpress = GetAllExpress().Where(e => e.Status == CommonModel.ExpressStatus.Open);
            var selectedExpresses = allExpress.Where(item => expressNames.Contains(item.Name)).ToList();
            if (selectedExpresses.Count < takeNumber)
            {
                Random rand = new Random();
                selectedExpresses.AddRange(allExpress.Where(item => !expressNames.Contains(item.Name)).OrderBy(item => rand.Next()).Take(takeNumber - selectedExpresses.Count));
            }
            return selectedExpresses;
        }

        #region 
        public void AddExpress(ExpressInfoInfo model)
        {
            DbFactory.Default.Add(model);
        }

        public void UpdateExpressCode(ExpressInfoInfo model)
        {
            var express = DbFactory.Default.Get<ExpressInfoInfo>().Where(e => e.Id == model.Id).FirstOrDefault();
            if (express != null)
            {
                express.Name = model.Name;
                express.Kuaidi100Code = model.Kuaidi100Code;
                express.KuaidiNiaoCode = model.KuaidiNiaoCode;
                express.TaobaoCode = model.TaobaoCode;
                DbFactory.Default.Update(express);
            }
        }

        public ExpressInfoInfo ClearExpressData(long id)
        {
            var express = DbFactory.Default.Get<ExpressInfoInfo>().Where(e => e.Id == id).FirstOrDefault();
            if (express != null)
            {
                express.BackGroundImage = string.Empty;
                //var expressElements = DbFactory.Default.Get<ExpressElementInfo>().Where(e => e.ExpressId == id).ToList();
                DbFactory.Default.Del<ExpressElementInfo>(e => e.ExpressId == id);
            }
            return express;
        }

        public void ChangeExpressStatus(long id, CommonModel.ExpressStatus status)
        {
            //var express = DbFactory.Default.Get<ExpressInfoInfo>().Where(e => e.Id == id).FirstOrDefault();
            //if (express == null)
            //{
            //    throw new MallException("快递公司不存在");
            //}
            //express.Status = status;
            //DbFactory.Default.Update(express);
            var flag = DbFactory.Default.Set<ExpressInfoInfo>().Set(n => n.Status, status).Where(e => e.Id == id).Succeed();
            if (!flag) throw new MallException("快递公司不存在");
        }

        public void UpdateExpressAndElement(ExpressInfoInfo express, ExpressElementInfo[] elements)
        {
            var model = DbFactory.Default.Get<ExpressInfoInfo>().Where(e => e.Name == express.Name).FirstOrDefault();
            if (model == null)
            {
                throw new MallException("未找到快递模板：" + express.Name);
            }
            model.BackGroundImage = express.BackGroundImage;
            model.Height = express.Height;
            model.Width = express.Width;

            //var elementModels = DbFactory.Default.Get<ExpressElementInfo>().Where(e => e.ExpressId == model.Id).ToList();
            DbFactory.Default.Del<ExpressElementInfo>(e => e.ExpressId == model.Id);

            foreach (var item in elements)
            {
                item.ExpressId = model.Id;
            }

            DbFactory.Default.Add<ExpressElementInfo>(elements);
            DbFactory.Default.Set<ExpressInfoInfo>(model);
        }


        #endregion

        public ExpressData GetExpressData(string expressCompanyName, string shipOrderNumber)
        {
            var settting = SiteSettingApplication.SiteSettings;
            var expressData = new ExpressData();//创建对象
            var expressInfo= GetExpress(expressCompanyName);
            if (expressInfo != null)
            {
                if (expressInfo.KuaidiNiaoCode.Contains("SF") && !string.IsNullOrWhiteSpace(settting.SFKuaidiCustomerCode))
                {
                    return GetExpressDataSF(shipOrderNumber);
                }
                //使用快递100查询快递数据
                if (settting.KuaidiType.Equals(0))
                {
                    expressData = GetExpressDataByKey(expressInfo.Kuaidi100Code, shipOrderNumber);
                }
                else//快递鸟
                {
                    expressData = GetExpressDataByKuai(expressInfo.KuaidiNiaoCode, shipOrderNumber);
                }
            }
            return expressData;
        }

        /// <summary>
        /// 快递100物流查看
        /// </summary>
        /// <param name="kuaidi100Code"></param>
        /// <param name="shipOrderNumber"></param>
        /// <returns></returns>
        private ExpressData GetExpressDataByKey(string kuaidi100Code, string shipOrderNumber)
        {
            var settting = SiteSettingApplication.SiteSettings;
            var key = settting.Kuaidi100Key;
            var expressData = new ExpressData();//创建对象
            expressData.Success = false;
            expressData.Message = "暂无物流信息";
            if (!string.IsNullOrEmpty(key))
            {
                var model = GetKuaidi100ExpressData(kuaidi100Code, shipOrderNumber);
                if (model != null)
                {
                    var content = model.DataContent;
                    var obj = new
                    {
                        message = string.Empty,
                        lastResult = new
                        {
                            data = new[] {
                             new{
                                context=string.Empty,
                                time=string.Empty,
                                ftime=string.Empty
                            }}
                        }
                    };

                    var m = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(content, obj);
                    expressData.Success = true;
                    expressData.Message = m.message;
                    var dataItems = new List<ExpressDataItem>();
                    foreach (var t in m.lastResult.data)
                    {
                        dataItems.Add(new ExpressDataItem()
                        {
                            Time = DateTime.ParseExact(t.ftime, "yyyy-MM-dd HH:mm:ss", null),
                            Content = t.context.ToString()
                        });
                    }
                    expressData.ExpressDataItems = dataItems;
                }
            }
            return expressData;
        }

        /// <summary>
        /// 百度物流查看
        /// </summary>
        /// <param name="kuaidi100Code"></param>
        /// <param name="shipOrderNumber"></param>
        /// <returns></returns>
        private ExpressData GetExpressDataFree(string kuaidi100Code, string shipOrderNumber)
        {
            var expressData = new ExpressData();//创建对象
            string url = string.Format("https://sp0.baidu.com/9_Q4sjW91Qh3otqbppnN2DJv/pae/channel/data/asyncqury?appid=4001&com={0}&nu={1}", kuaidi100Code, shipOrderNumber);//快递查询地址
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Timeout = 8000;
            request.Host = "sp0.baidu.com";
            request.Method = "GET";
            request.ContentType = "application/json;charset=UTF-8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.86 Safari/537.36";
            CookieContainer cookie = new CookieContainer();
            Cookie ck = new Cookie("BAIDUID", "65C7509B002B15BE3E4EEE6B5366AFEA:FG=1", "/", ".baidu.com");
            ck.Expires = DateTime.Now.AddYears(1);
            cookie.Add(ck);
            request.CookieContainer = cookie;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();
                    System.IO.StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.GetEncoding("UTF-8"));
                    StringBuilder content = new StringBuilder(streamReader.ReadToEnd());// 读取流字符串内容
                    content.Replace("&amp;", "").Replace("&nbsp;", "").Replace("&", "");//去除不需要的字符
                    // var jsonData = JObject.Parse(content.ToString());
                    var test = new
                    {
                        msg = "",
                        status = "",
                        data = new
                        {
                            info = new
                            {
                                status = "",
                                state = "",
                                context = new[]{
                               new{time="",desc=""}
                            }
                            }
                        }
                    };
                    var m = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(content.ToString(), test);
                    expressData.Success = true;
                    expressData.Message = m.msg;
                    var dataItems = new List<ExpressDataItem>();
                    foreach (var t in m.data.info.context)
                    {
                        dataItems.Add(new ExpressDataItem()
                        {
                            Time = GetTime(t.time),
                            // Time = DateTime.ParseExact(datetime, "yyyy-MM-dd HH:mm:ss", null),
                            Content = t.desc.ToString()
                        });
                    }
                    expressData.ExpressDataItems = dataItems;
                    return expressData;
                }
                else
                {
                    expressData.Message = "网络错误";
                }
            }
            catch (Exception ex)
            {
                Core.Log.Error(string.Format("快递查询错误:{{kuaidi100Code:{0},shipOrderNumber:{1}}}", kuaidi100Code, shipOrderNumber), ex);
                expressData.Message = "未知错误";
            }
            return expressData;
        }

        /// <summary>
        /// 快递鸟物流查看
        /// </summary>
        /// <param name="shipperCode">物流公司编码</param>
        /// <param name="logisticsCode">物流号</param>
        /// <returns></returns>
        private ExpressData GetExpressDataByKuai(string shipperCode, string logisticsCode)
        {
            var settting = SiteSettingApplication.SiteSettings;

            //签名
            SortedDictionary<string, string> data = new SortedDictionary<string, string>();
            data.Add("app_key", settting.KuaidiApp_key);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.Add("logisticsCode", logisticsCode);
            data.Add("shipperCode", shipperCode);
            data.Add("timestamp", timestamp);

            string si = string.Format("app_key{0}logisticsCode{1}shipperCode{2}timestamp{3}{4}", settting.KuaidiApp_key, logisticsCode, shipperCode, timestamp, settting.KuaidiAppSecret);
            //string sign = FormsAuthentication.HashPasswordForStoringInConfigFile(si, "MD5");
            string sign = SecureHelper.MD5(si);

            var expressData = new ExpressData();//创建对象
            string url = string.Format("http://wuliu.kuaidiantong.cn/api/logistics?app_key={0}&timestamp={1}&shipperCode={2}&logisticsCode={3}&sign={4}", settting.KuaidiApp_key, timestamp, shipperCode, logisticsCode, sign);//快递查询地址
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Timeout = 8000;
            request.Method = "GET";
            request.ContentType = "application/json;charset=UTF-8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.86 Safari/537.36";
            CookieContainer cookie = new CookieContainer();
            request.CookieContainer = cookie;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();
                    System.IO.StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.GetEncoding("UTF-8"));
                    StringBuilder content = new StringBuilder(streamReader.ReadToEnd());// 读取流字符串内容
                    content.Replace("&amp;", "").Replace("&nbsp;", "").Replace("&", "");//去除不需要的字符

                    var test = new
                    {
                        shipperCode = "",
                        logisticsCode = "",
                        success = true,
                        state = "",
                        traces = new[]
                        {
                            new{
                                acceptTime="",
                                acceptStation="",
                                remark=""
                            }
                        }
                    };
                    var m = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(content.ToString(), test);

                    var dataItems = new List<ExpressDataItem>();
                    if (m.success == true)
                    {
                        expressData.Success = true;
                        expressData.Message = "成功";
                        foreach (var t in m.traces)
                        {
                            dataItems.Add(new ExpressDataItem()
                            {
                                Time = DateTime.Parse(t.acceptTime),
                                Content = t.acceptStation
                            });
                        }
                    }
                    else
                    {
                        expressData.Success = false;
                        expressData.Message = "暂无物流信息";
                    }
                    expressData.ExpressDataItems = dataItems;
                    return expressData;
                }
                else
                {
                    //  expressData.Message = "网络错误";
                    expressData.Message = "网络错误";
                }
            }
            catch (Exception ex)
            {
                Core.Log.Error(string.Format("快递查询错误:{{kuaidi100Code:{0},shipOrderNumber:{1}}}", shipperCode, logisticsCode), ex);
                //  expressData.Message = "未知错误";
                expressData.Message = "暂无物流信息";
            }
            return expressData;
        }

        public ExpressData GetExpressDataSF(string logisticsCode)
        {
            //https://qiao.sf-express.com/pages/developDoc/index.html?level2=296618&level3=902583&level4=893568
            var settting = SiteSettingApplication.SiteSettings;
            //查询请求XML报文
            string xml = string.Format(
                "<Request service='RouteService' lang='zh-CN'>" +
                "<Head>{0}</Head>" +
                "<Body>" +
                "<RouteRequest tracking_type = '1' method_type = '1' tracking_number = '{1}'/>" +
                "</Body>" +
                "</Request>", settting.SFKuaidiCustomerCode, logisticsCode);

            string Checkword = settting.SFKuaidiCheckWord;//开发者账号对应的校验码; // "j8DzkIFgmlomPt0aLuwU";
            string verifyCode = MD5ToBase64String(xml + Checkword);//生成接口校验码
            string requestUrl = "http://bsp-oisp.sf-express.com/bsp-oisp/sfexpressService";//测试、生产环境同一个地址 

            string result = DoPostSF(requestUrl, xml, verifyCode);//返回结果XML 
            bool isSuccess = false;
            ExpressData data = new ExpressData();
            var itemList = new List<ExpressDataItem>();
            //解析返回结果
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(result);
            var headNode = xmlDoc.SelectSingleNode("/Response/Head");
            if (headNode != null)
            {
                if (headNode.InnerText.ToLower() == "ok")
                {
                    isSuccess = true;
                }
                else
                {//错误记录日志
                    var errNode= xmlDoc.SelectSingleNode("/Response/ERROR");
                    if (errNode != null)
                    {
                        data.Message = errNode.InnerText;
                    }
                }
            }
            if (isSuccess)
            {
                var routeNodes = xmlDoc.SelectNodes("/Response/Body/RouteResponse/Route");
                DateTime dt = new DateTime();
                string address = string.Empty;
                foreach (XmlNode route in routeNodes)
                {
                    ExpressDataItem item = new ExpressDataItem();
                    if (DateTime.TryParse(route.Attributes["accept_time"].Value, out dt))
                    {
                        item.Time = dt;
                    }
                    if (route.Attributes["accept_address"] != null)
                    {//accept_address 可能没有
                        address = route.Attributes["accept_address"].Value;
                    }
                    item.Content = address + " " + route.Attributes["remark"].Value;
                    itemList.Add(item);
                }
            }
            data.ExpressDataItems = itemList;
            data.Success = isSuccess;
            return data;

            /*
             <Response service="RouteService">
                <Head>OK</Head>
                <Body>
                <RouteResponse mailno="444003077898">
                <Route accept_time="2015-01-04 10:11:26" accept_address="深圳" remark="已收件" opcode="50"/>
                <Route accept_time="2015-01-05 17:41:50" remark="此件签单返还的单号为 123638813180" opcode="922"/>
                <RouteResponse mailno="444003077899">
                <Route accept_time="2015-01-04 10:11:26" accept_address="深圳" remark="已收件" opcode="50"/>
                <Route accept_time="2015-01-05 17:41:50" remark="此件签单返还的单号为 123638813181" opcode="922"/>
                </RouteResponse>
                </Body>
            </Response>
            //失败
            <Response service=”RouteService”>
                <Head>ERR</Head>
                <ERROR code="4001">系统发生数据错误或运行时异常</ERROR>
            </Response>
             */

        }
        public string MD5ToBase64String(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] MD5 = md5.ComputeHash(Encoding.UTF8.GetBytes(str));//MD5(注意UTF8编码) 
            string result = Convert.ToBase64String(MD5);//Base64 
            return result; 
        }
        public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        { // 总是接受 
            return true; 
        }
        public string DoPostSF(string Url, string xml, string verifyCode)
        { 
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
            string postData = string.Format("xml={0}&verifyCode={1}",xml, verifyCode); //请求 
            WebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            request.ContentLength = Encoding.UTF8.GetByteCount(postData);
            byte[] postByte = Encoding.UTF8.GetBytes(postData);
            Stream reqStream = request.GetRequestStream();
            reqStream.Write(postByte, 0, postByte.Length);
            reqStream.Close(); 
            //读取 
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        }
        
        /// <summary>
        /// 时间戳转为C#格式时间
        /// </summary>
        /// <param name="timeStamp">Unix时间戳格式</param>
        /// <returns>C#格式时间</returns>
        private DateTime GetTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
        public void SubscribeExpress100(string expressCompanyName, string number, string kuaidi100Key, string city, string redirectUrl)
        { //使用快递100查询快递数据
            string kuaidi100Code = GetExpress(expressCompanyName).Kuaidi100Code;//根据快递公司名称获取对应的快递100编码
            if (string.IsNullOrEmpty(kuaidi100Key) || string.IsNullOrEmpty(expressCompanyName) || string.IsNullOrEmpty(number))
            {
                Core.Log.Info("没有设置快递100Key");
                return;
            }
            string url = "http://www.kuaidi100.com/poll";
            System.Net.WebClient WebClientObj = new System.Net.WebClient();
            NameValueCollection PostVars = new NameValueCollection();
            PostVars.Add("schema", "json");
            var model = new
            {
                company = kuaidi100Code,
                number = number,
                key = kuaidi100Key,
                to = city,
                parameters = new { callbackurl = redirectUrl }
            };
            var param = Newtonsoft.Json.JsonConvert.SerializeObject(model);
            PostVars.Add("param", param);
            try
            {
                byte[] byRemoteInfo = WebClientObj.UploadValues(url, "POST", PostVars);
                string output = System.Text.Encoding.UTF8.GetString(byRemoteInfo);
                //注意返回的信息，只有result=true的才是成功
                var result = new { result = false, resultcode = "", message = "" };
                var m = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(output, result);
                Core.Log.Info(output + "," + redirectUrl + "物流公司代码：" + kuaidi100Code + ",物流单号:" + number + ",物流到达城市:" + city); //日志记录看成功否
                if (!m.result)
                {
                    Core.Log.Error("物流通知订阅失败：" + result.message + ",物流公司代码：" + kuaidi100Code + ",物流单号:" + number + ",物流到达城市:" + city + ",快递Key:" + kuaidi100Key + "回调地址：" + redirectUrl);
                }
            }
            catch (Exception ex)
            {
                Core.Log.Error("物流通知订阅失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 保存快递100回调的物流信息
        /// </summary>
        /// <param name="model"></param>
        public void SaveExpressData(OrderExpressDataInfo model)
        {
            var m = DbFactory.Default.Get<OrderExpressDataInfo>().Where(a => a.CompanyCode == model.CompanyCode && a.ExpressNumber == model.ExpressNumber).FirstOrDefault();
            if (m != null)
            {
                m.DataContent = model.DataContent;
                DbFactory.Default.Update(m);
            }
            else
            {
                DbFactory.Default.Add(model);
            }
        }

        private OrderExpressDataInfo GetKuaidi100ExpressData(string companyCode, string number)
        {
            var m = DbFactory.Default.Get<OrderExpressDataInfo>().Where(a => a.CompanyCode == companyCode && a.ExpressNumber == number).FirstOrDefault();
            return m;
        }

        public void DeleteExpress(long id)
        {
            //var expressElements = DbFactory.Default.Get<ExpressElementInfo>().Where(e => e.ExpressId == id).ToList();
            //var express = DbFactory.Default.Get<ExpressInfoInfo>().Where(e => e.Id == id).FirstOrDefault();
            //if (expressElements.Count() > 0)
            //{
            //    DbFactory.Default.Del<ExpressElementInfo>(expressElements);
            //}
            //DbFactory.Default.Del(express);
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Del<ExpressElementInfo>(e => e.ExpressId == id);
                DbFactory.Default.Del<ExpressInfoInfo>(e => e.Id == id);
            });
        }
    }
}
