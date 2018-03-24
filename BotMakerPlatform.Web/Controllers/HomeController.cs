using System;
using System.Web.Mvc;
using BotMakerPlatform.Web.Models;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;

namespace BotMakerPlatform.Web.Controllers
{
    public class HomeController : Controller
    {
        public static readonly List<HomeBotDto> Bots = new List<HomeBotDto>();
        public static readonly List<BotUserDto> Users = new List<BotUserDto>();
        public static readonly List<string> LogRecords = new List<string>(1000);

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View("Home", new HomeViewModel { Bots = Bots });
            }

            return View("Landing");
        }

        [HttpPost]
        public ActionResult AddBot(string token)
        {
            var botClient = new TelegramBotClient(token);
            var result = botClient.GetMeAsync().Result;

            var botDto = new HomeBotDto
            {
                Id = result.Username,
                Name = $"{result.FirstName} {result.LastName}",
                Token = token,
                WebhookSecret = Guid.NewGuid().ToString("N")
            };

            var webhookUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}{Request.ApplicationPath}/Webhook/Update/?botid={botDto.Id}&secret={botDto.WebhookSecret}";
            botClient.SetWebhookAsync(webhookUrl);

            if (botClient.GetWebhookInfoAsync().Result.Url != webhookUrl)
                throw new InvalidOperationException("Webhook failed to set. Seted webhook is not equal to asked one.");

            Bots.Add(botDto);

            TempData["Message"] = $"Bot @{result.Username} added successfully.";

            return RedirectToAction("Index");
        }

        public ActionResult WebhookInfo(string botId)
        {
            var webhookInfo = new TelegramBotClient(Bots.Single(x => x.Id == botId).Token).GetWebhookInfoAsync().Result;

            return Json(webhookInfo, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Logs()
        {
            return Json(LogRecords, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SendMessage(string botId, string message)
        {
            var token = Bots.First(x => x.Id == botId).Token;

            var botClient = new TelegramBotClient(token);

            foreach (var user in Users)
            {
                botClient.SendTextMessageAsync(user.ChatId, message);
            }

            TempData["Message"] = $"Message sent to {Users.Count} user(s).";

            return Redirect(Request.UrlReferrer?.ToString() ?? "/");
        }
    }
}