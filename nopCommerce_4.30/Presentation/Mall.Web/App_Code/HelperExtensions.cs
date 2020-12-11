using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNet.Mvc
{
    public static class HelperExtensions
    {
        public static string GetRequiredString(this RouteData routeData, string keyName)
        {
            object value;
            if (!routeData.Values.TryGetValue(keyName, out value))
            {
                throw new InvalidOperationException($"Could not find key with name '{keyName}'");
            }

            return value?.ToString();
        }
    }
}
