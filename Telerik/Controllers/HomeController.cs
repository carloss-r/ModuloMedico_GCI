using System.Web.Mvc;

namespace Telerik.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Inicio";
            return View();
        }
    }
}
