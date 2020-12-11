using Mall.Core;
using Mall.Core.Plugins.Payment;
using Mall.DTO;
using Mall.IServices;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Application
{

    public class PaymentApplication
    {
        /// <summary>
        /// 用于前台显示统一支付名称调用(一个端只会有一个微信或支付宝名称)(微信和支付宝多个端口描述不同名称统一[微信支付|支付宝支付])
        /// </summary>
        public static string GetForeGroundPaymentName(string displayName)
        {
            if (!string.IsNullOrEmpty(displayName))
            {
                if (displayName.IndexOf("微信") != -1)
                    displayName = "微信支付";//微信支付有(微信PC端扫描支付,微信H5支付,微信App支付,微信公众号支付（用于微信商城）--都统一微信支付)
                else if(displayName.IndexOf("小程序") != -1)
                    displayName = "微信支付";
                else if (displayName.IndexOf("支付宝") != -1)
                    displayName = "支付宝支付";//支付宝支付有(支付宝H5支付，支付宝App支付，支付宝PC端支付 --都统一支付宝支付)
            }
            return displayName;
        }


        /// <summary>
        /// 获取支付方式的描述
        /// </summary>
        /// <returns></returns>
        public static List<PaymentType> GetPaymentTypeDesc()
        {
            var payPlugins = Core.PluginsManagement.GetPlugins<IPaymentPlugin>().Select(t => t.PluginInfo);
            List<PaymentType> result = new List<PaymentType>();
            var plugins = payPlugins.Select(e=>e.Description).DistinctBy(e => e);
            foreach (var p in plugins)
            {
                result.Add(new PaymentType(p, p));
            }
            return result;
        }
        /// <summary>
        /// 获取单个支付方式的描述
        /// </summary>
        /// <returns></returns>
        public static string GetPaymentTypeDescById(string pluginid)
        {
            if (string.IsNullOrEmpty(pluginid))
            {
                return null;
            }
            var payPlugins = Core.PluginsManagement.GetPlugins<IPaymentPlugin>().FirstOrDefault(t => t.PluginInfo.PluginId == pluginid);
            if (payPlugins == null)
            {
                return null;
            }
            return payPlugins.PluginInfo.Description;
        }
        /// <summary>
        /// 根据支付方式描述（微信支付），获取支付方式ID
        /// <para>Mall.Plugin.Payment.WeiXinPay_Native</para>
        /// <para>Mall.Plugin.Payment.WeiXinPay</para>
        /// </summary>
        /// <returns></returns>
        public static List<string> GetPaymentIdByDesc(string desc)
        {
            if (string.IsNullOrEmpty(desc))
                return null;
            var payPlugins = Core.PluginsManagement.GetPlugins<IPaymentPlugin>().Where(e=>e.PluginInfo.Description== desc).Select(t => t.PluginInfo);
            return payPlugins.Select(e => e.PluginId).ToList();
        }
    }
}
