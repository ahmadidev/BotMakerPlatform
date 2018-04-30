using System.Linq;
using BotMakerPlatform.Web.Areas.SupportBot.Manager;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotMakerPlatform.Web.Areas.SupportBot
{
    public class SupportBotInstance : IBotInstance
    {
        public int Id { get; set; }

        private WaitingManager WaitingManager { get; }
        private SupporterRepo SupporterRepo { get; }
        private SettingRepo SettingRepo { get; }
        private ConnectionManager ConnectionManager { get; }
        private ITelegramBotClient TelegramClient { get; }

        public SupportBotInstance(
            WaitingManager waitingManager,
            SupporterRepo supporterRepo,
            SettingRepo settingRepo,
            ConnectionManager connectionManager,
            ITelegramBotClient telegramClient)
        {
            WaitingManager = waitingManager;
            SupporterRepo = supporterRepo;
            SettingRepo = settingRepo;
            ConnectionManager = connectionManager;
            TelegramClient = telegramClient;
        }

        public void Update(Update update, Subscriber subscriber)
        {
            if (update.Type != UpdateType.MessageUpdate)
                return;

            if (IsSupporter(subscriber))
                HandleSupporterMessage(update, subscriber);
            else
                HandleCustomerMessage(update, subscriber);
        }

        public bool IsSupporter(Subscriber customer)
        {
            return SupporterRepo.GetAll().Any(supporter => supporter.ChatId == customer.ChatId);
        }

        private void HandleSupporterMessage(Update update, Subscriber supporter)
        {
            ConnectionManager.Direct(supporter, update);
        }

        private void HandleCustomerMessage(Update update, Subscriber customer)
        {
            switch (update.Message.Text)
            {
                case "/start":
                    const string defaultWelcomeMessage = "Welcome to your support!\nWe never leave you alone😊";
                    TelegramClient.SendTextMessageAsync(customer.ChatId, SettingRepo.GetWelcomeMessage() ?? defaultWelcomeMessage);
                    break;
                case "/connect":
                    WaitingManager.AddToQueue(customer);
                    break;
                case "/cancel":
                    WaitingManager.Cancel(customer);
                    break;
                case "/end":
                    ConnectionManager.Disconnect(customer);
                    break;
                default:
                    ConnectionManager.Direct(customer, update);
                    break;
            }
        }
    }
}