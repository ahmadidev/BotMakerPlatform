using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotMakerPlatform.Simulator.Controllers
{
    public class ChatModel
    {
        public ChatModel()
        {
            Messages = new List<ChatMessage>();
        }

        public enum MessageDirection
        {
            Send,
            Received
        }
        public class ChatMessage
        {

            public string Text { get; set; }

            public MessageDirection Direction { get; set; }
        }

        public long ChatId { get; set; }

        public User Sender { get; set; }

        public int BotId { get; set; }

        public string BotUsername { get; set; }

        public List<ChatMessage> Messages { get; set; }
    }

    public class ChatsController : ApiController
    {
        public static readonly List<ChatModel> Chats = new List<ChatModel>();
        private static readonly Random Random = new Random();

        public IHttpActionResult GetAll()
        {
            return Ok(Chats);
        }

        public class NewChatModel
        {
            public int BotId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Username { get; set; }
        }
        public IHttpActionResult New(NewChatModel model)
        {
            Chats.Add(new ChatModel
            {
                ChatId = Random.Next(1000000, 9999999),
                BotId = model.BotId,
                BotUsername = BotsController.Bots.Single(x => x.User.Id == model.BotId).User.Username,
                Sender = new User
                {
                    Id = Random.Next(1000000, 9999999),
                    IsBot = false,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Username = model.Username
                }
            });

            return Ok();
        }

        public class DeleteChatModel
        {
            public long ChatId { get; set; }
        }
        [HttpPost]
        public IHttpActionResult Delete(DeleteChatModel model)
        {
            Chats.RemoveAll(x => x.ChatId == model.ChatId);

            return Ok();
        }

        public class SendMessageModel
        {
            public long ChatId { get; set; }
            public string Text { get; set; }
        }
        [HttpPost]
        public IHttpActionResult SendMessage(SendMessageModel model)
        {
            var chat = Chats.Single(x => x.ChatId == model.ChatId);
            var botInstance = BotsController.Bots.Single(x => x.User.Id == chat.BotId);

            if (string.IsNullOrEmpty(botInstance.WebhookUrl))
                throw new InvalidOperationException($"Webhook has not been set for bot {botInstance.User.Id}");

            var webhookUrl = botInstance.WebhookUrl;

            using (var httpClient = new HttpClient())
            {
                chat.Messages.Add(new ChatModel.ChatMessage
                {
                    Direction = ChatModel.MessageDirection.Send,
                    Text = model.Text
                });

                var update = new Update
                {
                    Message = new Message
                    {
                        Date = DateTime.Now,
                        Chat = new Chat
                        {
                            Id = chat.ChatId,
                            FirstName = chat.Sender.FirstName,
                            LastName = chat.Sender.LastName,
                            Username = chat.Sender.Username,
                            Type = ChatType.Private
                        },
                        MessageId = (int)DateTime.Now.Ticks,
                        From = chat.Sender,
                        Text = model.Text
                    }
                };
                var result = httpClient.PostAsJsonAsync(webhookUrl, update).Result;

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

            chatModel.Messages.Add(new ChatModel.ChatMessage
            {
                Direction = ChatModel.MessageDirection.Received,
                Text = model.Text
            });

            return Ok();
        }
    }
}
