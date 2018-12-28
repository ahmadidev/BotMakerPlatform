using System.Linq;
using System.Web.Mvc;
using BotMakerPlatform.Web.Areas.StoreBot.Models;
using BotMakerPlatform.Web.BotModule;
using BotMakerPlatform.Web.Repo;

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

        [HttpGet]
        public ActionResult Settings()
        {
            var settingRepo = new SettingRepo(BotInstanceId, new Db());
            var model = settingRepo.Load<Setting>();

            return View(model);
        }

        [HttpPost]
        public ActionResult Settings(Setting setting)
        {
            var settingRepo = new SettingRepo(BotInstanceId, new Db());
            settingRepo.Save(setting);

            TempData["Message"] = "تنظیمات جدید با موفقیت ثبت شد";

            return RedirectToAction("Settings");
        }

        [HttpPost]
        public ActionResult MakeAdmin(long chatId)
        {
            StoreAdminRepo.StoreAdmins.RemoveAll(x => x.BotId == BotInstanceId && x.ChatId == chatId);
            StoreAdminRepo.StoreAdmins.Add(new StoreAdmin { BotId = BotInstanceId, ChatId = chatId });

            return Redirect(Request.UrlReferrer?.ToString());
        }

        [HttpPost]
        public ActionResult RemoveAdmin(long chatId)
        {
            StoreAdminRepo.StoreAdmins.RemoveAll(x => x.BotId == BotInstanceId && x.ChatId == chatId);

            return Redirect(Request.UrlReferrer?.ToString());
        }
    }
}