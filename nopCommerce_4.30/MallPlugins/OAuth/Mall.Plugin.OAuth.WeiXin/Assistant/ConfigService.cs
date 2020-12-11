using System.IO;
using System.Xml.Serialization;

namespace Mall.Plugin.OAuth.WeiXin.Assistant
{
    public class ConfigService<T> where T : class
    {

        public ConfigService()
        {
        }

        public static T GetConfig(string filename)
        {
            T config;
            string sDirectory = Mall.Core.Helper.IOHelper.urlToVirtual(filename);

            XmlSerializer xs = new XmlSerializer(typeof(T));
            byte[] b = Mall.Core.MallIO.GetFileContent(sDirectory);
            string str = System.Text.Encoding.Default.GetString(b);
            MemoryStream fs = new MemoryStream(b);
            config = (T)xs.Deserialize(fs);

            return config;
        }

        public static void SaveConfig(T config, string filename)
        {
            string sDirectory = Mall.Core.Helper.IOHelper.urlToVirtual(filename);
            XmlSerializer xml = new XmlSerializer(typeof(T));
            MemoryStream Stream = new MemoryStream();
            xml.Serialize(Stream, config);

            byte[] b = Stream.ToArray();
            MemoryStream stream2 = new MemoryStream(b);
            Mall.Core.MallIO.CreateFile(sDirectory, stream2, Core.FileCreateType.Create);
        }
    }
}
