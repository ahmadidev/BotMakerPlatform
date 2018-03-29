using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web.Areas.SupportBot
{
    public class Connection
    {
        public Subscriber User { get; }
        public Supporter Supporter { get; }

        private readonly ITelegramBotClient botClient;

        // private int timeOutDuration; // In miliseconds

        public Connection(ITelegramBotClient botClient, Subscriber user, Supporter supporter)
        {
            User = user;
            Supporter = supporter;
            this.botClient = botClient;
        }

        public void Start()
        {
            botClient.SendTextMessageAsync(User.ChatId, "You are connected to supporter : " + Supporter.Username);
            botClient.SendTextMessageAsync(Supporter.ChatId, "You are connected to user : " + User.Username);
        }

        public void End()
        {
            botClient.SendTextMessageAsync(User.ChatId, "Your current session with " + Supporter.Username + " has ended").Wait();
            botClient.SendTextMessageAsync(Supporter.ChatId, "Your current session with " + User.Username + " has ended").Wait();
        }
    }
}