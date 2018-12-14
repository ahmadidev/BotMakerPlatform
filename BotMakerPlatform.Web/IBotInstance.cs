using BotMakerPlatform.Web.Repo;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web
{
    public interface IBotInstance
    {
        int Id { get; set; }

        void Update(Update update, SubscriberRecord subscriberRecord);
    }
}