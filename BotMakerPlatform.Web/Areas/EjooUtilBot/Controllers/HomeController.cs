using System.Linq;
using System.Web.Mvc;
using BotMakerPlatform.Web.Areas.EjooUtilBot.Models;
using BotMakerPlatform.Web.BotModule;

namespace BotMakerPlatform.Web.Areas.EjooUtilBot.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View(new HomeViewModel
            {
                Subscribers = Subscribers.ToList()
            });
        }
    }
}