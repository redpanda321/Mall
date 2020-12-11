using Mall.Core;
using Mall.Core.Plugins;
using Mall.Core.Plugins.Payment;
using Mall.PaymentPlugin;
using Mall.WeixinPaymentBase;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace Mall.Plugin.Payment.WeiXinPay_MiniProg
{
    public class Service : ServiceBase, IPaymentPlugin
    {
        public string GetRequestUrl(string returnUrl, string notifyUrl, string orderId, decimal totalFee, string productInfo, string openId = null)
        {
            string timestamp = "";
            string noncestr = "";
            string str3 = "";
            string str4 = orderId;
            string str5 = DateTime.Now.ToString("yyyyMMdd");
            RequestHandler handler = new RequestHandler();
            handler.Init();
            timestamp = TenPayUtil.GetTimestamp();
            noncestr = TenPayUtil.GetNoncestr();
            Config config = Utility<Config>.GetConfig(WorkDirectory);
            handler.SetParameter("appid", config.AppId);
            handler.SetParameter("mch_id", config.MCHID);
            handler.SetParameter("nonce_str", noncestr);
            handler.SetParameter("body", productInfo);
            handler.SetParameter("out_trade_no", str4);
            handler.SetParameter("total_fee", ((int)(totalFee * 100M)).ToString());
            handler.SetParameter("spbill_create_ip", _httpContextAccessor.HttpContext.Features.Get<IHttpConnectionFeature>().RemoteIpAddress.ToString());
            handler.SetParameter("notify_url", notifyUrl);
            handler.SetParameter("trade_type", "JSAPI");
            handler.SetParameter("openid", string.IsNullOrWhiteSpace(openId) ? "" : openId);
            string str6 = handler.CreateMd5Sign("key", config.Key);
            handler.SetParameter("sign", str6);
            string str7 = handler.ParseXML();
           // Log.Debug("data=" + str7);
            XDocument document = XDocument.Parse(TenPayV3.Unifiedorder(str7));
            if (document == null)
            {
                throw new ApplicationException("调用统一支付出错：请求内容：" + str7);
            }
            if (document.Element("xml").Element("return_code").Value == "FAIL")
            {
                throw new ApplicationException("预支付失败：" + document.Element("xml").Element("return_msg").Value);
            }
            if (document.Element("xml").Element("result_code").Value == "FAIL")
            {
                throw new ApplicationException("预支付失败：" + document.Element("xml").Element("err_code_des").Value);
            }
            string str11 = document.Element("xml").Element("prepay_id").Value;
            RequestHandler handler2 = new RequestHandler();
            handler2.SetParameter("appId", config.AppId);
            handler2.SetParameter("nonceStr", noncestr);
            handler2.SetParameter("timeStamp", timestamp);
            handler2.SetParameter("package", "prepay_id=" + str11);
            handler2.SetParameter("signType", "MD5");
            str3 = handler2.CreateMd5Sign("key", config.Key);
            handler2.SetParameter("sign", str3);
            handler2.SetParameter("prepayId", str11);
            Hashtable allParameters = handler2.GetAllParameters();
            StringBuilder builder = new StringBuilder();
            foreach (object obj2 in allParameters.Keys)
            {
                builder.Append(obj2 + ":" + allParameters[obj2]);
                builder.Append(",");
            }
            char[] trimChars = new char[] { ',' };
            return builder.ToString().TrimEnd(trimChars);
        }

        public EnterprisePayNotifyInfo ProcessEnterprisePayNotify(IHttpContextAccessor context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 属性
        /// </summary>
        public string HelpImage
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public UrlType RequestUrlType
        {
            get
            {
                return 0;
            }
        }

    }
}
