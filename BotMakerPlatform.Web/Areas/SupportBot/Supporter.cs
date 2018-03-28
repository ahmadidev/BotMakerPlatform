using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Telegram.Bot;

namespace BotMakerPlatform.Web.Areas.SupportBot
{
    public class Supporter
    {
        public Subscriber Subscriber { get; private set; }

        public List<Subscriber> WaitingList { get; private set; }

        public Supporter(Subscriber subscriber)
        {
            Subscriber = subscriber;
            WaitingList = new List<Subscriber>();
        }

        public void AddWaiter(ITelegramBotClient botClient, Subscriber waiter)
        {
            if (!WaitingList.Contains(waiter))
                WaitingList.Add(waiter);

            botClient.SendTextMessageAsync(waiter.ChatId,
                "You're number " + WaitingList.Count + " in line, Thank you for your patience :)");

            botClient.SendTextMessageAsync(Subscriber.ChatId,
                "You have " + WaitingList.Count + " customers in line, Thank you for your knowledge :)");
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
                botClient.SendTextMessageAsync(WaitingList[i].ChatId,
                    "You're number " + i + 1 + " in line, Thank you for your patience :)");
            }
        }
    }
}