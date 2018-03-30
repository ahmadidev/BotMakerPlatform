using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Telegram.Bot;

namespace BotMakerPlatform.Web.Areas.SupportBot
{
    public class Supporter
    {
        public int BotId { get; private set; }

        public long ChatId { get; private set; }

        public string Username { get; private set; }


        public List<Subscriber> WaitingList { get; private set; }

        public Supporter(int botId, long chatId, string username)
        {
            BotId = botId;
            ChatId = chatId;
            Username = username;
            WaitingList = new List<Subscriber>();
        }

        public void AddWaiter(ITelegramBotClient botClient, Subscriber waiter)
        {
            if (!WaitingList.Contains(waiter))
                WaitingList.Add(waiter);

            botClient.SendTextMessageAsync(waiter.ChatId,
                "You're number " + WaitingList.Count + " in line, Thank you for your patience :)");
        }

        public void RemoveWaiter(ITelegramBotClient botClient, Subscriber waiter)
        {
            if (WaitingList.Contains(waiter))
                WaitingList.Remove(waiter);
        }

        public Subscriber GetFirstWaiter()
        {
            if (!WaitingList.Any())
                return null;

            Subscriber waiter = WaitingList[0];
            WaitingList.RemoveAt(0);
            return waiter;
        }

        public void MessageWaiters(ITelegramBotClient botClient)
        {
            for (int i = 0; i < WaitingList.Count; i++)
            {
                int line = i + 1;

                botClient.SendTextMessageAsync(WaitingList[i].ChatId,
                    "You're number " + line + " in line, Thank you for your patience :)");
            }

            botClient.SendTextMessageAsync(ChatId,
                "You have " + WaitingList.Count + " customers in line, Thank you for your knowledge :)");
        }
    }
}