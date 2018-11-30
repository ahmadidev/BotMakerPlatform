using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Hosting;
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
        [HttpGet]
        public ActionResult Index(string uniqueName)
        {
            var bot = BotDefinitionRepo.BotDefinitions.Single(x => x.UniqueName == uniqueName);
            //var botInstance = BotInstanceRepo.BotInstanceRecords.FirstOrDefault(x => x.BotUniqueName == uniqueName && x.UserId == User.Identity.GetUserId());
            BotInstanceRecord botInstance;
            using (var db = new Db())
                botInstance = db.BotInstanceRecords.AsNoTracking().SingleOrDefault(x => x.BotUniqueName == uniqueName);

            ViewBag.HasIt = botInstance != null;
            ViewBag.BotInstance = botInstance;

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

            using (var db = new Db())
            {
                var exists = db.BotInstanceRecords.AsNoTracking().Any(x => x.BotUniqueName == botInstance.BotUniqueName);

                if (exists)
                    TempData["Message"] = $"Bot {botInstance.BotUniqueName} Already exists.";
                else
                {
                    db.BotInstanceRecords.Add(botInstance);
                    db.SaveChanges();

                    TempData["Message"] = $"Bot @{result.Username} added successfully.";
                }

                //BotInstanceRepo.BotInstanceRecords.Add(botInstance);
            }

            return RedirectToAction("Index", new { uniquename = uniqueName });
        }
    }
}