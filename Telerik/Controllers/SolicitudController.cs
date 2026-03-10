using System;
using Telerik.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Telerik.Controllers
{
    public class SolicitudController : Controller
    {
        // GET: Solicitud
        public ActionResult Index()
        {
            return View("~/Views/Solicitud/DashboardRecursosHumanos.cshtml");
        }
    }
}
