using Mall.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Mall.Web.Framework
{
   class JsonNetResult : JsonResult
    { 
        public JsonSerializerSettings SerializerSettings { get; set; }
        public Formatting Formatting { get; set; }



        
        public JsonNetResult(object value): base(value)
        {

            
            
            SerializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new HierpContractResolver(),
                Converters = new List<JsonConverter> {
                    new DateTimeConverter()
                }
            };
        }
        

       

        class HierpContractResolver : CamelCasePropertyNamesContractResolver
        {
            public HierpContractResolver()
            {
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> properties = base.CreateProperties(type, MemberSerialization.Fields);
                return properties;
            }
        }

        public  Task ExecuteResultAsync(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            HttpResponse response = context.HttpContext.Response;
            response.ContentType =
                !string.IsNullOrEmpty(ContentType) ? ContentType : "application/json";
            if (ContentType != null)
                response.ContentType = ContentType;
            if (Value != null)
            {

                /*
                   JsonTextWriter writer = new JsonTextWriter(response.Body)
                   {
                       Formatting = Formatting
                   };
                   JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
                   serializer.Serialize(writer, Value);
                   writer.Flush();
                    */

                string jsonText = JsonConvert.SerializeObject(Value);

                response.WriteAsync(jsonText);  
            
            }

            return null;
        }
    }
}
