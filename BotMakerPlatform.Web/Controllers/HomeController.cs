using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using BotMakerPlatform.Web.Repo;

namespace BotMakerPlatform.Web.Controllers
{
    public class HomeController : Controller
    {
        private BotDefinitionRepo BotDefinitionRepo { get; }
        public static readonly List<string> LogRecords = new List<string>(1000);

        public HomeController(BotDefinitionRepo botDefinitionRepo)
        {
            BotDefinitionRepo = botDefinitionRepo;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(BotDefinitionRepo.GetAll().ToList());
        }

        public ActionResult Logs()
        {
            if (Request.QueryString[null]?.StartsWith("c") ?? false)
                LogRecords.Clear();

            return Json(LogRecords.AsEnumerable().Reverse(), JsonRequestBehavior.AllowGet);
        }
    }
}