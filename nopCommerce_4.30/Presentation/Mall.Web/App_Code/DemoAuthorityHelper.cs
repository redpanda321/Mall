using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Mall.Web
{
    public class DemoAuthorityHelper
    {
        public static bool IsDemo()
        {
            var isDemo = ConfigurationManager.AppSettings["IsDemo"];
            if (isDemo != null && isDemo == "true")
            {
                return true;
            }
            return false;
        }
    }
}