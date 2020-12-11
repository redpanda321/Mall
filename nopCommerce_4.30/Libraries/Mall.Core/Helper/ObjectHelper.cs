
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Mall.Core
{
    public static class ObjectHelper
    {
        /// <summary>
        /// 深复制
        /// </summary>
        /// <param name="obj">待复制的对象</param>
        /// <returns></returns>
        public static object DeepColne(object obj)
        {
            var objJson = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            var objCopy = Newtonsoft.Json.JsonConvert.DeserializeObject(objJson);
            return objCopy;
        }


        /// <summary>
        /// 深复制
        /// </summary>
        /// <param name="obj">待复制的对象</param>
        /// <returns></returns>
        public static T DeepColne<T>(T t)
        {
            var objJson = Newtonsoft.Json.JsonConvert.SerializeObject(t);
            var objCopy = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(objJson);
            return objCopy;
        }

        /// <summary>
        /// 把对象转换为JSON字符串
        /// </summary>
        /// <param name="o">对象</param>
        /// <param name="bDateTimeFormat">是否格式化时间</param>
        /// <param name="strFormat">自定义格式化</param>
        /// <returns>JSON字符串</returns>
        public static string ToJSON(this object o, bool bDateTimeFormat = true, string strFormat = "")
        {
            if (o == null)
            {
                return string.Empty;
            }
            if (!bDateTimeFormat) return JsonConvert.SerializeObject(o);

            JsonSerializerSettings jsonSetting = new JsonSerializerSettings();
            jsonSetting.NullValueHandling = NullValueHandling.Ignore;
            IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
            timeConverter.DateTimeFormat = string.IsNullOrEmpty(strFormat) ? "yyyy-MM-dd HH:mm:ss" : strFormat;
            jsonSetting.Converters.Add(timeConverter);
            //DecimalJsonFormatConverter decimalConverter = new DecimalJsonFormatConverter();
            //jsonSetting.Converters.Add(decimalConverter);
            return JsonConvert.SerializeObject(o, jsonSetting);
        }
        /// <summary>
        /// 把JSON文本转为实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="settings">Specifies the settings on a Newtonsoft.Json.JsonSerializer object.</param>
        /// <param name="defaultvalue">默认值</param>
        /// <returns></returns>
        public static T FromJSON<T>(this string input, T defaultvalue = default(T), JsonSerializerSettings settings = null)
        {
            try
            {
                if (null == settings)
                    return JsonConvert.DeserializeObject<T>(input);
                else
                    return JsonConvert.DeserializeObject<T>(input, settings);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return defaultvalue;
            }
        }
    }
}
