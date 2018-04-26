using System.Collections.Generic;
using System.Linq;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
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

        public IEnumerable<Subscriber> Supporters { get; set; }

        public ConnectionManager ConnectionManager { get; set; }

        public void Update(Update update, Subscriber subscriber)
        {
            if (update.Type != UpdateType.MessageUpdate)
                return;

            var supporterRepo = new SupporterRepo(Id);
            Supporters = supporterRepo.GetAll();

            ConnectionManager = new ConnectionManager(Id, TelegramClient);

            Web.Controllers.HomeController.LogRecords.Add(subscriber.Username + " : " + update.Message.Text);

            if (IsSupporter(subscriber))
                HandleSupporterMessage(update, subscriber);
            else
                HandleUserMessage(update, subscriber);
        }

        public bool IsSupporter(Subscriber subscriber)
        {
            return Supporters.Any(supporter => supporter.ChatId == subscriber.ChatId);
        }

        private void HandleUserMessage(Update update, Subscriber subscriber)
        {
            if (update.Message.Text == "/connect")
            {
                ConnectionManager.Connect(update, subscriber, Supporters.First());
            }
            else if (update.Message.Text == "/end")
            {
                if (!ConnectionManager.End(subscriber))
                {
                    TelegramClient.SendTextMessageAsync(subscriber.ChatId, "There is no connection to end");
                }
            }
            else
            {
                ConnectionManager.MessageOther(update, subscriber);
            }
        }

        private void HandleSupporterMessage(Update update, Subscriber supporter)
        {
            ConnectionManager.MessageOther(update, supporter);
        }
    }
}