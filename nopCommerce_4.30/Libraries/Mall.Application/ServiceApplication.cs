using Mall.Core;
using Mall.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Http.Extensions;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;


namespace Mall.Application
{
    //TODO:FG 已存在一套依赖注入容器(ObjectContainer)，此处存在 策略多重实现。
    public static class ServiceApplication
    {

        private static IHttpContextAccessor _httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();


        public static T Create<T>() where T : IService
        {

            T t = ServiceProvider.Instance<T>.Create;
            if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.Session != null)
            {
                List<IService> ts = _httpContextAccessor.HttpContext.Session.Get<List<IService>>("_serviceInstace") as List<IService>;
                if (ts == null)
                {
                    ts = new List<IService>() { t };
                }
                else
                    ts.Add(t);
                _httpContextAccessor.HttpContext.Session.Set<List<IService>>("_serviceInstace",ts);
            }
            return t;
        }

        public static void DisposeService(ControllerContext filterContext)
        {
           

            List<IService> services = _httpContextAccessor.HttpContext.Session.Get<List<IService>>("_serviceInstace") as List<IService>;
            if (services != null)
            {
                foreach (var service in services)
                {
                    try
                    {
                        service.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(service.GetType().ToString() + "释放失败！", ex);
                    }
                }
                _httpContextAccessor.HttpContext.Session.Set<List<IService>>("_serviceInstace",null);
            }
        }
    }
}
