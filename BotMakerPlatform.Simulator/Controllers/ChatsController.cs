using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using BotMakerPlatform.Web.Controllers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotMakerPlatform.Simulator.Controllers
{
    public class ChatModel
    {
        public ChatModel()
        {
            Messages = new List<string>();
        }

        public long ChatId { get; set; }

        public User Sender { get; set; }

        public int BotId { get; set; }

        public List<string> Messages { get; set; }
    }

    public class ChatsController : ApiController
    {
        public static readonly List<ChatModel> Chats = new List<ChatModel>();

        public IHttpActionResult GetAll()
        {
            return Ok(Chats);
        }

        public class SendMessageModel
        {
            public int BotId { get; set; }
            public Update Update { get; set; }
        }
        [HttpPost]
        public IHttpActionResult SendMessage(SendMessageModel model)
        {
            if(model.Update.Type != UpdateType.MessageUpdate)
                throw new InvalidOperationException("Just MessageUpdate is supported.");

            var botInstance = BotsController.Bots.Single(x => x.User.Id == model.BotId);
            var webhookUrl = botInstance.WebhookUrl;

            using (var httpClient = new HttpClient())
            {
                var chatModel = Chats.SingleOrDefault(x => x.ChatId == model.Update.Message.Chat.Id);
                if (chatModel == null)
                {
                    chatModel = new ChatModel
                    {
                        ChatId = model.Update.Message.Chat.Id,
                        BotId = botInstance.User.Id,
                        Sender = model.Update.Message.From
                    };
                    Chats.Add(chatModel);
                }

                var result = httpClient.PostAsJsonAsync(webhookUrl, model.Update).Result;

                if (!result.IsSuccessStatusCode)
                    throw new InvalidOperationException("Send message to webhook failed.");

                return Ok();
            }
        }

        public class ReceiveMessageModel
        {
            public long ChatId { get; set; }

            public string Text { get; set; }

            public string Token { get; set; }
        }
        [HttpPost]
        public IHttpActionResult ReceiveMessage(ReceiveMessageModel model)
        {
            var botInstance = BotsController.Bots.Single(x => x.Token == model.Token);

            if (botInstance == null)
                return BadRequest("Invalid token.");

            var chatModel = Chats.SingleOrDefault(x => x.ChatId == model.ChatId);
            if (chatModel == null)
                return BadRequest("There is no such chat id.");

            chatModel.Messages.Add(model.Text);

            HomeController.LogRecords.Add($"Received message for {model.ChatId}: {model.Text}");

            return Ok();
        }
    }
}
