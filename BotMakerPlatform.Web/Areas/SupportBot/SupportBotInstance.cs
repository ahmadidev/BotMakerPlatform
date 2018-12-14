using BotMakerPlatform.Web.Areas.SupportBot.Manager;
using BotMakerPlatform.Web.Areas.SupportBot.Repo;
using BotMakerPlatform.Web.Repo;
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
        private StateManager StateManager { get; }
        private ITelegramBotClient TelegramClient { get; }

        public SupportBotInstance(
            WaitingManager waitingManager,
            SupporterRepo supporterRepo,
            SettingRepo settingRepo,
            ConnectionManager connectionManager,
            StateManager stateManager,
            ITelegramBotClient telegramClient)
        {
            WaitingManager = waitingManager;
            SupporterRepo = supporterRepo;
            SettingRepo = settingRepo;
            ConnectionManager = connectionManager;
            StateManager = stateManager;
            TelegramClient = telegramClient;
        }

        public void Update(Update update, SubscriberRecord subscriberRecord)
        {
            if (update.Type != UpdateType.Message)
                return;

            if (SupporterRepo.IsSupporter(subscriberRecord))
                HandleSupporterMessage(update, subscriberRecord);
            else
                HandleCustomerMessage(update, subscriberRecord);
        }

        private void HandleSupporterMessage(Update update, SubscriberRecord supporter)
        {
            ConnectionManager.Direct(supporter, update);
        }

        private void HandleCustomerMessage(Update update, SubscriberRecord customer)
        {
            switch (update.Message.Text)
            {
                case StateManager.Keyboards.StartCommand:
                    const string defaultWelcomeMessage = "Welcome to your support!\nWe never leave you alone😊";
                    TelegramClient.SendTextMessageAsync(customer.ChatId, SettingRepo.GetWelcomeMessage() ?? defaultWelcomeMessage,
                        replyMarkup: StateManager.GetCustomerReplyKeyboardMarkup(customer));
                    break;
                case StateManager.Keyboards.ConnectCommand:
                    WaitingManager.AddToQueue(customer);
                    break;
                case StateManager.Keyboards.CancelCommand:
                    WaitingManager.Cancel(customer);
                    break;
                case StateManager.Keyboards.DisconnectCommand:
                    ConnectionManager.Disconnect(customer);
                    break;
                default:
                    ConnectionManager.Direct(customer, update);
                    break;
            }
        }
    }
}