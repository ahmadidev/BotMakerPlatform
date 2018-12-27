using System.Linq;
using BotMakerPlatform.Web.Areas.StoreBot.Controllers;
using BotMakerPlatform.Web.Repo;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotMakerPlatform.Web.Areas.StoreBot
{
    public class StoreBotInstance : IBotInstance
    {
        public int Id { get; set; }
        public string Username { get; set; }
        private ITelegramBotClient TelegramClient { get; }

        public StoreBotInstance(ITelegramBotClient telegramClient)
        {
            TelegramClient = telegramClient;
        }

        public void Update(Update update, SubscriberRecord subscriberRecord)
        {
            if (update.Type != UpdateType.Message)
                return;

            var isAdmin = StoreAdminRepo.StoreAdmins.Any(x => x.ChatId == subscriberRecord.ChatId);

            if (isAdmin)
            {
                TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "Welcome admin!");
            }
            else
            {
                TelegramClient.SendTextMessageAsync(subscriberRecord.ChatId, "Select your category (masalan)...");
            }
        }
    }
}