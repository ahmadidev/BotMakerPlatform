using System.Linq;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotMakerPlatform.Web.Areas.SupportBot.Manager
{
    public class ConnectionManager
    {
        private SupporterRepo SupporterRepo { get; }
        private ConnectionRepo ConnectionRepo { get; }
        private ITelegramBotClient TelegramClient { get; }
        private ConnectionNotifier ConnectionNotifier { get; }

        public ConnectionManager(
            SupporterRepo supporterRepo,
            ConnectionRepo connectionRepo,
            ITelegramBotClient telegramClient,
            ConnectionNotifier connectionNotifier)
        {
            SupporterRepo = supporterRepo;
            ConnectionRepo = connectionRepo;
            TelegramClient = telegramClient;
            ConnectionNotifier = connectionNotifier;
        }

        public bool TryConnect(Subscriber customer)
        {
            var supporters = SupporterRepo.GetAll();
            var connections = ConnectionRepo.GetAll();

            if (supporters.Count() > connections.Count())
            {
                var supporter = FairSelectSupporter();
                ConnectionRepo.Add(supporter, customer);

                TelegramClient.SendTextMessageAsync(supporter.ChatId, $"You're Connected to {customer.FirstName} {customer.LastName}");
                TelegramClient.SendTextMessageAsync(customer.ChatId, "You're Connected.");

                return true;
            }

            return false;
        }

        public void Direct(Subscriber subscriber, Update update)
        {
            var partyChatId = ConnectionRepo.FindPartyChatId(subscriber);

            if (partyChatId != default(long))
                TelegramClient.SendTextMessageAsync(partyChatId, update.Message.Text);
            else
                TelegramClient.SendTextMessageAsync(subscriber.ChatId, "You have no current session.");
        }

        public void Disconnect(Subscriber customer)
        {
            var supporterChatId = ConnectionRepo.FindPartyChatId(customer);

            if (supporterChatId != default(long))
            {
                ConnectionRepo.RemoveByCustomer(customer);

                TelegramClient.SendTextMessageAsync(supporterChatId, $"You're Disconnected from {customer.FirstName} {customer.LastName}");
                TelegramClient.SendTextMessageAsync(customer.ChatId, "You're Disconnected.");

                ConnectionNotifier.CustomerDisconnected();
            }
            else
            {
                TelegramClient.SendTextMessageAsync(customer.ChatId, "No session to end.");
            }
        }

        private Subscriber FairSelectSupporter()
        {
            var supporters = SupporterRepo.GetAll();
            var connections = ConnectionRepo.GetAll();

            //TODO: Fair select
            return supporters.FirstOrDefault(x => connections.All(c => c.SupporterChatId != x.ChatId));
        }

        public bool HasCustomerConnection(Subscriber customer)
        {
            return ConnectionRepo.FindPartyChatId(customer) != default(long);
        }
    }
}