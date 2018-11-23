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
        private ITelegramBotClient TelegramClient { get; }

        public StoreBotInstance(ITelegramBotClient telegramClient)
        {
            TelegramClient = telegramClient;
        }

        public void Update(Update update, Subscriber subscriber)
        {
            if (update.Type != UpdateType.Message)
                return;

            var isAdmin = StoreAdminRepo.StoreAdmins.Any(x => x.ChatId == subscriber.ChatId);

            if (isAdmin)
            {
                TelegramClient.SendTextMessageAsync(subscriber.ChatId, "Welcome admin!");
            }
            else
            {
                TelegramClient.SendTextMessageAsync(subscriber.ChatId, "Select your category (masalan)...");
            }
        }
    }
}