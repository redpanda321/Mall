
using Mall.Web.Framework;

namespace Mall.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new GZipAttribute());
            filters.Add(new HandleErrorAttribute());
           
        }
    }


}
