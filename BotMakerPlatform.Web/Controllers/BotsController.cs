﻿using System;
using System.Linq;
using Autofac;
using Autofac.Core.Lifetime;
using BotMakerPlatform.Web.CriticalDtos;
using BotMakerPlatform.Web.Repo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace BotMakerPlatform.Web.Controllers
{
    public class BotsController : Controller
    {
        private Db Db { get; }
        private BotDefinitionRepo BotDefinitionRepo { get; }

        private ILogger<BotsController> Logger { get; }

        public BotsController(Db db, BotDefinitionRepo botDefinitionRepo, ILogger<BotsController> logger)
        {
            Db = db;
            BotDefinitionRepo = botDefinitionRepo;
            Logger = logger;
        }

        [HttpGet]
        public ActionResult Index(string uniqueName)
        {
            var bot = BotDefinitionRepo.GetByUniqueName(uniqueName);
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

            Logger.LogInformation("Adding Webhook for bot {BotUniqueName} ({BotInstanceId})", botInstance.BotUniqueName, botInstance.Id);

            // var webhookUrl = $"{Request.Scheme}://{Request.Host.Value}/{Request.ApplicationPath?.Trim('/')}/Webhook/Update/?{nameof(WebhookUpdateDto.BotInstanceId)}={botInstance.Id}&{nameof(WebhookUpdateDto.Secret)}={botInstance.WebhookSecret}";

            var webhookUrl = Url.Action("Update", "Webhook", new WebhookUpdateDto { BotInstanceId = botInstance.Id, Secret = botInstance.WebhookSecret }, Request.Scheme);

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