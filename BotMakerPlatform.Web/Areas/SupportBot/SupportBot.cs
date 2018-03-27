using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotMakerPlatform.Web.Areas.SupportBot
{
    public class SupportBot : IBot
    {
        public string UniqueName => "SupportBot";

        public string Description => "A Simpel bot to support your customers!";

        public void Update(ITelegramBotClient botClient, Update update, int botId, IEnumerable<Subscriber> subscribers, Subscriber subscriber)
        {
            if (update.Type != UpdateType.MessageUpdate)
                return;
        }
    }
}