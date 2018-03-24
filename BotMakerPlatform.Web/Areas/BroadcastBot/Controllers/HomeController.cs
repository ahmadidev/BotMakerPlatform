using System.Linq;
using System.Web.Mvc;

namespace BotMakerPlatform.Web.Areas.BroadcastBot.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var subscribers = Subscribers.ToList();

            return View(subscribers);
        }

        [HttpPost]
        public ActionResult SendMessage(string message)
        {
            var subscribers = Subscribers.ToList();

            foreach (var user in subscribers)
                BotClient.SendTextMessageAsync(user.ChatId, message);

            TempData["Message"] = $"Message sent to {subscribers.Count} user(s).";

            return RedirectToAction("Index");
        }
    }
}