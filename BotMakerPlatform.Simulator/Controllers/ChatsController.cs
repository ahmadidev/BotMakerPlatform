using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using BotMakerPlatform.Web;
using BotMakerPlatform.Web.Models;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Simulator.Controllers
{
    public class ChatModel
    {
        public long ChatId { get; set; }

        public User Sender { get; set; }

        public UserBot BotInstance { get; set; }
    }

    public class ChatsController : ApiController
    {
        public static readonly List<ChatModel> Chats = new List<ChatModel>();

        public IHttpActionResult New(User sender, [FromBody]int botId)
        {
            var chat = new ChatModel
            {
                ChatId = new Random().Next(1000000, 9999999),
                Sender = sender,
                BotInstance = UserBotRepo.UserBots.Single(x => x.BotId == botId)
            };
            Chats.Add(chat);

            return Ok();
        }


        public class SendMessageModel
        {
            public int BotId { get; set; }
            public Update Update { get; set; }
        }

        public IHttpActionResult SendMessage(SendMessageModel model)
        {
            var webhookUrl = BotsController.Bots.Single(x => x.User.Id == model.BotId).WebhookUrl;

            using (var httpClient = new HttpClient())
            {
                var result = httpClient.PostAsJsonAsync(webhookUrl, model.Update).Result;

                if (!result.IsSuccessStatusCode)
                    throw new InvalidOperationException("Send message to webhook failed.");

                return Ok();
            }
        }
    }
}
