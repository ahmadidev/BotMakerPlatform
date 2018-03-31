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
            return View("Home", BotDefinitionRepo.BotDefinitions.ToList());
        }

        public ActionResult Logs()
        {
            return Json(LogRecords.AsEnumerable().Reverse(), JsonRequestBehavior.AllowGet);
        }
    }
}