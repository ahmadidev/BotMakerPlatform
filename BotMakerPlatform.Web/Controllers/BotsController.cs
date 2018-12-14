using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using Autofac.Core.Lifetime;
using BotMakerPlatform.Web.CriticalDtos;
using BotMakerPlatform.Web.Repo;
using Microsoft.AspNet.Identity;
using Telegram.Bot;

namespace BotMakerPlatform.Web.Controllers
{
    public class BotsController : Controller
    {
        private Db Db { get; }

        public BotsController(Db db)
        {
            Db = db;
        }

        [HttpGet]
        public ActionResult Index(string uniqueName)
        {
            var bot = BotDefinitionRepo.BotDefinitions.Single(x => x.UniqueName == uniqueName);
            var userId = User.Identity.GetUserId();
            var botInstance = Db.BotInstanceRecords.AsNoTracking().SingleOrDefault(x => x.BotUniqueName == uniqueName && x.UserId == userId);

            ViewBag.HasIt = botInstance != null;
            ViewBag.BotInstance = botInstance;
            ViewBag.SubscribersCount = botInstance != null ? new SubscriberRepo(botInstance.Id, Db).GetAll().Count() : 0;

            return View(bot);
        }

        [HttpPost]
        public ActionResult AddBotInstance(string token, string uniqueName)
        {
            ITelegramBotClient telegramClient;

            //TODO: Make sure don't leak
            using (var scope = IocConfig.Container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
                telegramClient = scope.Resolve<ITelegramBotClient>(new NamedParameter("token", token));

            var result = telegramClient.GetMeAsync().Result;
            var userId = User.Identity.GetUserId();

            var botInstance = new BotInstanceRecord
            {
                Id = new Random().Next(1000, 9999),
                BotUsername = result.Username,
                BotUniqueName = uniqueName,
                UserId = userId,
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

            var existingBotInstanceRecord = Db.BotInstanceRecords.SingleOrDefault(x => x.BotUniqueName == botInstance.BotUniqueName && x.UserId == userId);

            if (existingBotInstanceRecord != null)
                Db.BotInstanceRecords.Remove(existingBotInstanceRecord);

            Db.BotInstanceRecords.Add(botInstance);
            Db.SaveChanges();

            TempData["Message"] = $"Bot @{result.Username} added successfully.";

            return RedirectToAction("Index", new { uniquename = uniqueName });
        }
    }
}