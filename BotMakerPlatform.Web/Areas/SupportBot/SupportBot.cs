﻿using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotMakerPlatform.Web.Areas.SupportBot
{
    public class SupportBot : IBot
    {
        public string UniqueName => "SupportBot";

        public string Description => "A Simple bot to support your customers!";

        public void Update(ITelegramBotClient botClient, Update update, int botId, IEnumerable<Subscriber> subscribers, Subscriber subscriber)
        {
            if (update.Type != UpdateType.MessageUpdate)
                return;

            //KeyboardButton[] buttons = {new KeyboardButton("Yes"), new KeyboardButton("No")};
            //ReplyKeyboardMarkup markup = new ReplyKeyboardMarkup();

            //botClient.SendTextMessageAsync(subscriber.ChatId, "Choose", replyMarkup: markup);

            Web.Controllers.HomeController.LogRecords.Add(subscriber.Username + " : " + update.Message.Text);

            ConnectionManager.Instance.HandleMessage(botClient, update, botId,  subscribers, subscriber);
        }
    }
}