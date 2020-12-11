using Mall.Core;
using Mall.Core.Plugins.Message;
using Mall.MessagePlugin;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Mall.Plugin.Message.SMS
{
    class SMSCore
    {
        /// <summary>
        /// 工作目录
        /// </summary>
        public static string WorkDirectory { get; set; }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        public static MessageSMSConfig GetConfig()
        {
            MessageSMSConfig config = new MessageSMSConfig();
            string sDirectory = Mall.Core.Helper.IOHelper.urlToVirtual(WorkDirectory) + "/Data/SMS.config";

            if (MallIO.ExistFile(sDirectory))
            {
                XmlSerializer xs = new XmlSerializer(typeof(MessageSMSConfig));
                byte[] b = Mall.Core.MallIO.GetFileContent(sDirectory);
                string str = System.Text.Encoding.Default.GetString(b);
                MemoryStream fs = new MemoryStream(b);
                config = (MessageSMSConfig)xs.Deserialize(fs);
            }
            return config;
        }

        /// <summary>
        /// 获取信息内容
        /// </summary>
        /// <returns></returns>
        public static MessageContent GetMessageContentConfig()
        {
            MessageContent config = Core.Cache.Get<MessageContent>("SMSMessageContent") as MessageContent;
            if (config == null)
            {
                //using (FileStream fs = new FileStream(WorkDirectory + "\\Data\\MessageContent.xml", FileMode.Open))
                //{
                //    XmlSerializer xs = new XmlSerializer(typeof(MessageContent));
                //    config = (MessageContent)xs.Deserialize(fs);
                //    Core.Cache.Insert("MessageContent", config);
                //}

                string sDirectory = Mall.Core.Helper.IOHelper.urlToVirtual(WorkDirectory) + "/Data/MessageContent.xml";

                if (MallIO.ExistFile(sDirectory))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(MessageContent));
                    byte[] b = Mall.Core.MallIO.GetFileContent(sDirectory);
                    string str = System.Text.Encoding.Default.GetString(b);
                    MemoryStream fs = new MemoryStream(b);
                    config = (MessageContent)xs.Deserialize(fs);
                    Core.Cache.Insert("SMSMessageContent", config);
                }
            }
            return config;
        }


        /// <summary>
        /// 获取发送状态
        /// </summary>
        /// <returns></returns>
        public static MessageStatus GetMessageStatus()
        {
            MessageStatus config = new MessageStatus();
            string sDirectory = Mall.Core.Helper.IOHelper.urlToVirtual(WorkDirectory) + "/Data/config.xml";
            
            if (MallIO.ExistFile(sDirectory))
            {
                XmlSerializer xs = new XmlSerializer(typeof(MessageStatus));
                byte[] b = Mall.Core.MallIO.GetFileContent(sDirectory);
                MemoryStream fs = new MemoryStream(b);

                config = (MessageStatus)xs.Deserialize(fs);
            }
            return config;
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="config"></param>
        public static void SaveConfig(MessageSMSConfig config)
        {
            //using (FileStream fs = new FileStream(WorkDirectory + "\\Data\\SMS.config", FileMode.Create))
            //{
            //    XmlSerializer xs = new XmlSerializer(typeof(MessageSMSConfig));
            //    xs.Serialize(fs, config);
            //}

            string sDirectory = Mall.Core.Helper.IOHelper.urlToVirtual(WorkDirectory) + "/Data/SMS.config";
            XmlSerializer xml = new XmlSerializer(typeof(MessageSMSConfig));
            MemoryStream Stream = new MemoryStream();
            xml.Serialize(Stream, config);

            byte[] b = Stream.ToArray();
            MemoryStream stream2 = new MemoryStream(b);
            Mall.Core.MallIO.CreateFile(sDirectory, stream2, Core.FileCreateType.Create);
        }

        /// <summary>
        /// 保存短信内容配置
        /// </summary>
        /// <param name="config"></param>
        public static void SaveMessageContentConfig(MessageContent config)
        {
            //using (FileStream fs = new FileStream(WorkDirectory + "\\Data\\MessageContent.xml", FileMode.Create))
            //{
            //    XmlSerializer xs = new XmlSerializer(typeof(MessageContent));
            //    xs.Serialize(fs, config);
            //    Core.Cache.Insert("MessageContent", config);
            //}

            string sDirectory = Mall.Core.Helper.IOHelper.urlToVirtual(WorkDirectory) + "/Data/MessageContent.xml";
            XmlSerializer xml = new XmlSerializer(typeof(MessageContent));
            MemoryStream Stream = new MemoryStream();
            xml.Serialize(Stream, config);

            byte[] b = Stream.ToArray();
            MemoryStream stream2 = new MemoryStream(b);
            Mall.Core.MallIO.CreateFile(sDirectory, stream2, Core.FileCreateType.Create);
        }


        /// <summary>
        /// 保持消息发送状态
        /// </summary>
        /// <param name="config"></param>
        public static void SaveMessageStatus(MessageStatus config)
        {
            string sDirectory = Mall.Core.Helper.IOHelper.urlToVirtual(WorkDirectory) + "/Data/config.xml";
            XmlSerializer xml = new XmlSerializer(typeof(MessageStatus));
            MemoryStream Stream = new MemoryStream();
            xml.Serialize(Stream, config);

            byte[] b = Stream.ToArray();
            MemoryStream stream2 = new MemoryStream(b);
            Mall.Core.MallIO.CreateFile(sDirectory, stream2, Core.FileCreateType.Create);
        }
    }
}
