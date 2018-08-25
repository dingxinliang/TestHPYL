using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web
{
    public class TemplateVisualizationViewEngine : IViewEngine
    {
        private object syncHelper = new object();
        private Dictionary<ViewEngineResultCacheKey, ViewEngineResult> viewEngineResults = new Dictionary<ViewEngineResultCacheKey, ViewEngineResult>();

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return this.FindView(controllerContext, partialViewName, null, useCache);
        }

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            ViewEngineResult result;
            string requiredString = controllerContext.RouteData.GetRequiredString("controller");
            ViewEngineResultCacheKey key = new ViewEngineResultCacheKey(requiredString, viewName);
            if (!useCache)
            {
                result = this.InternalFindView(controllerContext, viewName, requiredString);
                this.viewEngineResults[key] = result;
                return result;
            }
            if (this.viewEngineResults.TryGetValue(key, out result))
            {
                return result;
            }
            lock (this.syncHelper)
            {
                if (!this.viewEngineResults.TryGetValue(key, out result))
                {
                    result = this.InternalFindView(controllerContext, viewName, requiredString);
                    this.viewEngineResults[key] = result;
                }
                return result;
            }
        }

        private ViewEngineResult InternalFindView(ControllerContext controllerContext, string viewName, string controllerName)
        {
            string[] searchedLocations = new string[] { string.Format("~/views/{0}/{1}.shtml", controllerName, viewName), string.Format("~/views/Shared/{0}.shtml", viewName) };
            string path = controllerContext.HttpContext.Request.MapPath(searchedLocations[0]);
            if (File.Exists(path))
            {
                return new ViewEngineResult(new TemplateVisualizationView(path), this);
            }
            path = string.Format(@"\views\Shared\{0}.shtml", viewName);
            if (File.Exists(path))
            {
                return new ViewEngineResult(new TemplateVisualizationView(path), this);
            }
            return new ViewEngineResult(searchedLocations);
        }

        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
        }
    }
}