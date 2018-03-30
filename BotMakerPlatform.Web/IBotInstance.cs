using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web
{
    public interface IBotInstance
    {
        int Id { get; set; }
        ITelegramBotClient TelegramClient { get; set; }
        IEnumerable<Subscriber> Subscribers { get; set; }

        void Update(Update update, Subscriber subscriber);
    }
}