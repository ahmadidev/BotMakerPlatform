using System.Linq;
using BotMakerPlatform.Web.Areas.EjooUtilBot.Models;
using BotMakerPlatform.Web.BotModule;
using Microsoft.AspNetCore.Mvc;

namespace BotMakerPlatform.Web.Areas.EjooUtilBot.Controllers
{
    [Area("EjooUtilBot")]
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