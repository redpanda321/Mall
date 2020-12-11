using Microsoft.AspNet.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mall.Web
{
    public class TemplateVisualizationViewEngine : IViewEngine
    {
        private Dictionary<ViewEngineResultCacheKey, ViewEngineResult> viewEngineResults = new Dictionary<ViewEngineResultCacheKey, ViewEngineResult>();
        private object syncHelper = new object();


        private IWebHostEnvironment _hostingEnvironment = EngineContext.Current.Resolve<IWebHostEnvironment>();

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return this.FindView(controllerContext, partialViewName, null, useCache);
        }

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            string controllerName = controllerContext.RouteData.GetRequiredString("controller");
            ViewEngineResultCacheKey key = new ViewEngineResultCacheKey(controllerName, viewName);
            ViewEngineResult result;
            if (!useCache)
            {
                result = InternalFindView(controllerContext, viewName, controllerName);
                viewEngineResults[key] = result;
                return result;
            }
            if (viewEngineResults.TryGetValue(key, out result))
            {
                return result;
            }
            lock (syncHelper)
            {
                if (viewEngineResults.TryGetValue(key, out result))
                {
                    return result;
                }

                result = InternalFindView(controllerContext, viewName, controllerName);
                viewEngineResults[key] = result;
                return result;
            }
        }

        private ViewEngineResult InternalFindView(ControllerContext controllerContext, string viewName, string controllerName)
        {
            string[] searchLocations = new string[]
            {
                string.Format( "~/views/{0}/{1}.shtml", controllerName, viewName),
                string.Format( "~/views/Shared/{0}.shtml", viewName)
            };

           // string fileName = controllerContext.HttpContext.Request.MapPath(searchLocations[0]);
            string fileName = _hostingEnvironment.ContentRootPath + searchLocations[0];

            if (File.Exists(fileName))
            {
                return  ViewEngineResult.Found(viewName,new TemplateVisualizationView(fileName));
            }
            fileName = string.Format(@"\views\Shared\{0}.shtml", viewName);
            if (File.Exists(fileName))
            {
                return  ViewEngineResult.Found(viewName,new  TemplateVisualizationView(fileName));
            }
            return  ViewEngineResult.NotFound(viewName,searchLocations);
        }

        public void ReleaseView(ControllerContext controllerContext, IView view)
        { }

        public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage)
        {
            throw new NotImplementedException();
        }

        public ViewEngineResult GetView(string executingFilePath, string viewPath, bool isMainPage)
        {
            throw new NotImplementedException();
        }
    }
}