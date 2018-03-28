using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web.Areas.SupportBot
{
    public class ConnectionManager
    {
        private static ConnectionManager instance;

        public static ConnectionManager Instance()
        {
            return instance ?? (instance = new ConnectionManager());
        }

        public List<Supporter> Supporters;
        private List<Connection> currentConnections;
        private Dictionary<Subscriber, Connection> requestedConnections;

        public ConnectionManager()
        {
            this.Supporters = new List<Supporter>();
            this.currentConnections = new List<Connection>();
            this.requestedConnections = new Dictionary<Subscriber, Connection>();
        }

        public void HandleMessage(ITelegramBotClient botClient, Update update,
            int botId, IEnumerable<Subscriber> subscribers, Subscriber subscriber)
        {
            GetAllSupporters(subscribers);

            if (IsSupporter(subscriber))
                HandleSupporterMessage(botClient, update, botId, subscribers, subscriber);
            else
                HandleUserMessage(botClient, update, botId, subscribers, subscriber);
        }

        public List<Supporter> GetAllSupporters(IEnumerable<Subscriber> subscribers)
        {
            IEnumerable<Subscriber> supporters = subscribers.Where(x => x.Username != null && x.Username.ToLower() == "ahmadierfan");

            foreach (var subscriber in supporters)
                if (!IsSupporter(subscriber))
                    Supporters.Add(new Supporter(subscriber));

            return Supporters;
        }

        private void HandleUserMessage(ITelegramBotClient botClient, Update update,
            int botId, IEnumerable<Subscriber> subscribers, Subscriber subscriber)
        {
            if (update.Message.Text == "/connect")
            {
                QueueConnection(botClient, update, subscriber);
            }
            else if (update.Message.Text == "/end")
            {
                if (!EndConnection(botClient, subscriber))
                    botClient.SendTextMessageAsync(subscriber.ChatId, "There is no connection to end");
            }
            else
            {
                MessageOtherEnd(botClient, update, subscriber);
            }
        }

        private void HandleSupporterMessage(ITelegramBotClient botClient, Update update,
            int botId, IEnumerable<Subscriber> subscribers, Subscriber supporter)
        {
            MessageOtherEnd(botClient, update, supporter);
        }

        private void MessageOtherEnd(ITelegramBotClient botClient, Update update, Subscriber subscriber)
        {
            if (FindUserConnection(subscriber) != null)
            {
                Subscriber userEnd = FindUserConnectionEnd(subscriber);
                botClient.SendTextMessageAsync(userEnd.ChatId, update.Message.Text);
            }
            else
            {
                botClient.SendTextMessageAsync(subscriber.ChatId, "You have no current session.");
            }
        }

        private void QueueConnection(ITelegramBotClient botClient, Update update, Subscriber subscriber)
        {
            if (!Supporters.Any())
                botClient.SendTextMessageAsync(subscriber.ChatId,
                    "We're sorry there is no available supporter at this time :(");
            else if (IsWaiting(subscriber))
            {
                botClient.SendTextMessageAsync(subscriber.ChatId,
                    "We're sorry, you're still on the waiting list :( You know what they say, Patience is the key to Everything :)");
            }
            else
            {
                Supporter supporter = SelectSupporter();
                Connection connection = new Connection(botClient, subscriber, supporter);

                if (HasCurrentConnection(supporter.Subscriber))
                {
                    requestedConnections.Add(subscriber, connection);
                    supporter.AddWaiter(subscriber);
                    MessageWaiters(botClient);
                }
                else
                {
                    AddConnection(connection);
                }
            }
        }

        private void AddConnection(Connection connection)
        {
            currentConnections.Add(connection);
            connection.Start();
        }


        private void MessageWaiters(ITelegramBotClient botClient)
        {
            foreach (var waiter in requestedConnections.Keys)
            {
                int numberInLine = WaiterRequestedConnection(waiter).Supporter.WaitingList.Count();
                botClient.SendTextMessageAsync(waiter.ChatId,
                    "You're number " + numberInLine + " in line, Thank you for your patience :)");

            }
        }

        private Connection WaiterRequestedConnection(Subscriber waiter)
        {
            if (!requestedConnections.ContainsKey(waiter))
                return null;
            return requestedConnections[waiter];
        }

        private bool EndConnection(ITelegramBotClient botClient, Subscriber subscriber)
        {
            Connection connection = FindUserConnection(subscriber);

            if (connection == null)
                return false;

            connection.End();
            currentConnections.Remove(connection);

            foreach (var supporter in Supporters)
            {
                Subscriber firstWaiter = supporter.GetFirstWaiter();

                if (firstWaiter == null)
                    continue;

                AddConnection(requestedConnections[firstWaiter]);
                requestedConnections.Remove(firstWaiter);
            }

            MessageWaiters(botClient);

            return true;
        }


        private bool HasCurrentConnection(Subscriber subscriber)
        {
            return FindUserConnection(subscriber) != null;
        }

        private bool IsSupporter(Subscriber subscriber)
        {
            foreach (var supporter in Supporters)
            {
                if (supporter.Subscriber.ChatId == subscriber.ChatId)
                    return true;
            }

            return false;
        }

        private bool IsWaiting(Subscriber subscriber)
        {
            return requestedConnections.ContainsKey(subscriber);
        }

        private Supporter SelectSupporter()
        {
            if (!Supporters.Any())
                return null;

            Supporter minWaiterCountSupporter = Supporters[0];

            foreach (var supporter in Supporters)
            {
                if (minWaiterCountSupporter.WaitingList.Count() > supporter.WaitingList.Count())
                    minWaiterCountSupporter = supporter;
            }

            return minWaiterCountSupporter;
        }

        private Connection FindUserConnection(Subscriber subscriber)
        {
            foreach (var connection in currentConnections)
            {
                if (connection.User.ChatId == subscriber.ChatId || connection.Supporter.Subscriber.ChatId == subscriber.ChatId)
                    return connection;
            }

            return null;
        }

        private Subscriber FindUserConnectionEnd(Subscriber subscriber)
        {
            foreach (var connection in currentConnections)
            {
                if (connection.User.ChatId == subscriber.ChatId)
                    return connection.Supporter.Subscriber;
                if (connection.Supporter.Subscriber.ChatId == subscriber.ChatId)
                    return connection.User;
            }

            return null;
        }
    }
}