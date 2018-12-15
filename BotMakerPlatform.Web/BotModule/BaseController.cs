using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using Autofac.Core.Lifetime;
using BotMakerPlatform.Web.Repo;
using Microsoft.AspNet.Identity;
using Telegram.Bot;

namespace BotMakerPlatform.Web.BotModule
{
    public class BaseController : Controller
    {
        protected string UserId { get; private set; }
        protected IEnumerable<SubscriberRecord> Subscribers { get; private set; }
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

            //TODO: Make sure don't leak
            var scope = IocConfig.Container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            var db = scope.Resolve<Db>();
            var botInstance = db.BotInstanceRecords.AsNoTracking().SingleOrDefault(x => x.BotUniqueName == botUniqueName && x.UserId == UserId);

            TelegramClient = scope.Resolve<ITelegramBotClient>(new NamedParameter("token", botInstance.Token));

            BotInstanceId = botInstance.Id;
            Subscribers = botInstance.SubscriberRecords;

            ViewBag.WebhookUrl = Url.Action("WebhookInfo", new { BotId = botInstance.Id });
        }
    }
}