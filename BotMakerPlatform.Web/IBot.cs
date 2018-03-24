using BotMakerPlatform.Web.Models;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web
{
    public interface IBot
    {
        string UniqueName { get; }

        string Description { get; }
        
        void Update(Update update, int botId, Subscriber subscriber);
    }
}
