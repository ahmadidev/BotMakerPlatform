﻿using System.Collections.Generic;
using System.Linq;
using BotMakerPlatform.Web.Areas.StoreBot.Controllers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotMakerPlatform.Web.Areas.StoreBot
{
    public class StoreBot : IBot
    {
        public string UniqueName => "StoreBot";

        public string Description => "Made online store easy. Sell with power!";

        public void Update(ITelegramBotClient botClient, Update update, int botId, IEnumerable<Subscriber> subscribers, Subscriber subscriber)
        {
            if (update.Type != UpdateType.MessageUpdate)
                return;

            var isAdmin = StoreAdminRepo.StoreAdmins.Any(x => x.ChatId == subscriber.ChatId);

            if (isAdmin)
            {
                botClient.SendTextMessageAsync(subscriber.ChatId, "Welcome admin!");
            }
            else
            {
                botClient.SendTextMessageAsync(subscriber.ChatId, "Selec your category (masalan)...");
            }
        }
    }
}