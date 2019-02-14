using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core.Lifetime;
using BotMakerPlatform.Web.Repo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
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

            return Json(webhookInfo);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var botUniqueName = filterContext.RouteData.Values["area"].ToString();
            UserId = User.Identity.GetUserId();

            //TODO: Make sure don't leak
            var scope = IocConfig.Container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            var db = scope.Resolve<Db>();
            var botInstance = db.BotInstanceRecords
                .AsNoTracking()
                .Include(x => x.SubscriberRecords)
                .SingleOrDefault(x => x.BotUniqueName == botUniqueName && x.UserId == UserId);

            TelegramClient = scope.Resolve<ITelegramBotClient>(new NamedParameter("token", botInstance.Token));

            BotInstanceId = botInstance.Id;
            Subscribers = botInstance.SubscriberRecords;

            ViewBag.WebhookUrl = Url.Action("WebhookInfo", new { BotId = botInstance.Id });
        }
    }
}