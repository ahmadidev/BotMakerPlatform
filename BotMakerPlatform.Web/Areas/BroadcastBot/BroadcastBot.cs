using BotMakerPlatform.Web.Models;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web.Areas.BroadcastBot
{
    public class BroadcastBot : IBot
    {
        public string UniqueName => "BroadcastBot";

        public string Description => "A simpel bot to broadcast your stories to your subscripbers!";

        public void Update(Update update, int botId, Subscriber subscriber)
        {

        }
    }
}