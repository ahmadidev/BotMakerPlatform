using System.Linq;
using BotMakerPlatform.Web.Repo;
using Microsoft.AspNetCore.Mvc;

namespace BotMakerPlatform.Web.Controllers
{
    public class HomeController : Controller
    {
        private BotDefinitionRepo BotDefinitionRepo { get; }

        public HomeController(BotDefinitionRepo botDefinitionRepo)
        {
            BotDefinitionRepo = botDefinitionRepo;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(BotDefinitionRepo.GetAll().ToList());
        }
    }
}