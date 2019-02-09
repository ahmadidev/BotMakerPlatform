using System.Linq;
using BotMakerPlatform.Web.Areas.EjooUtilBot.Models;
using BotMakerPlatform.Web.BotModule;
using Microsoft.AspNetCore.Mvc;

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