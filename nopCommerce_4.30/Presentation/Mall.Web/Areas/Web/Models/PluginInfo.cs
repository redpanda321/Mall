namespace Mall.Web.Areas.Web.Models
{
    public class PluginsInfo
    {
        public string ShortName { get; set; }
        public string PluginId { get; set; }

        public bool Enable { get; set; }

        public bool IsBind { set; get; }

        public bool IsSettingsValid { set; get; }
    }
}