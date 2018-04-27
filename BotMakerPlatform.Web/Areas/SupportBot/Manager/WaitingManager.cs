using System.Linq;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using BotMakerPlatform.Web.Repo;
using Telegram.Bot;

namespace BotMakerPlatform.Web.Areas.SupportBot.Manager
{
    public class WaitingManager
    {
        private WaitingQueueRepo WaitingQueueRepo { get; }
        private ConnectionManager ConnectionManager { get; }
        private ITelegramBotClient TelegramClient { get; }
        private SubscriberRepo SubscriberRepo { get; }

        public WaitingManager(
            WaitingQueueRepo waitingQueueRepo,
            ConnectionManager connectionManager,
            ITelegramBotClient telegramClient,
            SubscriberRepo subscriberRepo,
            ConnectionNotifier connectionNotifier)
        {
            WaitingQueueRepo = waitingQueueRepo;
            ConnectionManager = connectionManager;
            TelegramClient = telegramClient;
            SubscriberRepo = subscriberRepo;

            connectionNotifier.NotifyOnCustomerDisconnect(CustomerDisconnected);
        }

        public void AddToQueue(Subscriber customer)
        {
            if (ConnectionManager.HasCustomerConnection(customer))
            {
                TelegramClient.SendTextMessageAsync(customer.ChatId, "You are already connected.");
                return;
            }

            if (WaitingQueueRepo.HasWaiter(customer))
            {
                TelegramClient.SendTextMessageAsync(customer.ChatId, $"You're Number {WaitingQueueRepo.GetPosition(customer)} In Queue.");
                return;
            }

            if (!WaitingQueueRepo.Any())
            {
                if (!ConnectionManager.TryConnect(customer))
                {
                    WaitingQueueRepo.Enqueue(customer);
                    TelegramClient.SendTextMessageAsync(customer.ChatId, $"You're Number {WaitingQueueRepo.GetPosition(customer)} In Queue.");
                }
            }
            else
            {
                WaitingQueueRepo.Enqueue(customer);
                TelegramClient.SendTextMessageAsync(customer.ChatId, $"You're Number {WaitingQueueRepo.GetPosition(customer)} In Queue.");
            }
        }

        public void CustomerDisconnected()
        {
            var customerChatId = WaitingQueueRepo.Dequeue();

            if (customerChatId == default(long))
                return;

            var customer = SubscriberRepo.GetByChatId(customerChatId);
            ConnectionManager.TryConnect(customer);

            var waitersChatIds = WaitingQueueRepo.GetAll().ToList();

            foreach (var waiterChatId in waitersChatIds)
                TelegramClient.SendTextMessageAsync(waiterChatId, $"You're Number {waitersChatIds.IndexOf(waiterChatId) + 1} In Queue.");
        }
    }
}