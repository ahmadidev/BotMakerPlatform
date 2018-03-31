using System.Collections.Generic;
using System.Linq;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotMakerPlatform.Web.Areas.SupportBot
{
    public class SupportBotInstance : IBotInstance
    {
        public int Id { get; set; }
        public ITelegramBotClient TelegramClient { get; set; }
        public IEnumerable<Subscriber> Subscribers { get; set; }

        public IEnumerable<Subscriber> Supporters { get; set; }

        public IEnumerable<Connection> Connections { get; set; }

        public void Update(Update update, Subscriber subscriber)
        {
            if (update.Type != UpdateType.MessageUpdate)
                return;

            var supporterRepo = new SupporterRepo(Id);
            Supporters = supporterRepo.GetAll();

            var connectionRepo = new ConnectionRepo(Id);
            Connections = connectionRepo.GetAll();

            Web.Controllers.HomeController.LogRecords.Add(subscriber.Username + " : " + update.Message.Text);

            if (IsSupporter(subscriber))
                HandleSupporterMessage(update, subscriber);
            else
                HandleUserMessage(update, subscriber);
        }

        private Subscriber SelectSupporter()
        {
            if (!Supporters.Any())
                return null;

            Subscriber minWaiterCountSupporter = Supporters.First();

            return minWaiterCountSupporter;
        }

        public bool IsSupporter(Subscriber subscriber)
        {
            return Supporters.Any(supporter => supporter.ChatId == subscriber.ChatId);
        }

        private void HandleUserMessage(Update update, Subscriber subscriber)
        {
            if (update.Message.Text == "/connect")
            {
                QueueConnection(update, subscriber);
            }
            else if (update.Message.Text == "/end")
            {
                if (!EndConnection(subscriber))
                {
                    //if (IsWaiting(subscriber))
                    //    RemoveWaiter(subscriber);
                    //else
                    TelegramClient.SendTextMessageAsync(subscriber.ChatId, "There is no connection to end");
                }
            }
            else
            {
                MessageOtherEnd(update, subscriber);
            }
        }

        private void HandleSupporterMessage(Update update, Subscriber supporter)
        {
            MessageOtherEnd(update, supporter);
        }

        private void MessageOtherEnd(Update update, Subscriber subscriber)
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

        private void QueueConnection(Update update, Subscriber subscriber)
        {
            if (!Supporters.Any())
                TelegramClient.SendTextMessageAsync(subscriber.ChatId,
                    "We're sorry there is no available supporter at this time :(");
            //else if (IsWaiting(subscriber))
            //{
            //    TelegramClient.SendTextMessageAsync(subscriber.ChatId,
            //        "We're sorry, you're still on the waiting list :( You know what they say, Patience is the key to Everything :)");
            //}
            else
            {
                var supporter = SelectSupporter();

                var connection = new Connection
                {
                    BotInstanceId = Id,
                    UserChatId = subscriber.ChatId,
                    SupporterChatId = supporter.ChatId,
                };

                if (HasCurrentConnection(supporter.ChatId) && !HasCurrentConnection(subscriber.ChatId))
                {
                    //supporter.AddWaiter(TelegramClient, subscriber);
                }
                else
                {
                    if (HasCurrentConnection(subscriber.ChatId))
                        TelegramClient.SendTextMessageAsync(subscriber.ChatId,
                            "You are already connected. try talking to the supporter. Don't be shy kiddo");
                    else
                        AddConnection(connection);
                }
            }
        }

        private void AddConnection(Connection connection)
        {
            var connectionRepo = new ConnectionRepo(Id);
            connectionRepo.Add(connection);
        }

        private bool EndConnection(Subscriber subscriber)
        {
            Connection connection = FindUserConnection(subscriber.ChatId);

            if (connection == null)
                return false;

            var connectionRepo = new ConnectionRepo(Id);
            connectionRepo.Remove(connection);

            return true;
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