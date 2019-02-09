using System.Linq;
using BotMakerPlatform.Web.Areas.StoreBot.Models;
using BotMakerPlatform.Web.Areas.StoreBot.Record;
using BotMakerPlatform.Web.Areas.StoreBot.Repo;
using BotMakerPlatform.Web.BotModule;
using BotMakerPlatform.Web.Repo;
using Microsoft.AspNetCore.Mvc;

namespace BotMakerPlatform.Web.Areas.StoreBot.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var storeAdminRepo = new StoreAdminRepo(BotInstanceId, new Db());

            //What an acidi method...
            var storeSubscribers = Subscribers
                .GroupJoin(storeAdminRepo.GetAllAdmins(),
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
            var storeAdminRepo = new StoreAdminRepo(BotInstanceId, new Db());

            storeAdminRepo.AddAdmin(chatId);

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public ActionResult RemoveAdmin(long chatId)
        {
            var storeAdminRepo = new StoreAdminRepo(BotInstanceId, new Db());

            storeAdminRepo.RemoveAdmin(chatId);

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}