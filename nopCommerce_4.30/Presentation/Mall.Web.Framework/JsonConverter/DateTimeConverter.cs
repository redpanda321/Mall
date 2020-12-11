using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Web.Framework
{
    public class DateTimeConverter : JsonConverter
    {
        private string format = "yyyy-MM-dd HH:mm:ss";
        
        public DateTimeConverter() { }
        public DateTimeConverter(string format)
        {
            this.format = format;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                var v = (DateTime)value;
                writer.WriteValue(v.ToString(format));
            }
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Convert.ToDateTime(reader.Value);
        }
        public override bool CanConvert(Type objectType)
        {
            var result = false;
            var name = objectType.FullName;
            if (name == "System.DateTime")
                result = true;// 处理System.DateTime
            if (name.IndexOf("System.Nullable`1[[System.DateTime,") >= 0)
                result = true;// 处理DateTime?
            return result;
        }
        
    }
}
