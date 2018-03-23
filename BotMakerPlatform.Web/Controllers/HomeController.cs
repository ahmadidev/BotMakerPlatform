using System.Web.Mvc;

namespace BotMakerPlatform.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var stats = 0;
                return PartialView("Home", stats);
            }

            return View("Landing");
        }
    }
}