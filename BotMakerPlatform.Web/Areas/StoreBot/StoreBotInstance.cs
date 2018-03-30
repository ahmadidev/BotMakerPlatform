using System.Collections.Generic;
using System.Linq;
using BotMakerPlatform.Web.Areas.StoreBot.Controllers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotMakerPlatform.Web.Areas.StoreBot
{
    public class StoreBotInstance : IBotInstance
    {
        public int Id { get; set; }
        public ITelegramBotClient TelegramClient { get; set; }
        public IEnumerable<Subscriber> Subscribers { get; set; }

        public void Update(Update update, Subscriber subscriber)
        {
            if (update.Type != UpdateType.MessageUpdate)
                return;

            var isAdmin = StoreAdminRepo.StoreAdmins.Any(x => x.ChatId == subscriber.ChatId);

            if (isAdmin)
            {
                TelegramClient.SendTextMessageAsync(subscriber.ChatId, "Welcome admin!");
            }
            else
            {
                TelegramClient.SendTextMessageAsync(subscriber.ChatId, "Selec your category (masalan)...");
            }
        }
    }
}