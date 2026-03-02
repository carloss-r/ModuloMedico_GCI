using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Telerik
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            if (ex == null) return;

            try
            {
                string logPath = Server.MapPath("~/App_Data/error.log");
                string msg = string.Format(
                    "[{0}] URL: {1}\r\nTIPO: {2}\r\nMSG: {3}\r\nINNER: {4}\r\nSTACK:\r\n{5}\r\n{6}\r\n",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Request.Url,
                    ex.GetType().FullName,
                    ex.Message,
                    ex.InnerException != null ? ex.InnerException.Message : "N/A",
                    ex.StackTrace,
                    new string('-', 80)
                );
                File.AppendAllText(logPath, msg);
            }
            catch { /* no podemos fallar aquí */ }
        }
    }
}
