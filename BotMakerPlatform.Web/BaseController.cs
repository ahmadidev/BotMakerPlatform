using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using Microsoft.AspNet.Identity;
using Telegram.Bot;

namespace BotMakerPlatform.Web
{
    public class BaseController : Controller
    {
        protected string UserId { get; private set; }
        protected IEnumerable<Subscriber> Subscribers { get; private set; }
        protected ITelegramBotClient BotClient { get; private set; }
        protected int BotId { get; private set; }

        public ActionResult WebhookInfo(string botId)
        {
            var webhookInfo = BotClient.GetWebhookInfoAsync().Result;

            return Json(webhookInfo, JsonRequestBehavior.AllowGet);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var botUniqueName = filterContext.RouteData.DataTokens["area"].ToString();
            UserId = User.Identity.GetUserId();
            var userBot = UserBotRepo.UserBots.Single(x => x.BotUniqueName == botUniqueName && x.UserId == UserId);

            //TODO: Make sure don't leack
            using (var scope = IocConfig.Container.BeginLifetimeScope())
                BotClient = scope.Resolve<ITelegramBotClient>(new NamedParameter("token", userBot.Token));

            Subscribers = SubscriberRepo.Subscribers.Where(x => x.BotId == userBot.BotId);

            ViewBag.WebhookUrl = Url.Action("WebhookInfo", new { userBot.BotId });
        }
    }
}