﻿using System.Linq;
using System.Web.Mvc;

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
            Subscriber subscriber = Subscribers.SingleOrDefault(x => x.ChatId == chatId);
            ConnectionManager.Instance.Supporters.RemoveAll(x => x.BotId == BotId && x.ChatId == chatId);
            ConnectionManager.Instance.Supporters.Add(new Supporter(subscriber.BotId, subscriber.ChatId, subscriber.Username));

            return Redirect(Request.UrlReferrer?.ToString());
        }

        [HttpPost]
        public ActionResult RemoveAdmin(long chatId)
        {
            ConnectionManager.Instance.Supporters.RemoveAll(x => x.BotId == BotId && x.ChatId == chatId);

            return Redirect(Request.UrlReferrer?.ToString());
        }
    }
}