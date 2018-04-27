using System.Linq;
using System.Web.Mvc;
using BotMakerPlatform.Web.Areas.SupportBot.Models;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using BotMakerPlatform.Web.BotModule;

namespace BotMakerPlatform.Web.Areas.SupportBot.Controllers
{
    public class HomeController : BaseController
    { 
        public ActionResult Index()
        {
            var supporters = new SupporterRepo(BotInstanceId).GetAll();
            var subscribers = Subscribers
                .GroupJoin(supporters,
                    subscriber => subscriber.ChatId,
                    supporter => supporter.ChatId,
                    (subscriber, thisSupporters) => new SubscriberViewModel
                    {
                        ChatId = subscriber.ChatId,
                        Username = subscriber.Username,
                        FirstName = subscriber.FirstName,
                        LastName = subscriber.LastName,
                        IsSupporter = thisSupporters.Any()
                    }
                )
                .ToList();

            return View(subscribers);
        }

        [HttpPost]
        public ActionResult MakeSupporter(long chatId)
        {
            var supporterRepo = new SupporterRepo(BotInstanceId);
            supporterRepo.Add(chatId);

            //TODO: What's the correct way?
            //ConnectionNotifier.CustomerDisconnected();

            return Redirect(Request.UrlReferrer?.ToString());
        }

        [HttpPost]
        public ActionResult RemoveSupporter(long chatId)
        {
            var supporterRepo = new SupporterRepo(BotInstanceId);
            supporterRepo.Remove(chatId);

            return Redirect(Request.UrlReferrer?.ToString());
        }
    }
}