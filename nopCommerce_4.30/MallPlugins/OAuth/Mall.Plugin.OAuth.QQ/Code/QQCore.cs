using Mall.Core;
using System.IO;
using System.Xml.Serialization;

namespace Mall.Plugin.OAuth.QQ
{
    class QQCore
    {
        /// <summary>
        /// 工作目录
        /// </summary>
        public static string WorkDirectory { get; set; }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        public static OAuthQQConfig GetConfig()
        {
            OAuthQQConfig config = new OAuthQQConfig();

            string sDirectory = Mall.Core.Helper.IOHelper.urlToVirtual(WorkDirectory) + "/QQ.config";

            if (MallIO.ExistFile(sDirectory))
            {
                XmlSerializer xs = new XmlSerializer(typeof(OAuthQQConfig));
                byte[] b = Mall.Core.MallIO.GetFileContent(sDirectory);
                string str = System.Text.Encoding.Default.GetString(b);
                MemoryStream fs = new MemoryStream(b);
                config = (OAuthQQConfig)xs.Deserialize(fs);
            }
            else
            {
                SaveConfig(config);
            }

            return config;
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="config"></param>
        public static void SaveConfig(OAuthQQConfig config)
        {
            string sDirectory = Mall.Core.Helper.IOHelper.urlToVirtual(WorkDirectory) + "/QQ.config";
            XmlSerializer xml = new XmlSerializer(typeof(OAuthQQConfig));
            MemoryStream Stream = new MemoryStream();
            xml.Serialize(Stream, config);

            byte[] b = Stream.ToArray();
            MemoryStream stream2 = new MemoryStream(b);
            Mall.Core.MallIO.CreateFile(sDirectory, stream2, Core.FileCreateType.Create);
        }

    }
}
