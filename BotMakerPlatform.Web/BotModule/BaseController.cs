using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using BotMakerPlatform.Web.Repo;
using Microsoft.AspNet.Identity;
using Telegram.Bot;

namespace BotMakerPlatform.Web.BotModule
{
    public class BaseController : Controller
    {
        protected string UserId { get; private set; }
        protected IEnumerable<Subscriber> Subscribers { get; private set; }
        protected ITelegramBotClient TelegramClient { get; private set; }
        protected int BotInstanceId { get; private set; }

        public ActionResult WebhookInfo(string botId)
        {
            var webhookInfo = TelegramClient.GetWebhookInfoAsync().Result;

            return Json(webhookInfo, JsonRequestBehavior.AllowGet);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var botUniqueName = filterContext.RouteData.DataTokens["area"].ToString();
            UserId = User.Identity.GetUserId();
            var botInstance = BotInstanceRepo.BotInstanceRecords.Single(x => x.BotUniqueName == botUniqueName && x.UserId == UserId);

            //TODO: Make sure don't leak
            using (var scope = IocConfig.Container.BeginLifetimeScope())
                TelegramClient = scope.Resolve<ITelegramBotClient>(new NamedParameter("token", botInstance.Token));

            BotInstanceId = botInstance.Id;
            Subscribers = new SubscriberRepo(BotInstanceId).GetAll();

            ViewBag.WebhookUrl = Url.Action("WebhookInfo", new { BotId = botInstance.Id });
        }
    }
}