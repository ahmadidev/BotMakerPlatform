using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web
{
    public interface IBot
    {
        string UniqueName { get; }

        string Description { get; }

        void Update(ITelegramBotClient botClient, Update update, int botId, Subscriber subscriber);
    }
}
