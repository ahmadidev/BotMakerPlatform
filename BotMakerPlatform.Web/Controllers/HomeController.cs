using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace BotMakerPlatform.Web.Controllers
{
    public class HomeController : Controller
    {
        public static readonly List<string> LogRecords = new List<string>(1000);

        [HttpGet]
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
                return View("Home", BotDefinitionRepo.BotDefinitions.ToList());

            return View("Landing");
        }

        public ActionResult Logs()
        {
            return Json(LogRecords, JsonRequestBehavior.AllowGet);
        }
    }
}