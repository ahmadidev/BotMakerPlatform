using System;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using BotMakerPlatform.Web.CriticalDtos;
using BotMakerPlatform.Web.Repo;
using Microsoft.AspNet.Identity;
using Telegram.Bot;

namespace BotMakerPlatform.Web.Controllers
{
    public class BotsController : Controller
    {
        [HttpGet]
        public ActionResult Index(string uniqueName)
        {
            var bot = BotDefinitionRepo.BotDefinitions.Single(x => x.UniqueName == uniqueName);
            var botInstance = BotInstanceRepo.BotInstanceRecords.FirstOrDefault(x => x.BotUniqueName == uniqueName && x.UserId == User.Identity.GetUserId());

            ViewBag.HasIt = botInstance != null;
            ViewBag.BotInstance = botInstance;

            return View(bot);
        }

        [HttpPost]
        public ActionResult AddBotInstance(string token, string uniqueName)
        {
            ITelegramBotClient telegramClient;

            //TODO: Make sure don't leak
            using (var scope = IocConfig.Container.BeginLifetimeScope())
                telegramClient = scope.Resolve<ITelegramBotClient>(new NamedParameter("token", token));

            var result = telegramClient.GetMeAsync().Result;

            var botInstance = new BotInstanceRecord
            {
                Id = new Random().Next(1000, 9999),
                BotUsername = result.Username,
                BotUniqueName = uniqueName,
                UserId = User.Identity.GetUserId(),
                Token = token,
                WebhookSecret = Guid.NewGuid().ToString("N")
            };

            HomeController.LogRecords.Add($"Adding Webhook for bot {uniqueName} ({botInstance.Id})");

            var webhookUrl = $"{Request.Url.Scheme}://{Request.Url.Authority.TrimEnd('/')}/{Request.ApplicationPath?.Trim('/')}/Webhook/Update/?{nameof(WebhookUpdateDto.BotInstanceId)}={botInstance.Id}&{nameof(WebhookUpdateDto.Secret)}={botInstance.WebhookSecret}";
            telegramClient.SetWebhookAsync(webhookUrl).Wait();
            var botInfoInquiry = telegramClient.GetWebhookInfoAsync().Result;

            if (!Configuration.IsDebug)
                if (botInfoInquiry.Url != webhookUrl)
                    throw new InvalidOperationException("Webhook failed to set. Setted webhook is not equal to asked one.");

            BotInstanceRepo.BotInstanceRecords.Add(botInstance);

            ////Id = new Random().Next(1000, 9999)
            //BotInstanceRepo.BotInstanceRecords.Add(
            //    result.Username,
            //    uniqueName,
            //    User.Identity.GetUserId(),
            //    token,
            //    Guid.NewGuid().ToString("N"));

            TempData["Message"] = $"Bot @{result.Username} added successfully.";

            return RedirectToAction("Index", new { uniquename = uniqueName });
        }
    }
}