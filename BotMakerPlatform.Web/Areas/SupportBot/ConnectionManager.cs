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

        public static ConnectionManager Instance => instance ?? (instance = new ConnectionManager());

        public List<Supporter> Supporters;

        private List<Connection> currentConnections;
        private Dictionary<Subscriber, Connection> requestedConnections;

        public ConnectionManager()
        {
            this.Supporters = new List<Supporter>();
            this.currentConnections = new List<Connection>();
            this.requestedConnections = new Dictionary<Subscriber, Connection>();
        }

        // TODO: Global Bot Client ? 

        public void HandleMessage(ITelegramBotClient botClient, Update update,
            int botId, IEnumerable<Subscriber> subscribers, Subscriber subscriber)
        {
            List<Supporter> supporters = GetSupporters(botId);

            if (IsSupporter(subscriber))
                HandleSupporterMessage(botClient, update, botId, subscribers, subscriber);
            else
                HandleUserMessage(botClient, update, botId, subscribers,  subscriber, supporters.ToArray());
        }
        
        public bool IsSupporter(Subscriber subscriber)
        {
            foreach (var supporter in Supporters)
            {
                if (supporter.ChatId == subscriber.ChatId)
                    return true;
            }

            return false;
        }

        public List<Supporter> GetSupporters(int botId)
        {
            return Supporters;//.Where(x => x.BotId == botId).ToList();
        }

        private void HandleUserMessage(ITelegramBotClient botClient, Update update,
            int botId, IEnumerable<Subscriber> subscribers, Subscriber subscriber, Supporter[] supporters)
        {
            if (update.Message.Text == "/connect")
            {
                QueueConnection(botClient, update, subscriber, supporters);
            }
            else if (update.Message.Text == "/end")
            {
                if (!EndConnection(botClient, subscriber))
                {
                    if (IsWaiting(subscriber))
                        RemoveWaiter(botClient, subscriber);
                    else
                        botClient.SendTextMessageAsync(subscriber.ChatId, "There is no connection to end");
                }
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
            if (FindUserConnection(subscriber.ChatId) != null)
            {
                long userEndId = FindUserConnectionEndChatId(subscriber);
                botClient.SendTextMessageAsync(userEndId, update.Message.Text);
            }
            else
            {
                botClient.SendTextMessageAsync(subscriber.ChatId, "You have no current session.");
            }
        }

        private void QueueConnection(ITelegramBotClient botClient, Update update, Subscriber subscriber, Supporter[] supporters)
        {
            if (!supporters.Any())
                botClient.SendTextMessageAsync(subscriber.ChatId,
                    "We're sorry there is no available supporter at this time :(");
            else if (IsWaiting(subscriber))
            {
                botClient.SendTextMessageAsync(subscriber.ChatId,
                    "We're sorry, you're still on the waiting list :( You know what they say, Patience is the key to Everything :)");
            }
            else
            {
                Supporter supporter = SelectSupporter(supporters);
                Connection connection = new Connection(botClient, subscriber, supporter);

                if (HasCurrentConnection(supporter.ChatId) && !HasCurrentConnection(subscriber.ChatId))
                {
                    requestedConnections.Add(subscriber, connection);
                    supporter.AddWaiter(botClient, subscriber);
                }
                else
                {
                    if (HasCurrentConnection(subscriber.ChatId))
                        botClient.SendTextMessageAsync(subscriber.ChatId,
                            "You are already connected. try talking to the supporter. Don't be shy kiddo");
                    else
                        AddConnection(connection);
                }
            }
        }

        private void RemoveWaiter(ITelegramBotClient botClient, Subscriber waiter)
        {
            if (!IsWaiting(waiter))
                return; ;

            botClient.SendTextMessageAsync(waiter.ChatId,
                "You were just ready to connect. So bad you quit so early :( maybe next time budd. ( just kidding you had a loooong way to go :D)");

            requestedConnections[waiter].Supporter.RemoveWaiter(botClient, waiter);
            requestedConnections.Remove(waiter);
        }

        private void AddConnection(Connection connection)
        {
            currentConnections.Add(connection);
            connection.Start();
        }

        private Connection WaiterRequestedConnection(Subscriber waiter)
        {
            if (!requestedConnections.ContainsKey(waiter))
                return null;
            return requestedConnections[waiter];
        }

        private bool EndConnection(ITelegramBotClient botClient, Subscriber subscriber)
        {
            Connection connection = FindUserConnection(subscriber.ChatId);

            if (connection == null)
                return false;

            connection.End();
            currentConnections.Remove(connection);

            Supporter supporter = connection.Supporter;

            Subscriber firstWaiter = supporter.GetFirstWaiter();

            if (firstWaiter != null)
            {
                AddConnection(requestedConnections[firstWaiter]);
                requestedConnections.Remove(firstWaiter);
            }

            supporter.MessageWaiters(botClient);

            return true;
        }


        private bool IsWaiting(Subscriber subscriber)
        {
            return requestedConnections.ContainsKey(subscriber);
        }

        private Supporter SelectSupporter(Supporter[] supporters)
        {
            if (!supporters.Any())
                return null;

            Supporter minWaiterCountSupporter = supporters[0];

            foreach (var supporter in supporters)
            {
                int supporterCount = HasCurrentConnection(supporter.ChatId) ? supporter.WaitingList.Count + 1 : supporter.WaitingList.Count;
                int minWaiterSupporterCount = HasCurrentConnection(minWaiterCountSupporter.ChatId) ? minWaiterCountSupporter.WaitingList.Count + 1 : minWaiterCountSupporter.WaitingList.Count;

                if (minWaiterSupporterCount > supporterCount)
                    minWaiterCountSupporter = supporter;
            }

            return minWaiterCountSupporter;
        }

        private Connection FindUserConnection(long subscriberChatId)
        {
            foreach (var connection in currentConnections)
            {
                if (connection.User.ChatId == subscriberChatId || connection.Supporter.ChatId == subscriberChatId)
                    return connection;
            }

            return null;
        }

        private bool HasCurrentConnection(long subscriberChatId)
        {
            foreach (var connection in currentConnections)
            {
                if (connection.User.ChatId == subscriberChatId || connection.Supporter.ChatId == subscriberChatId)
                    return true;
            }

            return false;
        }

        private long FindUserConnectionEndChatId(Subscriber subscriber)
        {
            foreach (var connection in currentConnections)
            {
                if (connection.User.ChatId == subscriber.ChatId)
                    return connection.Supporter.ChatId;
                if (connection.Supporter.ChatId == subscriber.ChatId)
                    return connection.User.ChatId;
            }

            return 0;
        }
    }
}