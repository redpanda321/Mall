using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

using System.Xml.Serialization;

namespace Mall.Web.Framework
{
    //TODO:ZJT 为微信接口做准备，微信部分接口回调需要返回XML
    public class XmlResult : ActionResult
    {
        // 可被序列化的内容
        object Data { get; set; }
        string Xml { get; set; }

        // Data的类型
        private Type _dataType = null;

        private XmlResultType _type;

        // 构造器
        public XmlResult( object data  )
        {
            this._type = XmlResultType.Object;
            Data = data;
            _dataType = data.GetType();
        }

        public XmlResult( string xml )
        {
            this._type = XmlResultType.String;
            Xml = xml;
        }

        public override void ExecuteResult( ActionContext context )
        {
            if( context == null )
            {
                throw new ArgumentNullException( "context" );
            }

            HttpResponse response = context.HttpContext.Response;

            // 设置 HTTP Header
            response.ContentType = "text/xml";

            switch( this._type )
            { 
                case XmlResultType.Object :
                    if( Data != null )
                    {
                        // 序列化 Data 并写入 Response
                        XmlSerializer serializer = new XmlSerializer( _dataType );
                        MemoryStream ms = new MemoryStream();
                        serializer.Serialize( ms , Data );
                        response.WriteAsync( System.Text.Encoding.UTF8.GetString( ms.ToArray() ) );
                    }
                    break;
                case XmlResultType.String:
                    response.WriteAsync( Xml );
                    break;
            }
        }
    }

    public enum XmlResultType
    { 
        Object = 1,

        String = 2
    }
}
