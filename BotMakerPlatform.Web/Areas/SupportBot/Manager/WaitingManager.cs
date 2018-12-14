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
        private StateManager StateManager { get; }
        private ITelegramBotClient TelegramClient { get; }
        private SubscriberRepo SubscriberRepo { get; }

        public WaitingManager(
            WaitingQueueRepo waitingQueueRepo,
            ConnectionManager connectionManager,
            ITelegramBotClient telegramClient,
            SubscriberRepo subscriberRepo,
            StateManager stateManager,
            ConnectionNotifier connectionNotifier)
        {
            WaitingQueueRepo = waitingQueueRepo;
            ConnectionManager = connectionManager;
            TelegramClient = telegramClient;
            SubscriberRepo = subscriberRepo;
            StateManager = stateManager;

            connectionNotifier.NotifyOnCustomerDisconnect(CustomerDisconnected);
        }

        public void AddToQueue(SubscriberRecord customer)
        {
            if (ConnectionManager.HasCustomerConnection(customer))
            {
                TelegramClient.SendTextMessageAsync(customer.ChatId, "You are already connected.",
                    replyMarkup: StateManager.GetCustomerReplyKeyboardMarkup(customer));
                return;
            }

            if (WaitingQueueRepo.HasWaiter(customer))
            {
                TelegramClient.SendTextMessageAsync(customer.ChatId, $"You're Number {WaitingQueueRepo.GetPosition(customer)} In Queue.",
                    replyMarkup: StateManager.GetCustomerReplyKeyboardMarkup(customer));
                return;
            }

            if (!WaitingQueueRepo.Any())
            {
                if (!ConnectionManager.TryConnect(customer))
                {
                    WaitingQueueRepo.Enqueue(customer);
                    TelegramClient.SendTextMessageAsync(customer.ChatId, $"You're Number {WaitingQueueRepo.GetPosition(customer)} In Queue.",
                        replyMarkup: StateManager.GetCustomerReplyKeyboardMarkup(customer));
                }
            }
            else
            {
                WaitingQueueRepo.Enqueue(customer);
                TelegramClient.SendTextMessageAsync(customer.ChatId, $"You're Number {WaitingQueueRepo.GetPosition(customer)} In Queue.",
                    replyMarkup: StateManager.GetCustomerReplyKeyboardMarkup(customer));
            }
        }

        public void Cancel(SubscriberRecord customer)
        {
            if (!WaitingQueueRepo.HasWaiter(customer))
            {
                TelegramClient.SendTextMessageAsync(customer.ChatId, "You're not In Queue.",
                    replyMarkup: StateManager.GetCustomerReplyKeyboardMarkup(customer));
                return;
            }

            var removedPosition = WaitingQueueRepo.GetPosition(customer);
            WaitingQueueRepo.Remove(customer);
            TelegramClient.SendTextMessageAsync(customer.ChatId, "You're not In Queue Anymore.",
                replyMarkup: StateManager.GetCustomerReplyKeyboardMarkup(customer));

            var waiters = WaitingQueueRepo.GetAll().ToList();
            var waitersToNotifyChatIds = waiters.Skip(removedPosition - 1);
            foreach (var chatId in waitersToNotifyChatIds)
                TelegramClient.SendTextMessageAsync(chatId, $"You're Number {waiters.IndexOf(chatId) + 1} In Queue.",
                    replyMarkup: StateManager.GetCustomerReplyKeyboardMarkup(customer));
        }

        public void CustomerDisconnected()
        {
            //TODO: Get first and if connect, then remove from queue
            var customerChatId = WaitingQueueRepo.Dequeue();

            if (customerChatId == default(long))
                return;

            var customer = SubscriberRepo.GetByChatId(customerChatId);
            ConnectionManager.TryConnect(customer);

            var waitersChatIds = WaitingQueueRepo.GetAll().ToList();

            foreach (var waiterChatId in waitersChatIds)
                TelegramClient.SendTextMessageAsync(waiterChatId, $"You're Number {waitersChatIds.IndexOf(waiterChatId) + 1} In Queue.",
                    replyMarkup: StateManager.GetCustomerReplyKeyboardMarkup(customer));
        }
    }
}