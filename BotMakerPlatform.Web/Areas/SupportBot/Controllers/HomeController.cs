using System.Linq;
using System.Web.Mvc;
using BotMakerPlatform.Web.BotModule;

namespace BotMakerPlatform.Web.Areas.SupportBot.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var subscribers = Subscribers.ToList();

            return View(subscribers);
        }


        [HttpPost]
        public ActionResult MakeAdmin(long chatId)
        {
            var subscriber = Subscribers.SingleOrDefault(x => x.BotInstanceId == BotInstanceId && x.ChatId == chatId);

            ConnectionManager.Instance.Supporters.RemoveAll(x => x.BotId == BotInstanceId && x.ChatId == chatId);

            if (subscriber != null)
                ConnectionManager.Instance.Supporters.Add(new Supporter(subscriber.BotInstanceId, subscriber.ChatId, subscriber.Username));

            return Redirect(Request.UrlReferrer?.ToString());
        }

        [HttpPost]
        public ActionResult RemoveAdmin(long chatId)
        {
            ConnectionManager.Instance.Supporters.RemoveAll(x => x.BotId == BotInstanceId && x.ChatId == chatId);

            return Redirect(Request.UrlReferrer?.ToString());
        }
    }
}