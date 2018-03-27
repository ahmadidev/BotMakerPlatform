using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotMakerPlatform.Web.Areas.BroadcastBot
{
    public class BroadcastBot : IBot
    {
        public string UniqueName => "BroadcastBot";

        public string Description => "A simpel bot to broadcast your stories to your subscripbers!";

        public void Update(ITelegramBotClient botClient, Update update, int botId, IEnumerable<Subscriber> subscribers, Subscriber subscriber)
        {
            if (update.Type != UpdateType.MessageUpdate)
                return;

            botClient.SendTextMessageAsync(subscriber.ChatId, $"Dear {subscriber.FirstName} {subscriber.LastName} (@{subscriber.Username}):");

            if (update.Message.Text == "/start")
                botClient.SendTextMessageAsync(subscriber.ChatId, "Do you want to start with Broadcastbot? well, you'r welcome :)");
            else
                botClient.SendTextMessageAsync(subscriber.ChatId, "Sorry, but I'm under development. I will broadcast for you soon :)");
        }
    }
}