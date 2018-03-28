﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        public ConnectionManager()
        {
            this.connections = new List<Connection>();
        }

        public IEnumerable<Subscriber> Supporters;

        private List<Connection> connections;

        public void HandleMessage(ITelegramBotClient botClient, Update update,
            int botId, IEnumerable<Subscriber> subscribers, Subscriber subscriber)
        {
            GetAllSupporters(subscribers);

            if (IsSupporter(subscriber))
                HandleSupporterMessage(botClient, update, botId, subscribers, subscriber);
            else
                HandleUserMessage(botClient, update, botId, subscribers, subscriber);
        }

        public IEnumerable<Subscriber> GetAllSupporters(IEnumerable<Subscriber> subscribers)
        {
            return Supporters = subscribers.Where(x => x.Username.ToLower() == "ahmadierfan");
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
                EndConnection(subscriber);
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
            else
            {
                Subscriber supporter = Supporters.First();
                Connection connection = new Connection(botClient, subscriber, supporter);
                connections.Add(connection);
                connection.Start();
            }
        }

        private void EndConnection(Subscriber subscriber)
        {
            Connection connection = FindUserConnection(subscriber);
            connection.End();
            connections.Remove(connection);
        }

        private bool IsSupporter(Subscriber subscriber)
        {
            return Supporters.Contains(subscriber);

        }

        private Connection FindUserConnection(Subscriber subscriber)
        {
            foreach (var connection in connections)
            {
                if (connection.User.Username == subscriber.Username || connection.Supporter.Username == subscriber.Username)
                    return connection;
            }

            return null;
        }

        private Subscriber FindUserConnectionEnd(Subscriber subscriber)
        {
            foreach (var connection in connections)
            {
                if (connection.User.Username == subscriber.Username)
                    return connection.Supporter;
                if (connection.Supporter.Username == subscriber.Username)
                    return connection.User;
            }

            return null;
        }
    }
}