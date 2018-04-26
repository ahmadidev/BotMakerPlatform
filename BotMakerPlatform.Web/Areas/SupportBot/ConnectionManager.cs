using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Linq;

namespace BotMakerPlatform.Web.Areas.SupportBot
{
    public class ConnectionManager
    {
        public int Id { get; set; }

        public ITelegramBotClient TelegramClient { get; set; }

        public IEnumerable<Connection> Connections { get; set; }

        public ConnectionManager(int id, ITelegramBotClient telegramClient)
        {
            this.Id = id;
            this.TelegramClient = telegramClient;

            var connectionRepo = new ConnectionRepo(Id);
            Connections = connectionRepo.GetAll();
        }

        public void Connect(Update update, Subscriber subscriber, Subscriber supporter)
        {
            if (HasCurrentConnection(subscriber.ChatId))
                TelegramClient.SendTextMessageAsync(subscriber.ChatId,
                    "You are already connected. try talking to the supporter. Don't be shy kiddo");
            else
                AddConnection(subscriber, supporter);
        }

        public bool End(Subscriber subscriber)
        {
            Connection connection = FindUserConnection(subscriber.ChatId);

            if (connection == null)
                return false;

            var connectionRepo = new ConnectionRepo(Id);
            connectionRepo.Remove(connection);

            TelegramClient.SendTextMessageAsync(connection.SupporterChatId, "Your Connection has ended with "
                + subscriber.Username);

            TelegramClient.SendTextMessageAsync(connection.UserChatId, "Your Connection has ended");

            return true;
        }

        public void MessageOther(Update update, Subscriber subscriber)
        {
            if (FindUserConnection(subscriber.ChatId) != null)
            {
                long userEndId = FindUserConnectionEndChatId(subscriber);
                TelegramClient.SendTextMessageAsync(userEndId, update.Message.Text);
            }
            else
            {
                TelegramClient.SendTextMessageAsync(subscriber.ChatId, "You have no current session.");
            }
        }

        private void AddConnection(Subscriber subscriber, Subscriber supporter)
        {
            var connectionRepo = new ConnectionRepo(Id);

            connectionRepo.Add(new Connection
            {
                BotInstanceId = Id,
                SupporterChatId = supporter.ChatId,
                UserChatId = subscriber.ChatId
            });

            TelegramClient.SendTextMessageAsync(supporter.ChatId, "You are now connected to user : "
                + subscriber.Username);

            TelegramClient.SendTextMessageAsync(subscriber.ChatId, "You are now connected to supporter");
        }

        private Connection FindUserConnection(long subscriberChatId)
        {
            return Connections.FirstOrDefault(connection => connection.UserChatId == subscriberChatId || connection.SupporterChatId == subscriberChatId);
        }

        private bool HasCurrentConnection(long subscriberChatId)
        {
            foreach (var connection in Connections)
            {
                if (connection.UserChatId == subscriberChatId || connection.SupporterChatId == subscriberChatId)
                    return true;
            }

            return false;
        }

        private long FindUserConnectionEndChatId(Subscriber subscriber)
        {
            foreach (var connection in Connections)
            {
                if (connection.UserChatId == subscriber.ChatId)
                    return connection.SupporterChatId;
                if (connection.SupporterChatId == subscriber.ChatId)
                    return connection.UserChatId;
            }

            return 0;
        }
    }
}