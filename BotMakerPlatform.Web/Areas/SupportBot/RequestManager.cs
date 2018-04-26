using System.Collections.Generic;
using System.Linq;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web.Areas.SupportBot
{
    public class RequestManager
    {
        public int Id { get; set; }

        public ITelegramBotClient TelegramClient { get; set; }

        protected ConnectionManager ConnectionManager { get; set; }

        public IEnumerable<Subscriber> Supporters { get; set; }

        public IEnumerable<Subscriber> WaitingQueue { get; set; }

        public RequestManager(int id, ITelegramBotClient telegramClient, IEnumerable<Subscriber> supporters)
        {
            this.Id = id;
            this.TelegramClient = telegramClient;
            this.Supporters = supporters;

            var waitersRepo = new WaitersRepo(Id);
            WaitingQueue = waitersRepo.GetAll();

            ConnectionManager = new ConnectionManager(Id, telegramClient);
        }

        public void RequestConnect(Update update, Subscriber subscriber)
        {
            if (ConnectionManager.HasCurrentConnection(subscriber.ChatId))
            {
                TelegramClient.SendTextMessageAsync(subscriber.ChatId,
                    "You are already connected. try talking to the supporter. Don't be shy kiddo");
            }
            else
            {
                AddWaiter(subscriber);
                HandleWaiters(update);
            }
        }

        public void RequestEnd(Update update, Subscriber subscriber)
        {
            if (!ConnectionManager.End(subscriber))
                TelegramClient.SendTextMessageAsync(subscriber.ChatId, "There is no connection to end");
            else
            {
                HandleWaiters(update);
            }
        }

        public void RequestMessage(Update update, Subscriber subscriber)
        {
            ConnectionManager.MessageOther(update, subscriber);
        }

        private void AddWaiter(Subscriber subscriber)
        {
            var waitersRepo = new WaitersRepo(Id);
            waitersRepo.Add(subscriber.ChatId);
        }

        private void RemoveWaiter(Subscriber subscriber)
        {
            var waitersRepo = new WaitersRepo(Id);
            waitersRepo.Remove(subscriber.ChatId);
        }

        private void HandleWaiters(Update update)
        {
            foreach (var supporter in Supporters)
            {
                if(!ConnectionManager.HasCurrentConnection(supporter.ChatId))
                    ConnectWaiter(update, supporter);
            }

            foreach (var waiter in WaitingQueue)
            {
                TelegramClient.SendTextMessageAsync(waiter.ChatId, "Please Be Patient You will be connected soon.");
            }
        }

        private void ConnectWaiter(Update update, Subscriber supporter)
        {
            if (!WaitingQueue.Any())
                return;
            
            ConnectionManager.Connect(update, WaitingQueue.Last(), supporter);
            RemoveWaiter(WaitingQueue.Last());
        }
    }
}