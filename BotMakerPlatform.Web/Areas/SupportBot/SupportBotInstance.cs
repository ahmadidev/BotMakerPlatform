using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotMakerPlatform.Web.Areas.SupportBot
{
    public class SupportBotInstance : IBotInstance
    {
        public int Id { get; set; }
        public ITelegramBotClient TelegramClient { get; set; }
        public IEnumerable<Subscriber> Subscribers { get; set; }

        public void Update(Update update, Subscriber subscriber)
        {
            if (update.Type != UpdateType.MessageUpdate)
                return;

            Web.Controllers.HomeController.LogRecords.Add(subscriber.Username + " : " + update.Message.Text);

            ConnectionManager.Instance.HandleMessage(TelegramClient, update, Id, Subscribers, subscriber);
        }
    }
}