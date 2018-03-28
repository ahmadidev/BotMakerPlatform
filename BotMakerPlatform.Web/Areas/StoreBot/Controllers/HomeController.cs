using System.Linq;
using System.Web.Mvc;

namespace BotMakerPlatform.Web.Areas.StoreBot.Controllers
{
    public class HomeController : BaseController
    {

        public ActionResult Index()
        {
            //What an acidi method...
            var storeSubscribers = Subscribers
                .GroupJoin(StoreAdminRepo.StoreAdmins,
                    subscriber => subscriber.ChatId,
                    admin => admin.ChatId,
                    (subscriber, admins) => new StoreSubscriber
                    {
                        ChatId = subscriber.ChatId,
                        Name = $"{subscriber.FirstName} {subscriber.LastName} ({subscriber.Username})",
                        IsAdmin = admins.Any()
                    }
                )
                .ToList();

            return View(storeSubscribers);
        }

        [HttpPost]
        public ActionResult MakeAdmin(long chatId)
        {
            StoreAdminRepo.StoreAdmins.RemoveAll(x => x.BotId == BotId && x.ChatId == chatId);
            StoreAdminRepo.StoreAdmins.Add(new StoreAdmin { BotId = BotId, ChatId = chatId });

            return Redirect(Request.UrlReferrer?.ToString());
        }

        [HttpPost]
        public ActionResult RemoveAdmin(long chatId)
        {
            StoreAdminRepo.StoreAdmins.RemoveAll(x => x.BotId == BotId && x.ChatId == chatId);

            return Redirect(Request.UrlReferrer?.ToString());
        }
    }
}