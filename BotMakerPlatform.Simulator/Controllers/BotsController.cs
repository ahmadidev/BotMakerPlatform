﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Simulator.Controllers
{
    public class BotsController : ApiController
    {
        public class TelegramBotInstance
        {
            public string Token { get; set; }
            public string WebhookUrl { get; set; }
            public User User { get; set; }
        }

        public static readonly List<TelegramBotInstance> Bots = new List<TelegramBotInstance>
        {
            new TelegramBotInstance
            {
                Token = "samplebot_1_token",
                User = new User
                {
                    Id = 1,
                    FirstName = "Sample 1",
                    LastName = "Bot",
                    Username = "sample1_bot",
                    IsBot = true
                }
            },
            new TelegramBotInstance
            {
                Token = "samplebot_2_token",
                User =
                    new User
                    {
                        Id = 2,
                        FirstName = "Sample 2",
                        LastName = "Bot",
                        Username = "sample2_bot",
                        IsBot = true
                    }
            }
        };

        public class SetWebhookModel
        {
            public string Token { get; set; }
            public string Url { get; set; }
        }

        [HttpGet]
        public IHttpActionResult GetAll()
        {
            return Ok(Bots);
        }

        [HttpPost]
        public IHttpActionResult SetWebhook(SetWebhookModel model)
        {
            var botInstance = Bots.SingleOrDefault(x => x.Token == model.Token);

            if (botInstance == null)
                return NotFound();

            botInstance.WebhookUrl = model.Url;

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult GetWebhookInfo(string token)
        {
            var botInstance = Bots.SingleOrDefault(x => x.Token == token);

            if (botInstance == null)
                return NotFound();

            var webhookUrl = botInstance?.WebhookUrl;
            return Ok(new WebhookInfo { Url = webhookUrl });
        }

        [HttpGet]
        public IHttpActionResult GetMe(string token)
        {
            var botInstance = Bots.SingleOrDefault(x => x.Token == token);

            if (botInstance == null)
                return NotFound();

            return Ok(botInstance.User);
        }
    }
}
