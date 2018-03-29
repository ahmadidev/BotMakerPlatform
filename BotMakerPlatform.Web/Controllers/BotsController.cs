using System;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using BotMakerPlatform.Web.Models;
using Microsoft.AspNet.Identity;
using Telegram.Bot;

namespace BotMakerPlatform.Web.Controllers
{
    public class BotsController : Controller
    {
        [HttpGet]
        public ActionResult Index(string uniqueName)
        {
            var bot = BotRepo.Bots.Single(x => x.UniqueName == uniqueName);
            var userBot = UserBotRepo.UserBots.FirstOrDefault(x => x.BotUniqueName == uniqueName && x.UserId == User.Identity.GetUserId());
            ViewBag.HasIt = userBot != null;
            ViewBag.UserBot = userBot;

            return View(bot);
        }

        [HttpPost]
        public ActionResult AddBot(string token, string uniqueName)
        {
            ITelegramBotClient botClient;

            //TODO: Make sure don't leack
            using (var scope = IocConfig.Container.BeginLifetimeScope())
                botClient = scope.Resolve<ITelegramBotClient>(new NamedParameter("token", token));

            var result = botClient.GetMeAsync().Result;

            var botDto = new UserBot
            {
                BotId = new Random().Next(1000, 9999),
                BotUsername = result.Username,
                BotUniqueName = uniqueName,
                UserId = User.Identity.GetUserId(),
                Token = token,
                WebhookSecret = Guid.NewGuid().ToString("N")
            };

            HomeController.LogRecords.Add($"Adding Webhook for bot {uniqueName} ({botDto.BotId})");

            var webhookUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}{Request.ApplicationPath}Webhook/Update/?botId={botDto.BotId}&secret={botDto.WebhookSecret}";
            botClient.SetWebhookAsync(webhookUrl).Wait();
            var botInfoInquiry = botClient.GetWebhookInfoAsync().Result;

            if (!Configuration.IsDebug)
                if (botInfoInquiry.Url != webhookUrl)
                    throw new InvalidOperationException("Webhook failed to set. Seted webhook is not equal to asked one.");

            UserBotRepo.UserBots.Add(botDto);

            TempData["Message"] = $"Bot @{result.Username} added successfully.";

            return RedirectToAction("Index", new { uniquename = uniqueName });
        }
    }
}