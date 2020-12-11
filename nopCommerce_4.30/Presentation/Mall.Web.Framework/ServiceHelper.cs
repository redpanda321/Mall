using Microsoft.AspNetCore.Http;
using Mall.IServices;
using Nop.Core.Infrastructure;

namespace Mall.Web.Framework
{
    public  class ServiceHelper
    {

        private static IHttpContextAccessor _httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();

        public static T Create<T>() where T : IService
        {

          //  T t = EngineContext.Current.Resolve<T>();

            
            
            T t = Mall.ServiceProvider.Instance<T>.Create;
            /*
            if (_httpContextAccessor.HttpContext != null&& _httpContextAccessor.HttpContext.Session!=null)
            {
                List<IService> ts = _httpContextAccessor.HttpContext.Session.Get<List<IService>>("_serviceInstace") ;
                if (ts == null)
                {
                    ts = new List<IService>() { t };
                }
                else
                    ts.Add(t);


                _httpContextAccessor.HttpContext.Session.Set<List<IService>>("_serviceInstace",ts) ;
            }
            */

            return t;
        }       
    }
}
